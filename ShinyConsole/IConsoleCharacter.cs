// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

namespace ShinyConsole
{
    public interface IConsoleCharacter
    {
        Font Font { get; }
        char Glyph { get; }
        uint Foreground { get; }
        uint Background { get; }
    }
}
