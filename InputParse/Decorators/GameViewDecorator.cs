using System;
using InputParser.Abstract;
using Putty;
using static InputParser.Constant.Helpers;

namespace InputParser.Decorators
{
    public class GameViewDecorator : AbstractDecorator
    {
        public GameViewDecorator(IParser model) : base(model) { }

        public override Model ParseData(TerminalCharacter[,] chars)
        {
            var parsedModel = base.ParseData(chars);
            parsedModel.LineLength = GameViewWidth;
            var coloredStrings = new string[GameViewWidth * GameViewHeight];
            var highlightColorStrings = new string[GameViewWidth * GameViewHeight];
            var curentChar = 0;
            try
            {

                for (int j = 0; j < GameViewHeight; j++)
                {
                    for (int i = 0; i < GameViewWidth; i++)
                    {
                        coloredStrings[curentChar] = GetColoredCharacter(chars[i, j]);
                        highlightColorStrings[curentChar] = GetBackgroundColor(chars[i, j]);
                        curentChar++;
                    }
                }

                parsedModel.TileNames = coloredStrings;
                parsedModel.HighlightColors = highlightColorStrings;
            }
            catch (Exception)
            {
                foreach (var item in chars)
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
