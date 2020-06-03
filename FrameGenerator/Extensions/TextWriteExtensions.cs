using InputParser;
using SkiaSharp;

namespace FrameGenerator.Extensions
{
    public static class TextWriteExtensions
    {
        public static void WriteCharacter(this SKCanvas g, string coloredCharacter, SKPaint font, float x, float y)
        { 
            //font.Typeface = SKTypeface.FromFamilyName("Courier New",10,20,SKFontStyleSlant.Upright);
            //font.TextSize = 20;
            //g.DrawText("asdf", 20, 20, font);
            font.Color = ColorList.GetColor(coloredCharacter.Substring(1));
            g.DrawText(coloredCharacter[0].ToString(), x, y + font.TextSize, font);
        }

        public static void WriteCharacter(this SKCanvas g, string coloredCharacter, SKPaint font, float x, float y, string backgroundColor, float OffsetY = 3)
        {
            var black = ColorList.GetColor("BLACK");
            var yellow = ColorList.GetColor("YELLOW");
            var brown = ColorList.GetColor("BROWN");
            var LIGHTGREY = ColorList.GetColor("LIGHTGREY");
            var color = ColorList.GetColor(backgroundColor);
            if (color != black && color != LIGHTGREY)
            {
                if (color == brown)
                {
                    color = yellow;
                }
                var rect = SKRect.Create(x,y+4,13f,16f);
                var recpaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = color
                };
                g.DrawRect(rect, recpaint);
            }
            WriteCharacter(g, coloredCharacter, font, x - OffsetY, y);
        }

        public static SKCanvas WriteSideDataInfo(this SKCanvas g, string title, string info, SKPaint font, float x, float y)
        {
            var brown = new SKColor(143, 89, 2);
            var gray = new SKColor(186, 189, 182);

            font.Color = brown;
            g.DrawText(title, x, y + font.TextSize, font);
            font.Color = gray;
            g.DrawText(info, x + font.MeasureText(title), y + font.TextSize, font);

            return g;
        }

        public static SKCanvas DrawPercentageBar(this SKCanvas g, int amount, int maxAmount, int previousAmount, SKColor barColor, SKColor LostAmmountColor, float x, float y)
        {
            SKBitmap bar = new SKBitmap(250, 16);
            SKCanvas temp = new SKCanvas(bar);
            temp.Clear(SKColors.Gray);

            g.DrawBitmap(bar, x, y);

            if (amount > 0)
            {
                int barLength = (int)(250 * ((float)amount / maxAmount));
                bar = new SKBitmap(barLength, 16);
                temp = new SKCanvas(bar);
                temp.Clear(barColor);
                g.DrawBitmap(bar, x, y);
                previousAmount = previousAmount > maxAmount ? maxAmount : previousAmount;
                if (barLength != 250 && previousAmount - amount > 0)
                {
                    int prevBarLength = (int)(250 * ((float)(previousAmount - amount) / maxAmount)) + 1;
                    SKBitmap losthealthbar = new SKBitmap(prevBarLength, 16);
                    new SKCanvas(losthealthbar).Clear(LostAmmountColor);
                    g.DrawBitmap(losthealthbar, x + barLength, y);
                }
            }

            return g;
        }
    }
}