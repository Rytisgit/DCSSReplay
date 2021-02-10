using Putty;

namespace InputParser.Abstract
{
    public interface IParser
    {
        Model ParseData(TerminalCharacter[,] chars);
    }
}
