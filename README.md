# ZX Game Loader

A comprehensive GUI application for loading ZX Spectrum games onto original hardware with advanced tape control and save state functionality.

## Features

### Core Functionality
- **Game Browser** with instant search (supports multi-term filtering)
- **Screenshot Viewer** (supports JPG, PNG, GIF)
- **Manual Viewer** (TXT format)
- Supports both **.tzx** and **.tap** game files
- **New in v1.5.0**: Edit Mode with game management tools

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

- **New in v1.5.0**: Drag-and-drop support for adding games/images/manuals
- **New in v1.5.0**: Right-click context menu for game management
- **New in v1.5.0**: Updated in-app manual with Edit Mode documentation
- **New in v1.5.0**: About dialog with version/license info

## Requirements
- **Windows 10/11** (64-bit)
- **.NET Framework 8.0**
- **Python** (embedded in distribution)


## Installation
1. Download latest release
2. Extract to preferred location
3. Run `ZX Game Loader.exe`
4. Optional - Download Assets.zip (Contains screenshots and game manuals/info rename your tzx/tap files to match)

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

### Edit Mode Features (New in v1.5.0)
1. Enable **Edit Mode** (Menu → Edit → Editor On)
2. Select the game and right-click for options:
   - **Rename**: Change game name while keeping all associated files
   - **Move**: Relocate game to different folder
   - **Delete**: Remove game (sent to Recycle Bin)
3. Drag and drop files directly onto:
   - Game list (to add .tzx/.tap files)
   - Image panel (to update screenshots)
   - Manual panel (to update documentation)

### Saving Progress
1. In-game, select or type the save command, it will then say "Press Record" or something similar.
2. Click SAVE in the Game Saves section (button changes to STOP).
3. Wait for "Waiting for signal..." message.
4. Wait for save to finish on the Spectrum.
5. Click STOP to end recording.
6. Application will automatically:
Detect the signal
Save as timestamped .wav file

### Loading Progress
1. In-game, select or type the load command.
2. Click LOAD in Game Saves and choose the desired .wav file.
3. File should begin loading on the Spectrum.

## File Naming Convention
All supporting files must match game filename exactly:
- Game: `GameName.tzx` or `GameName.tap`
- Image: `GameName.jpg/png/gif`
- Manual: `GameName.txt`

## Technical Details
- Uses modified **tzxplay.py** from [tzxtools](https://github.com/shred/tzxtools) by shred
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
- **Edit Mode features not appearing?** Ensure:
  - Edit Mode is enabled (Menu → Edit → Editor On)
  - You're right-clicking on a game in the list
  
## License
GNU General Public License v3 (GPLv3)

## Contributing
Bug reports and feature requests welcome via GitHub Issues.

