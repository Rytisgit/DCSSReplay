## To launch with ttyrec tiles player on visual studio

1. set solution start program to ttyrecmonkey, configuration to release, cpu to x86
2. restore nuget packages
3. rebuild the solution
4. it should now work


![Screenshot](https://raw.githubusercontent.com/MaulingMonkey/TtyRecMonkey/master/.projnfo/screenshots/Untitled.png)

# Quickstart

Please read [LICENSE.txt](https://github.com/MaulingMonkey/TtyRecMonkey/blob/master/LICENSE.txt) for license terms

## Downloads

See the latest [Releases](https://github.com/MaulingMonkey/TtyRecMonkey/releases)

## Placeholder Controls

```
ZXCVBNM    Alter playback speed (-100x, -10x, -1x, 0x, +1x, +10x, +100x)
AS         Zoom In/Out
```

## Contact

- The author can also be contacted in the IRC channel #gamedev on irc.afternet.org under the alias MaulingMonkey
- File github issues, I don't mind :)



# Development

## Dependencies

- Visual Studio 2008    ( no solution for Express, sorry )
- [SlimDX Developer SDK](http://slimdx.org/download.php)
- Direct3D9

## Grabbing the Source

You can browse and observe project development at https://github.com/MaulingMonkey/TtyRecMonkey

If you simply want to track changes, read only git access is available at:
```
	git clone git://github.com/MaulingMonkey/TtyRecMonkey.git
	svn checkout http://svn.github.com/MaulingMonkey/TtyRecMonkey.git
```

## Building the Source

- Open TtyRecMonkey.sln
- Build it.
- Select the project TtyRecMonkey and make it your startup project before running.
  - Alternatively, launch bin\x86\Debug\TtyRecMonkey.exe or bin\x86\Release\TtyRecMonkey.exe
- Select a non-compressed ttyrec file and open it

## Project Layout

### PuttyDLL

PuttyDLL is a native C project which is almost a direct replica of PUTTY.DSP project from the official PuTTY source code.

Key differences:

- It's been converted to a VS2008 project via the upgrade manager (which worked flawlessly)
- It now generates DLLs instead of EXEs
- _*_SECURE_NO_DEPRECATE have been added to supress warning spam from standard library functions
- export.c has been added which exposes methods for PuttySharp (C#) to PInvoke
- Other source files may be modified to expose more to export.c -- should be mostly cosmetic/making availbe outside the TU


### PuttySharp

PuttySharp is a C# library which wraps the methods exposed by PuttyDLL for easier consumption in C#

It is currently small enough that it ended up well commented and designed.  This probably won't last very long.


### ShinyConsole

ShinyConsole is a C# program which provides and tests SlimDX/D3D9 based console text rendering

It is crufty and in need of sanitization.

It is used as a library by TtyPlayer, despite being a program.



### TtyRecMonkey

TtyRecMonkey is the main C# program which is the point of this project.  It is not well written.

It currently handles:

- Parsing (to be spin off?) of TtyRecs
- Basic playback of TtyRecs
