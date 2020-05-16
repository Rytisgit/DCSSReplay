// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System.Diagnostics;

namespace Putty
{
    [DebuggerDisplay("{Character} {attr}")]
    public struct TerminalCharacter
    {
        private readonly uint chr;
        private readonly uint attr;
        private readonly int cc_next;

        public char Character { get { return (char)chr; } }

        public bool Blink { get { return (0x200000u & attr) != 0; } }
        public bool Wide { get { return (0x400000u & attr) != 0; } }
        public bool Narrow { get { return (0x800000u & attr) != 0; } }
        public bool Bold { get { return (0x040000u & attr) != 0; } }
        public bool Underline { get { return (0x080000u & attr) != 0; } }
        public bool Reverse { get { return (0x100000u & attr) != 0; } }
        public int ForegroundPaletteIndex { get { var fg = (0x0001FFu & attr) >> 0; if (fg < 16 && Bold) fg |= 8; if (fg > 255 && Bold) fg |= 1; return (int)fg; } } // TODO: Reverse modes
        public int BackgroundPaletteIndex { get { var bg = (0x03FE00u & attr) >> 9; if (bg < 16 && Blink) bg |= 8; if (bg > 255 && Blink) bg |= 1; return (int)bg; } }
        public uint Colors { get { return (0x03FFFFu & attr) >> 9; } }
    }
}
