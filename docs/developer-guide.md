# Developer Guide

## Building the repository

Opensage uses the standard .NET Core tooling. Install the tools and build normally (dotnet build).

### Windows

On Windows you can just open the provided solution file `src/OpenSage.sln` to get started.

### Linux & MacOS

On Unix systems you want to use the .NET Core CLI tools to compile OpenSage:
```
cd src
dotnet build
```

You can also build like that on Windows, in case you don't want to use Visual Studio.

## Running OpenSAGE

OpenSage does not provide any game assets itself, so it expects the user to have the C&C game files installed on the system. 

### Windows

On Windows any local installations of C&C or BFME are found through the registry.

### Linux & MacOS

Since there are no official installers of the original games on Unix systems, the game installations must be defined through environment variables here. The list of environment variables is as follows:

| Game      | Environment variable  |
|-----------|-----------------------|
| Generals  | CNC_GENERALS_PATH     |
| Zero Hour | CNC_GENERALS_ZH_PATH  |
| BFME      | BFME_PATH             |
| BFME II   | BFME2_PATH            |
| BFME II   | BFME2_ROTWK_PATH      |

The same identifiers do also work for Windows, in case you don't want to use the registry.

### Starting OpenSage

To run OpenSage you must run the OpenSage.Launcher project, which by default will start into C&C Generals. You can start a different game,
by passing the `-g` option with an identfier for the game you want to start, e.g. `-g bfme` will start **Battle for Middleearth** (granted you have specified a valid installation location).

### Developer Mode

While inside the launcher application you can switch into a `developer mode`, by pressing **F11**. This mode gives you some useful insights on what's going on inside the engine and should help you for debugging purposes.