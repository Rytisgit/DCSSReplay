using System;
using InputParser.Abstract;
using Putty;
using static InputParser.Constant.Helpers;
 


namespace InputParser.Decorators
{
    public class GameViewDecorator : AbstractDecorator
    {
        public GameViewDecorator(IParser model) : base(model) { }

        public override Model ParseData(TerminalCharacter[,] characters)
        {
            var parsedModel = base.ParseData(characters);
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
