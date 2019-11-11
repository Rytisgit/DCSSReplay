// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using ShinyConsole;

namespace TtyRecMonkey
{
    public struct Character : IConsoleCharacter
    {
        public new uint Foreground, Background;
        public uint ActualForeground;// { get { return base.Foreground; } set { base.Foreground = value; }}
        public uint ActualBackground;// { get { return base.Background; } set { base.Background = value; }}
        public char Glyph;
        public Font Font;

        uint IConsoleCharacter.Foreground { get { return ActualForeground; } }
        uint IConsoleCharacter.Background { get { return ActualBackground; } }
        Font IConsoleCharacter.Font { get { return Font; } }
        char IConsoleCharacter.Glyph { get { return Glyph; } }
    }
}