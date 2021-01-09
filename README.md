# ![OpenSAGE](/art/opensage-logo.png)

[![Build Status](https://github.com/OpenSage/OpenSage/workflows/CI/badge.svg)](https://github.com/OpenSAGE/OpenSAGE/actions)
[![Discord Chat](https://img.shields.io/discord/398393968234332161.svg?logo=discord)](https://discord.gg/G2FhZUT)

**OpenSAGE** is a free & open source re-implementation of [SAGE](<https://en.wikipedia.org/wiki/SAGE_(game_engine)>), the 3D
real time strategy (RTS) engine used in Command & Conquer™: Generals and other
RTS titles from EA Pacific.

This project is being developed with an initial focus on Command & Conquer™:
Generals and Command & Conquer™: Generals Zero Hour. There is also some support for other SAGE-based games such as The Battle for Middle-Earth™ series. The engine is written in C# using [.NET Core](https://dotnet.microsoft.com/). The primary development target is Windows, but the engine also supports macOS and Linux (with some caveats).

OpenSAGE has similar goals to [OpenRA](https://github.com/OpenRA/OpenRA), which is as similar engine reimplementation project in C#, but for older titles in the C&C series.

We have a growing [OpenSAGE Discord](https://discord.gg/G2FhZUT) community. If you have questions about the project or can't get it working,
there's usually someone there who can help out. You should also check out [our website](https://opensage.github.io/), where we have written dozens of blog posts about the development of the engine.

You can read more about the project's background [here](./docs/background.md) (2017).
## Project status

The current goal of the project is to implement enough engine features so that a player (or multiple players in multiplayer) can play a game from start to finish. We've made great progress in the past few years, but there is still a lot of work to do.

At the time of writing, you can start a few of the supported games, get into a skirmish match or even a LAN lobby, and play with the fundamental gameplay elements. However, we are still missing some important pieces that would make any of the games actually playable.


## Related repositories

- [OpenSAGE/OpenSAGE.BlenderPlugin](https://github.com/OpenSAGE/OpenSAGE.BlenderPlugin) is a plugin for [Blender](https://www.blender.org/) which enables importing and exporting W3D(X) model formats used by the original SAGE engine.
- [OpenSAGE/opensage.github.io](https://github.com/OpenSAGE/opensage.github.io) contains the source code for our website and blog.
- [OpenSAGE/Docs](https://github.com/OpenSAGE/Docs) contains some documentation we've written about the SAGE engine. You can see a rendered version at [ReadTheDocs](https://opensage.readthedocs.io/).

## How to contribute

### Developers

We accept pull requests! You can check our issue tracker for inspiration on what to work on, or just try the engine yourself - you're guaranteed to find something to improve upon. However, it might be a good idea to come talk to us at Discord before dedicating a lot of your valuable time on a large PR.

Check out our [developer guide](./docs/developer-guide.md) for instructions on how to build and run the project.

### Everyone else

## Legal disclaimers

- This project is not affiliated with or endorsed by EA in any way. Command & Conquer is a trademark of Electronic Arts.
- This project is non-commercial. The source code is available for free under an open source license and always will be.
- OpenSAGE is not playable yet, but when it is and you want to play any of the supported games,
  you will need to have a legally acquired copy of that game. OpenSAGE relies on data files from the original games.
  You can purchase [Command & Conquer: The Ultimate Collection through Origin](https://www.origin.com/twn/en-us/store/command-and-conquer/command-and-conquer-the-ultimate-collection/ultimate-collection).
- This is a blackbox re-implementation project. The code in this project was written based on reading data files,
  and observing the game running. In some cases (for example refpack decompression) the code was written based on specs available on the Internet.
  I believe this puts the project in the clear, legally speaking. If someone at EA disagrees, please talk to me.
- If you want to contribute to this repository, your contribution must be either your own original code, or open source code with a clear acknowledgement of its origin. No code that was acquired through reverse engineering executable binaries will be accepted.
- No assets from the original games are included in this repo.

A note on the name: while Command & Conquer is a trademark of EA, SAGE is not (as far as I can tell, based on a US trademark search). "OpenSAGE" seems like a good way to make it clear what the project is about, without infringing on trademarks.


## Credits

DeeZire's [module list](http://www.redsys.su/mkportal/files/ModuleList.txt) has been extremely helpful in understanding all the `Object` parameters in `.ini` files.

## License

OpenSAGE is licensed under [GNU LGPL 3.0](./license.md).