![OpenSAGE](/art/opensage-logo.png)
============================================================

[![Build Status](https://github.com/OpenSage/OpenSage/workflows/CI/badge.svg)](https://github.com/OpenSAGE/OpenSAGE/actions)
[![Discord Chat](https://img.shields.io/discord/398393968234332161.svg?logo=discord)](https://discord.gg/G2FhZUT)

**OpenSAGE**: a free, open source re-implementation of [SAGE](https://en.wikipedia.org/wiki/SAGE_(game_engine)), the 3D 
real time strategy (RTS) engine used in Command & Conquer™: Generals and other 
RTS titles from EA Pacific.

This project is being developed with an initial focus on Command & Conquer:
Generals and Command & Conquer: Generals Zero Hour. There is also some support for other SAGE-based games such as The Battle for Middle-Earth series. The engine is written in C# using [.NET Core](https://dotnet.microsoft.com/). The primary development target is Windows, but the engine also supports macOS and Linux (with some caveats).

We have a growing [OpenSAGE Discord](https://discord.gg/G2FhZUT) community. If you have questions about the project or can't get it working,
there's usually someone there who can help out.

## Project status
The current goal of the project is to implement enough engine features so that a player (or multiple players in multiplayer) can play a game from start to finish. We've made great progress in the past few years, but there is still a lot of work to do.

Here are some of the things we've worked on (but not necessarily finished):

*Updated 2020-08-16*

- Core engine
  - Asset loading
  - Framerate independent game loop
  - Logging
  - Game file discovery
- Asset types
  - Maps
  - Models
  - Textures
  - SAGE INI
  - WND UI
  - APT UI
- Rendering
  - Basic map rendering (terrain, roads, static models)
  - Skeletal animation
  - Water
  - Shadows (disabled by default)
- Scripting
  - Map script interpreter
  - Lua runtime
- Gameplay systems
  - Orders
  - Individual unit pathfinding
  - Building construction
  - Locomotors
- Networking
  - Order serialization
  - Reliable UDP
  - LAN lobbies
- UI
  - WND UI system (used by Generals & ZH)
  - APT UI system (used by BFME1 and later games)
  - Health bars
- Game-specific code
  - Command & Conquer: Generals
    - UI callbacks
      - Main menu
      - In-game control bar

Things that are yet to be implemented:

- Rendering
  - [ ] Shadows
      - Implemented but disabled by default because of issues.
  - [ ] Post processing (e.g SSAO, motion blur, bloom, DOF)
- Gameplay systems

## How to contribute

There are many ways to contribute to the project.

If you are a developer, you can contribute code to the project using pull requests.

## Related repositories

* [OpenSAGE/OpenSAGE.BlenderPlugin](https://github.com/OpenSAGE/OpenSAGE.BlenderPlugin) is a plugin for [Blender](https://www.blender.org/) which enables importing and exporting W3D(X) model formats used by the original SAGE engine.
* [OpenSAGE/opensage.github.io](https://github.com/OpenSAGE/opensage.github.io) contains the source code for our website and blog.

## Background

*by Time Jones (2017)*

OpenSAGE is being created by me, Tim Jones. I was at university in February 2003 when C&C Generals was first released. I spent far too much time playing it and the sequel, Zero Hour (and as a consequence, not enough time studying). In my opinion, as a near-real-world RTS game, it is still unmatched even 14 years later.

I later bought The First Decade on DVD, and it was this copy I wanted to install in Windows 10. I even bought an external DVD drive in order to do so. It wasn't straightforward, and it made me worry that one day I won't be able to play what is still my favourite RTS game.

One thing led to another, and I found myself opening `.map` files in a hex viewer. I had the idea of recreating the game, using the original assets. This appealed to me on many levels: it's a preservation of history, it satisfies my nostalgia, and it's an extreme programming challenge.

It's a vast project, and who knows how far I'll get. Hopefully we'll all have some fun along the way.

## Acknowledgements

First, I would like to thank Stephan Vedder ([feliwir](https://github.com/feliwir)) for his efforts, over several years, to understand
several of the key SAGE data formats, including `.w3d`. Without his prior work, I would have had a much harder time getting started.

DeeZire's [module list](http://www.redsys.su/mkportal/files/ModuleList.txt) has been extremely helpful in understanding all the `Object` parameters in `.ini` files.

The font used in the OpenSAGE logo was created by Dexistor371 and is available from [DeviantArt](https://dexistor371.deviantart.com/art/Command-and-Conquer-logo-font-396527879).

The sage / leaf icon used in the OpenSAGE logo was created by Monjin Friends and is licenced under [Creative Commons CC BY 3.0 US](https://creativecommons.org/licenses/by/3.0/us/). It is available [from the Noun Project](https://thenounproject.com/term/leaf/1052490/).

Finally, I want to thank the original team who built Generals and Zero Hour, because without their work, I wouldn't be doing any of this.
(As I get further into the project, I'm gaining ever greater admiration for what those people were able to achieve 14 years ago, both technically
and artistically.)

## Legal disclaimers

* This project is not affiliated with or endorsed by EA in any way. Command & Conquer is a trademark of Electronic Arts.
* This project is non-commercial. The source code is available for free and always will be.
* OpenSAGE is nowhere near playable yet, but when it is and you want to play Generals or Zero Hour with it,
  you will need to have a legally acquired installation of one of those games. OpenSAGE uses data files from the original games. 
  You can purchase [Command & Conquer: The Ultimate Collection through Origin](https://www.origin.com/twn/en-us/store/command-and-conquer/command-and-conquer-the-ultimate-collection/ultimate-collection).
* This is a blackbox re-implementation project. The code in this project was written based on reading data files, 
  and observing the game running. In some cases (for example refpack decompression) the code was written based on specs available on the Internet.
  I believe this puts the project in the clear, legally speaking. If someone at EA disagrees, please talk to me.
* If you want to contribute to this repository, your contribution must be either your own original code, or open source code with a
  clear acknowledgement of its origin. No code that was acquired through reverse engineering executable binaries will be accepted.
* No assets from the original games are included in this repo.

A note on the name: while Command & Conquer is a trademark of EA, SAGE is not (as far as I can tell, based on a US trademark search). "OpenSAGE" seems like a good way to make it clear what the project is about, without infringing on trademarks.

## Similar projects

These projects have similar goals:

* [Arda](https://github.com/feliwir/arda)
* [smx-smx/openSage](https://github.com/smx-smx/openSage)

[OpenRA](http://www.openra.net/) already does for the Westwood RTS games what I hope to do for the EA-era RTS games with OpenSAGE.
