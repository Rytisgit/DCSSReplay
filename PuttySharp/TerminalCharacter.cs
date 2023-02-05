// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System.Diagnostics;

namespace Putty
{
    [DebuggerDisplay("{Character} {Attributes}")]
    public struct TerminalCharacter
    {
        public TerminalCharacter(uint charValue, uint attrValue, int ccNextValue = 0)
        {
            chr = charValue;
            attr = attrValue;
            cc_next = ccNextValue;
        }
        private uint _chr;
        private uint chr
        {
            set { _chr = value; }
            get { return _chr; }
        }

        private uint _attr;
        private uint attr
        {
            set { _attr = value; }
            get { return _attr; }
        }

        private int _cc_next;
        private int cc_next
        {
            set { _cc_next = value; }
            get { return _cc_next; }
        }

        public char Character { get { return (char)chr; } }
        public uint Attributes =>
            (attr | (uint)cc_next);

        public bool Blink { get { return (0x200000u & Attributes) != 0; } }
        public bool Wide { get { return (0x400000u & Attributes) != 0; } }
        public bool Narrow { get { return (0x800000u & Attributes) != 0; } }
        public bool Bold { get { return (0x040000u & Attributes) != 0; } }
        public bool Underline { get { return (0x080000u & Attributes) != 0; } }
        public bool Reverse { get { return (0x100000u & Attributes) != 0; } }
        public int ForegroundPaletteIndex { get { var fg = (0x0001FFu & Attributes) >> 0; if (fg < 16 && Bold) fg |= 8; if (fg > 255 && Bold) fg |= 1; return (int)fg; } }
        public int BackgroundPaletteIndex { get { var bg = (0x03FE00u & Attributes) >> 9; if (bg < 16 && Blink) bg |= 8; if (bg > 255 && Blink) bg |= 1; return (int)bg; } }
    }
}
