using System;
using InputParser.Abstract;
using Putty;
using static InputParser.Constant.Helpers;

namespace InputParser.Decorators
{
    class GameViewParser : AbstractDecorator
    {
        public GameViewParser(IParser Imodel) : base(Imodel)
        {
        }

        public override Model ParseData(TerminalCharacter[,] characters)
        {
            var parsedModel = base.model.ParseData(characters);
            if (!(base.model is Model)) return new Model();
            parsedModel.LineLength = GameViewWidth;
            var coloredStrings = new string[GameViewWidth * GameViewHeight];
            var highlightColorStrings = new string[GameViewWidth * GameViewHeight];
            var curentChar = 0;
            try
            {

                for (int j = 0; j < GameViewHeight; j++)
                for (int i = 0; i < GameViewWidth; i++)
                {
                    coloredStrings[curentChar] = GetColoredCharacter(characters[i, j]);
                    highlightColorStrings[curentChar] = GetBackgroundColor(characters[i, j]);
                    curentChar++;
                }

                parsedModel.TileNames = coloredStrings;
                parsedModel.HighlightColors = highlightColorStrings;
            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15)
                        Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }

            return parsedModel;

        }
    }
}
