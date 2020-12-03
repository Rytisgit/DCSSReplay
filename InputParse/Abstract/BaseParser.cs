using Putty;

namespace InputParser.Abstract
{
    public class BaseParser : IParser
    {

        public BaseParser()
        {
        }

        public Model ParseData(TerminalCharacter[,] chars)
        {
            return new Model();
        }
    }
}
