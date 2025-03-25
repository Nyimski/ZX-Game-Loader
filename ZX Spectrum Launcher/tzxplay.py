import sys
import os
import argparse
import numpy
import sounddevice as sd
import struct
import time
import wave
from tzxlib.tapfile import TapHeader
from tzxlib.tzxfile import TzxFile
from tzxlib.tzxblocks import TzxbLoopStart, TzxbLoopEnd, TzxbJumpTo, TzxbPause, TzxbStopTape48k
from tzxlib.saver import TapeSaver

wavelets = {}
numpySilence = numpy.zeros(1024, dtype=numpy.float32)

# Control file path
control_file_path = os.path.join(os.getenv("TEMP"), "tzx_control.txt")

# Current block file path
current_block_file = os.path.join(os.getenv("TEMP"), "current_block.txt")

# Total blocks file path
total_blocks_file = os.path.join(os.getenv("TEMP"), "total_blocks.txt")

# Next block file path
next_block_file = os.path.join(os.getenv("TEMP"), "next_block.txt")

def get_default_audio_device():
    """Retrieve the system's default output audio device."""
    devices = sd.query_devices()
    default_output = sd.default.device[1]  # Default output device index
    return default_output

def wavelet(length, level, sine=False, npy=True):
    type = (length, level)
    if type in wavelets:
        return wavelets[type]
    sign = 1 if level else -1
    amp = sign * (32000 / 32767)  # Normalized amplitude
    wave = numpy.full(length, amp, dtype=numpy.float32)
    wavelets[type] = wave
    return wave

def check_control_file():
    """Check the control file for pause/resume/stop/rewind commands."""
    if os.path.exists(control_file_path):
        with open(control_file_path, "r") as f:
            command = f.read().strip()
        os.remove(control_file_path)
        return command
    return None

def streamAudio(tzx: TzxFile, rate=44100, stopAlways=False, stop48k=False, sine=False, cpufreq=3500000, verbose=False):
    saver = TapeSaver(cpufreq)
    block = 0
    repeatBlock = None
    repeatCount = None
    currentSampleTime = 0
    realTimeNs = 0
    paused = False

    if os.path.exists(current_block_file):
        os.remove(current_block_file)
    if os.path.exists(total_blocks_file):
        os.remove(total_blocks_file)
    if os.path.exists(next_block_file):
        os.remove(next_block_file)

    try:
        with open(total_blocks_file, "w") as f:
            f.write(str(len(tzx.blocks) - 1))
    except Exception as e:
        print(f"Error writing total blocks: {e}")

    while block < len(tzx.blocks):
        try:
            with open(current_block_file, "w") as f:
                f.write(str(block))
            with open(next_block_file, "w") as f:
                f.write(str(min(block + 1, len(tzx.blocks) - 1)))  # Never exceeds tape end
        except Exception as e:
            print(f"Error writing block files: {e}")

        command = check_control_file()
        if command == "pause":
            paused = True
        elif command == "resume":
            paused = False
        elif command == "stop":
            break
        elif command and command.startswith("rewind"):
            try:
                block = int(command.split(":")[1])
                currentSampleTime = 0
                realTimeNs = 0
                continue
            except ValueError:
                block = 0
                currentSampleTime = 0
                realTimeNs = 0
                continue
        elif command and command.startswith("jump"):
            try:
                block = max(0, min(int(command.split(":")[1]), len(tzx.blocks) - 1))
                currentSampleTime = 0
                realTimeNs = 0
                continue
            except ValueError:
                continue

        if paused:
            time.sleep(0.1)
            continue

        b = tzx.blocks[block]
        block += 1

        if isinstance(b, TzxbLoopStart):
            repeatBlock = block
            repeatCount = b.repeats()
            continue
        elif isinstance(b, TzxbLoopEnd) and repeatBlock is not None:
            repeatCount -= 1
            if repeatCount > 0:
                block = repeatBlock
                continue
            else:
                repeatBlock = None
                repeatCount = None
                continue
        elif isinstance(b, TzxbJumpTo):
            block += b.relative() - 1
            if block < 0 or block > len(tzx.blocks) - 1:
                raise IndexError('Jump to non-existing block')
            continue
        elif isinstance(b, TzxbPause) and b.stopTheTape() and stopAlways:
            break
        elif isinstance(b, TzxbStopTape48k) and stop48k:
            break

        currentLevel = False
        lastLevel = False
        for ns in b.playback(saver):
            command = check_control_file()
            if command == "pause":
                paused = True
            elif command == "resume":
                paused = False
            elif command == "stop":
                return
            elif command and command.startswith("rewind"):
                try:
                    block = int(command.split(":")[1])
                    currentSampleTime = 0
                    realTimeNs = 0
                    break
                except ValueError:
                    block = 0
                    currentSampleTime = 0
                    realTimeNs = 0
                    break
            elif command and command.startswith("jump"):
                try:
                    block = int(command.split(":")[1])
                    currentSampleTime = 0
                    realTimeNs = 0
                    break
                except ValueError:
                    continue

            if paused:
                time.sleep(0.1)
                continue

            currentLevel = not currentLevel
            if ns > 0:
                realTimeNs += ns
                newSampleTime = ((realTimeNs * rate) + 500000000) // 1000000000
                wavelen = newSampleTime - currentSampleTime
                if wavelen <= 0:
                    continue
                if currentLevel != lastLevel:
                    yield wavelet(wavelen, currentLevel, sine)
                else:
                    yield numpy.zeros(wavelen, dtype=numpy.float32)
                lastLevel = currentLevel
                currentSampleTime = newSampleTime
    print("Playback finished successfully.")

def main():
    parser = argparse.ArgumentParser(description='Playback a tzx file with pause functionality')
    parser.add_argument('file', nargs='?', type=argparse.FileType('rb'), help='TZX file')
    args = parser.parse_args()

    if args.file is None:
        parser.print_help(sys.stderr)
        sys.exit(1)

    tzx = TzxFile()
    tzx.read(args.file)

    output_device = get_default_audio_device()
    stream = streamAudio(tzx)

    try:
        with sd.OutputStream(samplerate=44100, channels=1, device=output_device, latency='high') as out:
            for b in stream:
                if b is None:
                    break
                out.write(b)
    except KeyboardInterrupt:
        print("\nPlayback stopped.")
    except Exception as e:
        print(f"\nAn error occurred: {e}")
    finally:
        print("Playback finished successfully.")

if __name__ == "__main__":
    main()