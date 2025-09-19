# The Arcanum Project

This project aims to build and improve upon what its predecessor [Minnator's Modforge](https://github.com/Minnator/Minnators-Modforge) has achieved.
The main difference is that this tool is meant for the game **Europa Universalis V** also known as Project Ceasar.
It is being developed by [MelonCoaster](https://github.com/mel-co) and [Minnator](https://github.com/Minnator).
We develop this in our spare freetime so the speed of progress may vary from time to time.

![Lines of Code](https://tokei.rs/b1/github/Minnator/Arcanum) ![Repo Size](https://img.shields.io/github/repo-size/Minnator/Arcanum) ![GitHub release (latest by date)](https://img.shields.io/github/v/release/Minnator/Arcanum) ![GitHub all releases](https://img.shields.io/github/downloads/Minnator/Arcanum/total)

## Structure of the tool:
The entire tool is made as modular as possible.
Many of the parts are generated automatically when compiling.
The entire pipeline consists of a handfull of powerfull engines:
- APG (Automatic Parsing Generator):
  This source generator can take simple object definitions and generate entire parsing pipelines for them.
- Errorhandling: Across the entire loading, lexing, parsing, interpreting process we gather and handle errors to provide an exact line and char pos for each error or inconsistency detected.
- NUI (Navigateable User Interface): This is our UI engine which completely automatically generates interfaces to interact with and modify any object parsed and underlines it with powerfull customization and navigation.
- AGS (Automatically Generated Saving): Another sourcegenerator which creates a list of rules and informations about each and every property and object being saved, which speeds up the runtime process of saving drastically.
- Queastor: Search everywhere. Every object and property loaded can be searched here in an instance. Even settings and UI elements can be included.

## What are we aiming for?
Minnator's Modforge already has a nice suite of tools and decent performance, but we want to shadow this:
- Shader based map rendering (almost done)
- Project files to ease editing and resume where ever you left of (almost done)
- Full plugin support to have users add or expand features how they like it (exists in theory but needs proper support across the board)
- Smart history tree to be able to undo and redo any action taken (done)
- Quick and consistent loading times (done)
- All ingame mapmodes
- Interactive map for selection and editing
- PDX map editor for all games which share the same format of province definition in a `.bmp` file.
- Mission / focus "autocompletion" and implementation aid
- Detailed error detection and pinpointing to file and line (done)
- Hot reloading support for mod and game files (done)
- Selfdocumenting codebase and features
An overview of what is currently being worked on can be seen in [this](https://github.com/users/Minnator/projects/2/views/2) project.

## Input and contribution:
Currently we are *not* looking for people to contribute, however we are open to ideas and suggestions. 
If you have any suggestions, questions or feedback feel free to reach out to us on the official [Discord Server](https://discord.gg/CXFGsEgugn)
