using Putty;

namespace InputParser.Abstract
{
    public class AbstractDecorator : IParser
    {
        protected IParser model; 

        public AbstractDecorator(IParser model)
        {
            this.model = model;
        }

        public virtual Model ParseData(TerminalCharacter[,] chars)
        {
            return model.ParseData(chars);
        }
    }
}
