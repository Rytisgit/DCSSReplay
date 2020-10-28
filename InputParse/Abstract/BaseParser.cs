using Putty;

namespace InputParser.Abstract
{
    public class BaseParser : IParser
    {

        public BaseParser()
        {
        }

        public Model ParseData(TerminalCharacter[,] characters)
        {
            return new Model();
        }
    }
}
