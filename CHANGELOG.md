# Changelog

## v1.5.0 - 2025-06-01

### Added
- Comprehensive Edit Mode system with toggle
- Context menu for game management (rename/move/delete)
- Drag-and-drop support for games/images/manuals
- About dialog with version/license/contact info
- Updated in-app manual

### Improved
- File operation safety and error handling
- Resource management
- Process handling
- Removed DeleteTempFiles.bat; temporary file cleanup is now handled directly by Form1.vb.

## [v1.4.0] - 2025-05-22

### Control Scheme Changes
- **Revised playback controls** to better match original hardware behavior:
  - ⏹️ **Stop** now pauses playback
  - ▶️ **Play** handles resume
  - ⏏️ **Eject** terminates playback completely (replaces old Stop behavior)

### New Features
- ↪️ Added **Jump** functionality
- 🔄 Auto-scroll for game names exceeding 78 characters
- ⚡ Auto-pause during Fast Forward/Rewind operations

### UI Improvements
- Removed dedicated Pause/Resume button (streamlined into Stop/Play)
- Renamed "Set 000" to clearer "Counter Reset" label
- Updated all control descriptions and tooltips

### Technical Improvements
- Namespace standardized to `Acorn_Game_Loader`
- `UEFPLAY.PY` timing precision improved (0x0116 exact values fix cumulative drift)


## [1.3.1] - 2025-04-01
### Changed
- **Save/Load System Improvements**  
  - Replaced automatic duration-based saving with manual stop control  
  - Added Save/Stop toggle button for more precise control  
  - Removed duration configuration options  
  - Faster save/load completion without fixed duration delay  

- **Documentation Updates**  
  - Updated Instructions.txt for new manual save system  
  - Modified HelpForm to reflect interface changes  

## [1.3.0] - 2025-03-31
### Added
- **Save/Load Game Progress System**  
  - Complete audio save state functionality  
  - Status feedback during save/load operations  
  - Automatic WAV file management  

- **Help System**  
  - Integrated help form with formatted instructions  
  - Section highlighting and improved readability  
  - Dynamic content loading from Instructions.txt  

- **Menu Improvements**  
  - Added File → Exit option  
  - Reorganized menu structure (File, Settings, Help)  

### Changed
- **Tape Control**  
  - Reset block counter to 0 on game stop/change  
  - Improved block tracking accuracy  
  - Enhanced rewind/fast-forward reliability  

- **Code Quality**  
  - Process management improvements  
  - Exception handling for child processes  
  - Memory leak prevention in image loading  
  - Status message timeout system  

- **Documentation**  
  - Updated README with all current features  
  - Added save/load instructions  

## [1.2.0] - 2025-03-25
### Added
- **Fast-forward functionality**  
  - Block-perfect skipping with single-click precision  
  - Maintains tape timing integrity  
  - Works seamlessly with pause/rewind  

## [1.1.0] - 2025-03-24
### Added
- **Real-time game search**  
  - Instant filtering as you type  
  - Case-insensitive matching  
  - Multi-term support (e.g., "zon att" matches "Horizon Attack")  

### Changed
- Improved list loading performance  
- Default sort now prioritizes exact matches  

## [1.0.0] - 2025-03-24
### Added
- Initial release with TZX/TAP loading  
- Game manuals and screenshot display  
- "Remember Last Game" feature  

## [0.1.0] - 2025-03-04
### Added
- Basic prototype with tape loading functionality
- Basic game loading functionality  
