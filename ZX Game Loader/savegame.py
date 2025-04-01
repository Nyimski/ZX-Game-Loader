import sys
import os
import wave
import numpy as np
import sounddevice as sd
from datetime import datetime
import time

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
        with open(save_status_file, 'w') as f:
            f.write(message)
    except:
        pass

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

def record_save(game_name):
    try:
        # Clear any previous control commands
        clear_control_file()
        write_status("waiting_for_signal")
        
        # Audio settings
        sample_rate = 44100
        silence_threshold = 0.05
        silence_timeout = 2
        volume_boost = 8.0
        min_recording_time = 5
        
        # Prepare save directory
        clean_name = clean_filename(game_name)
        save_dir = os.path.join(saves_dir, clean_name)
        os.makedirs(save_dir, exist_ok=True)
        
        # Recording state
        recording = []
        recording_started = False
        recording_start_time = 0
        silence_frames = 0
        
        def callback(indata, frames, time_info, status):
            nonlocal recording_started, silence_frames, recording_start_time
            current_volume = np.sqrt(np.mean(np.square(indata)))
            
            if not recording_started:
                if current_volume > silence_threshold:
                    write_status("signal_detected")
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
                write_status("recording")
        
        # Start recording
        with sd.InputStream(samplerate=sample_rate, channels=1,
                          dtype='int16', callback=callback,
                          blocksize=2048):
            while not check_stop_command():
                time.sleep(0.1)
        
        if not recording:
            write_status("no_signal_detected")
            return False
            
        write_status("processing")
        audio_data = np.concatenate(recording)
        audio_data = np.clip(audio_data * volume_boost, -32767, 32767).astype(np.int16)
        
        # Save WAV file
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        wav_path = os.path.join(save_dir, f"{clean_name}_{timestamp}.wav")
        
        with wave.open(wav_path, 'wb') as wf:
            wf.setnchannels(1)
            wf.setsampwidth(2)
            wf.setframerate(sample_rate)
            wf.writeframes(audio_data.tobytes())
        
        write_status(f"recording_complete:{wav_path}")
        return True
        
    except Exception as e:
        write_status(f"error:{str(e)}")
        return False
    finally:
        clear_control_file()

def play_save(wav_path):
    try:
        if not os.path.exists(wav_path):
            write_status("error:File not found")
            return

        write_status("playback_started")
        
        with wave.open(wav_path, 'rb') as wf:
            sample_rate = wf.getframerate()
            channels = wf.getnchannels()
            frames = wf.readframes(wf.getnframes())
            audio_data = np.frombuffer(frames, dtype=np.int16).astype(np.float32)
        
        audio_data = np.clip(audio_data * 3.0, -32767, 32767).astype(np.int16)
        
        with sd.OutputStream(samplerate=sample_rate, channels=channels,
                           dtype=np.int16) as stream:
            stream.write(audio_data)
        
        write_status("playback_complete")
            
    except Exception as e:
        write_status(f"error:{str(e)}")

def main():
    if len(sys.argv) < 3:
        print("Usage: savegame.py <command> <game_name> [wav_file]")
        return
    
    command = sys.argv[1]
    game_name = sys.argv[2]
    
    if command == "save":
        record_save(game_name)
    elif command == "load":
        if len(sys.argv) < 4:
            print("Error: Missing WAV file path for load command")
            return
        play_save(sys.argv[3])
    else:
        write_status("error:Invalid command")

if __name__ == "__main__":
    main()