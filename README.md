# DCSSReplay - A visual TtyRec player for DCSS
## We've entered Beta but theres still so much left to do.

![Screenshot](https://github.com/Rytisgit/DCSSReplay/blob/0.7.0/.projnfo/screenshots/thumbnail0.7.png?raw=true)

# FOR DOWNLOADS

## [Click Here to see the latest Releases](https://github.com/Rytisgit/DCSSReplay/releases)

## Keyboard Shortcuts

```
ZXCVBNM    Alter Playback speed (-100x, -10x, -1x, Pause/Resume, 1x, 10x, 100x)
M          Increase Playback speed by +100, stacks
V/SPACE      Pause/Resume
F/G         Increase/Decrease Playback speed by 1 (-1x, +1x)
D/H         Increase/Decrease Playback speed by 0.2 (-0.2x, +0.2x)
Ctrl+O     Open a ttyrec file from your computer
Escape     Close ttyrec and return To Main menu
Alt+Enter  Toggle fullscreen

, (Comma)     Frame Step Back 1
. (Dot)       Frame Step Forward 1

Left Arrow    Time Backward 5 Seconds
Right Arrow   Time Forward 5 Seconds
                  
A / S     Switch Tile to console / Full Console Mode
```

## Contact

- The author can be contacted in Discord as Sentei#5306
- Those who prefer slower communication can send emails to petronisrytis@gmail.com
- You can file github issues as well, in fact I encourage it!

# Development

## Building the Source

I Used VS 2019 Enterprise, should work for older versions as well

- Open DCSSReplay.sln
- Select the project TtyRecMonkey and make it your startup project before running.
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
- Overrided encoding check, to always output UTF-8, since DCSS TtyRecs are in that format


### PuttySharp - MaulingMonkey

PuttySharp is a C# library which wraps the methods exposed by PuttyDLL for easier consumption in C#

It is currently small enough that it ended up well commented and designed.  This probably won't last very long.

### TtyRecDecoder - MaulingMonkey, Rytisgit

Parses Ttyrec files, prepares for feeding the data to Putty

### TtyRecMonkey - MaulingMonkey, Rytisgit, Aspectus

TtyRecMonkey is the main C# program which is the point of this project.  It is not well written.

It currently handles:

- Controlling the threading for individual frame generation
- Updating the main display window 
- Inputs from user

### InputParser - Rytisgit

Contains a Model that is used for frame Generation. 

Parser fills the Model with data before every frame from the terminal Character array that is passed to it.

### FrameGenerator - Aspectus, Rytisgit

The Big One - Takes all the custom rules along with the Model for each frame and tries to make an image that resembles what the screen would look like on a real DCSS game in that moment. 

Looks similar quite often.

Was a really big mess before the big refactoring, now is just a bit of a mess, but its getting better.

### Installer - Rytisgit 
- Creates .msi installers for releases.
- Has seperate repository
- https://github.com/Rytisgit/DCSSReplayInstaller

