// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System.Collections.Generic;

namespace TtyRecMonkey
{
    class Palette : List<uint>
    {
        public static readonly Palette Default;

        static Palette()
        {
            Default = new Palette()
                { 0xFF000000
                , 0xFFCC0000
                , 0xFF00CC00
                , 0xFFCCCC00
                , 0xFF0000CC
                , 0xFFCC00CC
                , 0xFF00CCCC
                , 0xFFCCCCCC

                , 0xFF808080
                , 0xFFFF0000
                , 0xFF00FF00
                , 0xFFFFFF00
                , 0xFF0000FF
                , 0xFFFF00FF
                , 0xFF00FFFF
                , 0xFFFFFFFF
                };

            //debug.assert( 6*6*6 == 216 );

            for (uint r = 0; r < 6; ++r)
                for (uint g = 0; g < 6; ++g)
                    for (uint b = 0; b < 6; ++b)
                    {
                        Default.Add((0xFFu << 24) + ((42u * r) << 16) + ((42u * g) << 8) + ((42u * b) << 0));
                    }

            for (uint grey = 0; grey < 24; ++grey)
            {
                var component = 0xFFu * (grey + 1) / 26;
                Default.Add((0xFFu << 24) + 0x10101u * component);
            }

            //debug.assert( Default.Count==256 );

            Default.Add(0xFFE0E0E0); // default foreground
            Default.Add(0xFFFFFFFF); // default bold foreground
            Default.Add(0xFF000000); // default background
            Default.Add(0xFF404040); // default bold background
            Default.Add(0xFF00FF00); // cursor foreground
            Default.Add(0xFF00FF00); // cursor background
        }
    }
}
