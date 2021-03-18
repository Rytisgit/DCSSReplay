// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System.Diagnostics;
using System.Dynamic;

namespace Putty
{
    [DebuggerDisplay("{Character} {Attributes}")]
    public struct TerminalCharacter
    {
        private uint chr { get; set; }
        private uint attr { get; set; }
        private int cc_next { get; set; }

        //private uint? fixed_attr;//for some reason linux compiled puttydll returns attr and cc_next switched, so we need to have a central attribute to check for color

        public char Character { get { return (char)chr; } }
        public uint Attributes =>
            //fixed_attr ??= attr != 0 ? attr : (uint) cc_next;
            (attr | (uint)cc_next);

        public bool Blink { get { return (0x200000u & Attributes) != 0; } }
        public bool Wide { get { return (0x400000u & Attributes) != 0; } }
        public bool Narrow { get { return (0x800000u & Attributes) != 0; } }
        public bool Bold { get { return (0x040000u & Attributes) != 0; } }
        public bool Underline { get { return (0x080000u & Attributes) != 0; } }
        public bool Reverse { get { return (0x100000u & Attributes) != 0; } }
        public int ForegroundPaletteIndex { get { var fg = (0x0001FFu & Attributes) >> 0; if (fg < 16 && Bold) fg |= 8; if (fg > 255 && Bold) fg |= 1; return (int)fg; } } // TODO: Reverse modes
        public int BackgroundPaletteIndex { get { var bg = (0x03FE00u & Attributes) >> 9; if (bg < 16 && Blink) bg |= 8; if (bg > 255 && Blink) bg |= 1; return (int)bg; } }
    }
}
