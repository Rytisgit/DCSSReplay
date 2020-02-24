# DCSSReplay - WIP tiles player
## It's not perfect, but it's the best we got.

Still quite a few things to be improved but at least its something.

![Screenshot](https://github.com/Rytisgit/DCSSReplay/blob/master/.projnfo/screenshots/Untitled.png)

## Downloads

See the latest [Releases](https://github.com/Rytisgit/DCSSReplay/releases)

## Placeholder Controls

### The window that accepts controls is the ASCII window, it needs to have focus

```
ZXCVBNM    Alter playback speed (-100x, -10x, -1x, Pause/Resume, 1x, 10x, 100x)
M          Increase Playback speed by +100, stacks
SPACE      Pause/Resume
FG         Increase/Decrease PLayback speed (-1x, +1x)
Ctrl+C     Reconfigure TtyRecMonkey
Ctrl+O     Open a ttyrec
```

## Contact

- The author can be contacted in Discord as Sentei#5306
- Those who prefer slower communication can send emails to petronisrytis@gmail.com
- You can file github issues as well

# Development

## Building the Source

I Used VS 2019 Enterprise, should work for older versions as well

- Open TtyRecMonkey.sln
- Select the project TtyRecMonkey and make it your startup project before running.
- configuration to release, cpu to x86
- restore nuget packages
- rebuild the solution
- it should now work

## Project Layout

### PuttyDLL - MaulingMonkey

PuttyDLL is a native C project which is almost a direct replica of PUTTY.DSP project from the official PuTTY source code.

Key differences:

- It's been converted to a VS2008 project via the upgrade manager (which worked flawlessly)
- It's then been converted to a VS2012 project, which also worked
- It now generates DLLs instead of EXEs
- _*_SECURE_NO_DEPRECATE have been added to supress warning spam from standard library functions
- export.c has been added which exposes methods for PuttySharp (C#) to PInvoke
- Other source files may be modified to expose more to export.c -- should be mostly cosmetic/making availbe outside the TU


### PuttySharp - MaulingMonkey

PuttySharp is a C# library which wraps the methods exposed by PuttyDLL for easier consumption in C#

It is currently small enough that it ended up well commented and designed.  This probably won't last very long.


### ShinyConsole - MaulingMonkey

ShinyConsole is a C# program which provides and tests SlimDX/D3D9 based console text rendering

It is crufty and in need of sanitization.

It is used as a library by TtyPlayer, despite being a program.


### TtyRecMonkey - MaulingMonkey, Rytisgit

TtyRecMonkey is the main C# program which is the point of this project.  It is not well written.

It currently handles:

- Parsing (to be spin off?) of TtyRecs
- Basic playback of TtyRecs

### InputParse - Rytisgit -TODO description
### FrameGenerator - Aspectus, Rytisgit -TODO description
### Character - Rytisgit -TODO description
### Window - Aspectus -TODO description

### Installer - Rytisgit https://github.com/Rytisgit/DCSSReplayInstaller

