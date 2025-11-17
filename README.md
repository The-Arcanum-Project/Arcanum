[![Lines of Code](https://tokei.rs/b1/github/XAMPPRocky/tokei)](https://github.com/Minnator/Arcanum) ![Repo Size](https://img.shields.io/github/repo-size/Minnator/Arcanum) ![GitHub release (latest by date)](https://img.shields.io/github/v/release/Minnator/Arcanum) ![GitHub all releases](https://img.shields.io/github/downloads/Minnator/Arcanum/total)

# The Arcanum Project

The **Arcanum Project** is a bundle of tools to ease the modding of Europa Universalis 5.
The tools is mainly being developed by [Minnator](https://github.com/Minnator) and [MelonCoaster](https://github.com/mel-co) with the help of [CzerstfyChlep](https://github.com/CzerstfyChlep), [zulacecek](https://github.com/zulacecek) and other contributors alike.

This tool is in active development and will become more feature rich and polished in the future.

## Quick Setup Guide
1. Backup your modfiles
2. Aquire the latest release
3. Run the `docs` command in EUV to generate the documentation data
4. Run Arcanum and enter the required mod and vanilla paths in the main menu
5. Lauch the current config and start modding.

## Editing - Quick Guide
- Any object can be selected via search or the map using different selection modes.
- Ones an object is selected it is loaded to a custom UI (NUI)
- Edit any values you want in NUI
- Before you hit `Ctrl+S` to save all changes check with the settings for saving if they are to your liking
- Save all your changes

## Implemented Features
- Smart history tree to be able to undo and redo any action taken
- Interactive map with map modes
- Detailed error detection and pinpointing to file, line and char
- Hot reloading support for mod and game files
- Support for more than **40** different game objects to be edited.

## Current limitations
- Anything from the `menu_screen/start/setup` folder can not yet be saved and thus is readonly
- No objects with the posibility of effects and triggers can be edited

## Future features
- Map editor
- Intelligent map design aides
- Automatic error correction
- Plugin support
- User defined map modes
- Map exporting
- Heightmap and normals on the interactive map

## Input and contribution:
Currently we are *not* actively looking for people to contribute, however we are open to ideas and suggestions. 
If you have any suggestions, questions or feedback feel free to reach out to us on the official [Discord Server](https://discord.gg/CXFGsEgugn)
