using Putty;

namespace InputParser.Abstract
{
    public abstract class AbstractDecorator : IParser
    {
        private readonly IParser _model;

        protected AbstractDecorator(IParser model)
        {
            _model = model;
        }

        public virtual Model ParseData(TerminalCharacter[,] chars)
        {
            return _model.ParseData(chars);
        }
    }
}
