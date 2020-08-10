![OpenSAGE](/art/opensage-logo.png)
============================================================

[![Build Status](https://github.com/OpenSage/OpenSage/workflows/CI/badge.svg)](https://github.com/OpenSAGE/OpenSAGE/actions)
[![Discord Chat](https://img.shields.io/discord/398393968234332161.svg?logo=discord)](https://discord.gg/G2FhZUT)
<!---[![codecov](https://codecov.io/gh/OpenSAGE/OpenSAGE/branch/master/graph/badge.svg)](https://codecov.io/gh/OpenSAGE/OpenSAGE)--->

**OpenSAGE**: a free, open source re-implementation of [SAGE](https://en.wikipedia.org/wiki/SAGE_(game_engine)), the 3D 
real time strategy (RTS) engine used in Command & Conquer™: Generals and other 
RTS titles from EA Pacific.

This project is being developed with an initial focus on Command & Conquer:
Generals and Command & Conquer: Generals Zero Hour. Support for other SAGE-based
games may come later. The primary development target is Windows, with support
planned for macOS at a later date.

## Work in progress

This project is currently able to load and display most dataformats used in SAGE.
While working on multiple games in parallel the game in focus is Command & Conquer: Generals and Zero Hour.
The current focus is to *play* a game from start to finish.

Here's a rough roadmap how to get there:
* [X] modified A* pathfinding
* [ ] AI - Path finding, base building, fighting
* [ ] Physics engine
* [ ] Weapons (in progress)
* [ ] Locomotors (in progress)
* [ ] Specialpowers
* [ ] Object behaviors (in progress)
* [ ] Input (keyboard, mouse)
* [X] Network play (LAN only so far)
* [ ] Much more...

### Platforms

* [x] Windows
  * OpenGL 4.3
  * Direct3D 11
* [x] Mac
  * Metal 2 (requires macOS High Sierra)
* [x] Linux
  * OpenGL 4.3

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

## About

OpenSAGE is being created by me, Tim Jones. I was at university in February 2003 when C&C Generals was first released. I spent far too much time playing it and the sequel, Zero Hour (and as a consequence, not enough time studying). In my opinion, as a near-real-world RTS game, it is still unmatched even 14 years later.

I later bought The First Decade on DVD, and it was this copy I wanted to install in Windows 10. I even bought an external DVD drive in order to do so. It wasn't straightforward, and it made me worry that one day I won't be able to play what is still my favourite RTS game.

One thing led to another, and I found myself opening `.map` files in a hex viewer. I had the idea of recreating the game, using the original assets. This appealed to me on many levels: it's a preservation of history, it satisfies my nostalgia, and it's an extreme programming challenge.

It's a vast project, and who knows how far I'll get. Hopefully we'll all have some fun along the way.

## Community

We have a growing [OpenSAGE Discord](https://discord.gg/G2FhZUT) community. If you have questions about the project or can't get it working,
there's usually someone there who can help out.

## Acknowledgements

First, I would like to thank Stephan Vedder ([feliwir](https://github.com/feliwir)) for his efforts, over several years, to understand
several of the key SAGE data formats, including `.w3d`. Without his prior work, I would have had a much harder time getting started.

DeeZire's [module list](http://www.redsys.su/mkportal/files/ModuleList.txt) has been extremely helpful in understanding all the `Object` parameters in `.ini` files.

The font used in the OpenSAGE logo was created by Dexistor371 and is available from [DeviantArt](https://dexistor371.deviantart.com/art/Command-and-Conquer-logo-font-396527879).

The sage / leaf icon used in the OpenSAGE logo was created by Monjin Friends and is licenced under [Creative Commons CC BY 3.0 US](https://creativecommons.org/licenses/by/3.0/us/). It is available [from the Noun Project](https://thenounproject.com/term/leaf/1052490/).

Finally, I want to thank the original team who built Generals and Zero Hour, because without their work, I wouldn't be doing any of this.
(As I get further into the project, I'm gaining ever greater admiration for what those people were able to achieve 14 years ago, both technically
and artistically.)

## Similar projects

These projects have similar goals:

* [Arda](https://github.com/feliwir/arda)
* [smx-smx/openSage](https://github.com/smx-smx/openSage)

[OpenRA](http://www.openra.net/) already does for the Westwood RTS games what I hope to do for the EA-era RTS games with OpenSAGE.
