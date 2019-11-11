using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TtyRecMonkey
{
    public static class CurrentFrame
    {
        public static Putty.TerminalCharacter[,] frame { get; set; } = null;

    }
}
