using Putty;

namespace InputParser.Abstract
{
    public class BaseDecorator : IParser
    {

        public BaseDecorator()
        {
        }

        public Model ParseData(TerminalCharacter[,] characters)
        {
            return new Model();
        }
    }
}
