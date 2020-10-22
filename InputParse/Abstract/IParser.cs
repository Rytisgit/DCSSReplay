using System;
using System.Collections.Generic;
using System.Text;
using Putty;

namespace InputParser
{
    public interface IParser
    {
        Model ParseData(TerminalCharacter[,] chars);
    }
}
