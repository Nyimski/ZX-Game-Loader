# ZX Spectrum Game Loader

ZX Spectrum Game Loader is a graphical user interface (GUI) application designed to load ZX Spectrum games onto original hardware. The application features a game list, screenshots, and instruction manuals, all displayed within the GUI. It also includes added features such as pause and rewind, with plans to add fast forward and record features in the future.

## Features
- **Game List**: Displays a list of available ZX Spectrum games.
- **Screenshots**: Shows screenshots of the selected game.
- **Instruction Manuals**: Displays the instruction manual for the selected game.
- **Pause**: Allows pausing the game playback.
- **Rewind**: Allows rewinding the game playback.
- **Instant Search**: Filter games by typing in the search box - no button needed!
- **Remember Last Game**: When selected returns to last game in list when app is reloaded.
- **Fast Forward**: *(Planned)* Allows fast-forwarding the game playback.
- **Record**: *(Planned)* Allows recording and saving game progress.

## Requirements
- **Windows 10** (64-bit) or **Windows 11** (64-bit)
- **Python** (embedded in the project)

## Installation

1. Extract the archive to a folder of your choice.

## Usage

1. Run the application (ZX Spectrum Launcher.exe).
2. Click "Settings" and add paths to your Games, Instruction Manuals, and Artwork.
3. Select a game from the list.
4. Click "Load Game" to start the game playback.
5. Use the "Pause" button to pause the game and the "Rewind" button to rewind the game playback.

*Note - Artwork and Instruction Manuals must be named exactly the same as your game file.
For example if your game file is named "1943 - The Battle of Midway (1988)(Go!)[t Pokes].tap" then the Artwork should be named "1943 - The Battle of Midway (1988)(Go!)[t Pokes].jpg" and your Instruction manual "1943 - The Battle of Midway (1988)(Go!)[t Pokes].txt"

Game files can be .tzx or .tap
Artwork can be .jpg, .png or .gif
Instruction manuals must be .txt 

## Acknowledgements

This project uses a modified version of **tzxplay.py** from **[tzxtools](https://github.com/shred/tzxtools)**, authored by **shred**, which is licensed under the **GNU General Public License v3 (GPLv3)**.

## License

This project is licensed under the **GNU General Public License v3 (GPLv3)**. See the [LICENSE](./LICENSE) file for more information.

The **tzxplay.py** file is a modified version of the original from **tzxtools**, and it is licensed under the **GPLv3 License**.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request with your improvements.

## Contact

For any questions or inquiries, please open an issue on the GitHub repository.
