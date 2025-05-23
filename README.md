# ZX Game Loader

A comprehensive GUI application for loading ZX Spectrum games onto original hardware with advanced tape control and save state functionality.

## Features

### Core Functionality
- **Game Browser** with instant search (supports multi-term filtering)
- **Screenshot Viewer** (supports JPG, PNG, GIF)
- **Manual Viewer** (TXT format)
- Supports both **.tzx** and **.tap** game files

### Tape Control
- ▶️ **Play**: Start/Resume game playback
- ⏹️ **Stop**: Halt playback
- ⏏️ **Eject**: Completely end playback
- ⏪ **Rewind**: Move back 1 tape block
- ⏩ **Forward**: Jump to next tape block
- 🔢 **Block Counter**: Shows current playback position
- 0️⃣ **Counter Reset**: Mark reference point (e.g., after loading screens)
- ↪️ **Jump**: Jumps to block set by Counter Reset

### Save States
- 💾 **Save Game Progress**:
  - Records audio from Spectrum's EAR port
  - Auto-detects signal start/stop
- 📂 **Load Game Progress**:
  - Browse and select saved .wav files
  - Simulates tape loading process
  - Status feedback during operation

### Convenience Features
- **Remember Last Game**: Auto-reopens your last-played game
- **Customizable Folders**: Set paths for games, images, manuals

## Requirements
- **Windows 10/11** (64-bit)
- **Python** (embedded in distribution)

## Installation
1. Download latest release
2. Extract to preferred location
3. Run `ZX Game Loader.exe`

## Usage Guide

### First-Time Setup
1. Open **Settings** (Menu → Settings)
2. Configure folders for:
   - Games (.tzx/.tap files)
   - Images (screenshots as .jpg/.png/.gif)
   - Manuals (.txt files)
3. Enable "Remember Last Game" if desired

### Playing Games
1. Select game from list (use search to filter)
2. View screenshot and manual
3. Click **Play** to start or resume playback (after Rewind/Forward/Stop)
4. Use **Stop**, **Rewind**, **Forward** as needed

### Saving Progress
1. During gameplay, click **Save**
2. Wait for "Waiting for signal..." message
3. Play audio from Spectrum's EAR port
4. Application will automatically:
   - Detect the signal
   - Save as timestamped .wav file

### Loading Progress
1. Click **Load**
2. Select your saved .wav file
3. App will simulate tape loading

## File Naming Convention
All supporting files must match game filename exactly:
- Game: `GameName.tzx` or `GameName.tap`
- Image: `GameName.jpg/png/gif`
- Manual: `GameName.txt`

## Technical Details
- Uses modified **tzxplay.py** from [tzxtools](https://github.com/shred/tzxtools)
- Save system works with standard audio cables
- Optimized for 44.1kHz mono WAV files

## Troubleshooting
- **No sound during playback?** Check audio cable connections
- **Save/Load not working?** Ensure:
  - Adequate volume during save
  - Minimal background noise
  - Correct WAV format (44.1kHz mono)
- **Game missing from list?** Verify:
  - Correct folder location
  - Proper file extension (.tzx/.tap)
  - File integrity

## License
GNU General Public License v3 (GPLv3)

## Contributing
Bug reports and feature requests welcome via GitHub Issues.

