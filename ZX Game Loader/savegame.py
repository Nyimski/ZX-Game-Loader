import sys
import os
import wave
import numpy as np
import sounddevice as sd
from datetime import datetime
import time
import subprocess
import tempfile

# Constants
save_status_file = os.path.join(os.getenv("TEMP"), "save_status.txt")
save_control_file = os.path.join(os.getenv("TEMP"), "save_control.txt")
saves_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Saves")

def clean_filename(name):
    """Sanitize filenames to be filesystem-safe"""
    return "".join(c for c in name if c.isalnum() or c in (' ', '_', '-')).rstrip()

def write_status(message):
    """Write status message to file with error handling"""
    try:
        if message:
            message = message[0].upper() + message[1:]
        with open(save_status_file, 'w') as f:
            f.write(message)
    except Exception as e:
        print(f"Failed to write status: {str(e)}")

def check_stop_command():
    """Check if stop command has been issued"""
    try:
        if os.path.exists(save_control_file):
            with open(save_control_file, 'r') as f:
                return f.read().strip().lower() == "stop"
        return False
    except:
        return False

def clear_control_file():
    """Clear the control file"""
    try:
        if os.path.exists(save_control_file):
            os.remove(save_control_file)
    except:
        pass

def play_wav(file_path):
    """Play WAV file through sounddevice"""
    try:
        write_status("Loading save data")
        with wave.open(file_path, 'rb') as wf:
            sample_rate = wf.getframerate()
            channels = wf.getnchannels()
            frames = wf.readframes(wf.getnframes())
            audio_data = np.frombuffer(frames, dtype=np.int16).astype(np.float32)
        
        audio_data = np.clip(audio_data * 3.0, -32767, 32767).astype(np.int16)
        
        with sd.OutputStream(samplerate=sample_rate, channels=channels,
                           dtype=np.int16) as stream:
            stream.write(audio_data)
        
        write_status("Save loaded successfully!")
        time.sleep(3)
        write_status("Ready")
        return True
    except Exception as e:
        write_status(f"Error: WAV playback failed: {str(e)}")
        return False

def play_tzx(file_path):
    """Play TZX file directly using tzxplay.py"""
    try:
        write_status("Loading save data...")
        
        process = subprocess.Popen(
            [
                sys.executable,
                os.path.join(os.path.dirname(__file__), "tzxplay.py"),
                file_path
            ],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            creationflags=subprocess.CREATE_NO_WINDOW
        )
        
        with sd.OutputStream(samplerate=44100, channels=1, dtype=np.float32) as stream:
            while True:
                output = process.stdout.read(4096)
                if not output:
                    break
                
                try:
                    audio_data = np.frombuffer(output, dtype=np.float32)
                    stream.write(audio_data)
                except Exception as e:
                    write_status(f"Error: Audio conversion failed: {str(e)}")
                    process.terminate()
                    return False
                
                if process.poll() is not None:
                    err = process.stderr.read()
                    if err:
                        write_status(f"Error: {err.decode().strip()}")
                        return False
                    break
        
        write_status("Save loaded successfully!")
        time.sleep(3)
        write_status("Ready")
        return True
        
    except Exception as e:
        write_status(f"Error: TZX playback failed: {str(e)}")
        return False

def record_save(game_name):
    """Record audio from microphone and save as TZX"""
    try:
        clear_control_file()
        write_status("Waiting for signal")
        
        sample_rate = 44100
        silence_threshold = 0.05
        silence_timeout = 2
        volume_boost = 8.0
        min_recording_time = 5
        
        clean_name = clean_filename(game_name)
        save_dir = os.path.join(saves_dir, clean_name)
        
        # Debug output
        print(f"Creating save directory: {save_dir}")
        write_status(f"Creating save directory: {save_dir}")
        
        os.makedirs(save_dir, exist_ok=True)
        
        if not os.path.exists(save_dir):
            error_msg = f"Failed to create directory: {save_dir}"
            print(error_msg)
            write_status(error_msg)
            return False
        
        recording = []
        recording_started = False
        recording_start_time = 0
        silence_frames = 0
        
        def callback(indata, frames, time_info, status):
            nonlocal recording_started, silence_frames, recording_start_time
            current_volume = np.sqrt(np.mean(np.square(indata)))
            
            if not recording_started:
                if current_volume > silence_threshold:
                    write_status("Signal detected")
                    recording_started = True
                    recording_start_time = time.time()
                    recording.append(indata.copy())
                return
            
            if check_stop_command():
                raise sd.CallbackStop
            
            recording.append(indata.copy())
            
            if current_volume < silence_threshold:
                silence_frames += frames
                if (time.time() - recording_start_time > min_recording_time and 
                    silence_frames >= silence_timeout * sample_rate):
                    raise sd.CallbackStop
            else:
                silence_frames = 0
                write_status("Recording")
        
        with sd.InputStream(samplerate=sample_rate, channels=1,
                          dtype='int16', callback=callback,
                          blocksize=2048):
            while not check_stop_command():
                time.sleep(0.1)
        
        if not recording:
            write_status("No signal detected")
            return False
            
        write_status("Processing")
        audio_data = np.concatenate(recording)
        audio_data = np.clip(audio_data * volume_boost, -32767, 32767).astype(np.int16)
        
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        wav_path = os.path.join(save_dir, f"{clean_name}_{timestamp}.wav")
        
        print(f"Creating WAV file: {wav_path}")
        write_status(f"Creating WAV file: {wav_path}")
        
        with wave.open(wav_path, 'wb') as wf:
            wf.setnchannels(1)
            wf.setsampwidth(2)
            wf.setframerate(sample_rate)
            wf.writeframes(audio_data.tobytes())
        
        if not os.path.exists(wav_path):
            error_msg = f"Failed to create WAV file: {wav_path}"
            print(error_msg)
            write_status(error_msg)
            return False
        
        write_status("Recording complete")
        
        write_status("Processing save")
        tzx_path = wav_path.replace(".wav", ".tzx")
        
        cmd = [sys.executable, "-m", "tzxtools.tzxwav", "-o", tzx_path, wav_path]
        print(f"Executing: {' '.join(cmd)}")
        write_status(f"Executing: {' '.join(cmd)}")
        
        try:
            result = subprocess.run(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                creationflags=subprocess.CREATE_NO_WINDOW,
                check=True,
                text=True
            )
            
            print(f"Command output:\nSTDOUT: {result.stdout}\nSTDERR: {result.stderr}")
            write_status(f"Command output:\nSTDOUT: {result.stdout}\nSTDERR: {result.stderr}")
            
            if os.path.exists(tzx_path):
                os.remove(wav_path)
                write_status("Conversion complete")
                print(f"Successfully created TZX file: {tzx_path}")
                return True
            else:
                error_msg = f"TZX file not created: {tzx_path}"
                print(error_msg)
                write_status(error_msg)
                return False
        except subprocess.CalledProcessError as e:
            error_msg = f"Conversion failed: {e.stderr}"
            print(error_msg)
            write_status(error_msg)
            return False
        
    except Exception as e:
        error_msg = f"Error: {str(e)}"
        print(error_msg)
        write_status(error_msg)
        return False
    finally:
        clear_control_file()

def main():
    if len(sys.argv) < 3:
        print("Usage: savegame.py <command> <game_name> [file]")
        print("Commands: save, load, loadtzx")
        return
    
    command = sys.argv[1].lower()
    game_name = sys.argv[2]
    
    if command == "save":
        record_save(game_name)
    elif command == "load":
        if len(sys.argv) < 4:
            print("Error: Missing file path for load command")
            return
        play_wav(sys.argv[3])
    elif command == "loadtzx":
        if len(sys.argv) < 4:
            print("Error: Missing file path for loadtzx command")
            return
        play_tzx(sys.argv[3])
    else:
        write_status("Error: Invalid command")

if __name__ == "__main__":
    main()