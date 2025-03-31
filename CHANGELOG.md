# Changelog

## [1.3.0] - 2025-03-31
### Added
- **Save/Load Game Progress System**  
  - Complete audio save state functionality  
  - Configurable recording durations (15s/30s/60s/90s)  
  - Status feedback during save/load operations  
  - Automatic WAV file management  

- **Help System**  
  - Integrated help form with formatted instructions  
  - Section highlighting and improved readability  
  - Dynamic content loading from Instructions.txt  

- **Menu Improvements**  
  - Added File → Exit option  
  - Reorganized menu structure (File, Save Duration, Settings, Help)  

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
- Basic game loading functionality  
