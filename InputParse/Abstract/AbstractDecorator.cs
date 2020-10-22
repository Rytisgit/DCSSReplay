using System;
using System.Collections.Generic;
using System.Text;
using Putty;
using static InputParser.Helpers;

namespace InputParser.Decorators
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
