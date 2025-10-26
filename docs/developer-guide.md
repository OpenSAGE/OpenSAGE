# Developer Guide

## Building the repository

Opensage uses the standard .NET Core tooling. Install the tools and build normally (dotnet build).

### Windows

On Windows you can just open the provided solution file `src/OpenSage.sln` to get started.

### Linux & MacOS

On Unix systems you want to use the .NET Core CLI tools to compile OpenSage:

```shell
cd src
dotnet build
```

You can also build like that on Windows, in case you don't want to use Visual Studio.

#### Linux dependencies

- .NET 8.0+
- `libc`
- `libsdl2`
- `libopenal`

##### .NET 8.0+

Linux distributions may have multiple forms of .NET Core installations, with the minimum version supported being 8.0.

If you wish to install .NET Core 8.0+ on your system, you can follow the instructions either for your distribution, for example:

- [Ubuntu Package Instructions](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install?tabs=dotnet8&pivots=os-linux-ubuntu-2404)
- [Fedora Package Instructions](https://learn.microsoft.com/en-us/dotnet/core/install/linux-fedora?tabs=dotnet8)

Or simply follow the universal Linux [installation script instructions](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual).

Do look at the main, [official Linux installation guide by MicroSoft](https://learn.microsoft.com/en-us/dotnet/core/install/linux) to see how to install .NET Core on your system.

##### `libc`

- APT based: `sudo apt install libc-dev`
- RPM based: `sudo dnf install glibc-devel`.

Some distros or package managers may install the file libdl.so.2 and not libdl.so. This can be remedied via a symlink ([source](https://github.com/mellinoe/veldrid/issues/143#issuecomment-446096640)):

- use `ldconfig -p | grep libdl` to dtermine the location of `libdl.so.2` (or similar)
- `sudo ln -s </path/to/libdl.so.2> </same/path/libdl.so>`

```shell
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

- APT based: `sudo apt install libsdl2-dev`
- RPM based: `sudo dnf install sdl2-compat-devel`

##### `openal`

- APT based: `sudo apt install libopenal-dev`
- RPM based: `sudo apt install openal-soft-devel`

## Running OpenSAGE

OpenSage does not provide any game assets itself, so it expects the user to have the game files installed on the system.

### Windows

On Windows any local installations of C&C or BFME/BFME2/BFME2ROTWK are found through the registry or if they are defined by a environment variables. (See Linux & MacOS below)

### Linux & MacOS

Since there are no official installers of the original games on Unix systems, the game installations must be defined through environment variables here. The list of environment variables is as follows:

| Game      | Environment variable |
| --------- | -------------------- |
| Generals  | CNC_GENERALS_PATH    |
| Zero Hour | CNC_GENERALS_ZH_PATH |
| BFME      | BFME_PATH            |
| BFME II   | BFME2_PATH           |
| BFME II   | BFME2_ROTWK_PATH     |

The same identifiers do also work for Windows, in case you don't want to use the registry.

### Starting OpenSage

To run OpenSage you must run the OpenSage.Launcher project, which by default will start into C&C Generals. You can start a different game,
by passing the `-g` option with an identfier for the game you want to start, e.g. `-g bfme` will start **Battle for Middleearth** (granted you have specified a valid installation location).

### Developer Mode

While inside the launcher application you can switch into a `developer mode`, by pressing **F11**. This mode gives you some useful insights on what's going on inside the engine and should help you for debugging purposes.
