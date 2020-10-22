using System;
using InputParser.Abstract;
using InputParser.Constant;
using Putty;
using static InputParser.Constant.Helpers;

namespace InputParser.Decorators
{
    public class TextDecorator : AbstractDecorator
    {
        public TextDecorator(IParser model): base(model)
        { 
        }
        public virtual Model ParseData(TerminalCharacter[,] characters)
        {
            base.model.ParseData(characters);
            if (!(base.model is Model)) return new Model();
            var model = (Model)base.model;
            model.Layout = LayoutType.ConsoleFull;
            model.LineLength = FullWidth;
            var coloredStrings = new string[FullWidth * FullHeight];
            var curentChar = 0;
            try
            {
                for (int j = 0; j < FullHeight; j++)
                for (int i = 0; i < FullWidth; i++)
                {
                    coloredStrings[curentChar] = GetColoredCharacter(characters[i, j]);
                    curentChar++;
                }
                model.TileNames = coloredStrings;

            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return model;
        }
    }
}
