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

#### Linux dependencies
 - .NET 6.0.300+
 - `libc`
 - `libsdl2`
 - `libopenal`

##### .NET 6.0.300+
The standard package manage install for dotnet on linux currently installs 6.0.1xx, which does not contain some of the C# 11 preview features used in OpenSAGE. To install 6.0.402 or greater on Ubuntu, follow the instructions [here](https://github.com/dotnet/core/issues/7699) for using a .NET 6 package via **PMC** (_not_ via Jammy feed).

##### `libc`
`sudo apt install libc-dev`
Some distros or package managers may install the file libdl.so.2 and not libdl.so. This can be remedied via a symlink ([source](https://github.com/mellinoe/veldrid/issues/143#issuecomment-446096640)):
 - use `ldconfig -p | grep libdl` to dtermine the location of `libdl.so.2` (or similar)
 - `sudo ln -s </path/to/libdl.so.2> </same/path/libdl.so>`

```
➜  ~ ldconfig -p | grep libdl
	libdl.so.2 (libc6,x86-64) => /lib/x86_64-linux-gnu/libdl.so.2
➜  ~ sudo ln -s /lib/x86_64-linux-gnu/libdl.so.2 libdl.so
➜  ~ ls -l /lib/x86_64-linux-gnu | grep libdl
-rw-r--r--  1 root root         8 Oct  7 01:13 libdl.a
lrwxrwxrwx  1 root root        32 Nov  6 00:12 libdl.so -> /lib/x86_64-linux-gnu/libdl.so.2
-rw-r--r--  1 root root     14480 Oct  7 01:13 libdl.so.2
➜  ~ 
```

##### `libsdl2`
`sudo apt install libsdl2-dev`

##### `openal`
`sudo apt install libopenal-dev`

## Running OpenSAGE

OpenSage does not provide any game assets itself, so it expects the user to have the game files installed on the system. 

### Windows

On Windows any local installations of C&C or BFME/BFME2/BFME2ROTWK are found through the registry or if they are defined by a environment variables. (See Linux & MacOS below)

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
