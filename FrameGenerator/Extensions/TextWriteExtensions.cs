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
        //private static int MeasureDisplayStringWidth(this Graphics graphics, string text, Font font)
        //{

        //    StringFormat format = new StringFormat(StringFormat.GenericDefault);
        //    RectangleF rect = new RectangleF(0, 0, 1000, 1000);
        //    CharacterRange[] ranges = { new CharacterRange(0, text.Length) };

        //    format.SetMeasurableCharacterRanges(ranges);
        //    format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

        //    Region[] regions = graphics.MeasureCharacterRanges(text, font, rect, format);
        //    rect = regions[0].GetBounds(graphics);

        //    return (int)(rect.Right);
        //}
        public static void WriteCharacter(this SKCanvas g, string coloredCharacter, SKPaint font, float x, float y, string backgroundColor, float OffsetY = 3)
        {
            var black = ColorList.GetColor("BLACK");
            var yellow = ColorList.GetColor("YELLOW");
            var brown = ColorList.GetColor("BROWN");
            //var brush = new SolidBrush(ColorList.GetColor(coloredCharacter.Substring(1)));
            //var color = ColorList.GetColor(backgroundColor);
            //if (color.ToArgb() != black.ToArgb())
            //{
            //    if (color.ToArgb() == brown.ToArgb())
            //    {
            //        color = yellow;
            //    }
            //    int stringWidth = g.MeasureDisplayStringWidth(coloredCharacter[0].ToString(), font);
            //    Bitmap backgroundColorbmp = new Bitmap(stringWidth, (int)font.Size);
            //    Graphics.FromImage(backgroundColorbmp).Clear(color);
            //    g.DrawImage(backgroundColorbmp, x, y + OffsetY);
            //}
            WriteCharacter(g, coloredCharacter, font, x, y);
            //g.DrawString(coloredCharacter[0].ToString(), font, brush, x, y);
        }

        //public static void PaintBackground(this Graphics g, string backgroundColor, Font font, float x, float y)
        //{
        //    var black = ColorList.GetColor("BLACK");
        //    var yellow = ColorList.GetColor("YELLOW");
        //    var brown = ColorList.GetColor("BROWN");
        //    var color = ColorList.GetColor(backgroundColor);
        //    if (color.ToArgb() != black.ToArgb())
        //    {
        //        if (color.ToArgb() == brown.ToArgb())
        //        {
        //            color = yellow;
        //        }
        //        int stringWidth = g.MeasureDisplayStringWidth(" ", font);
        //        Bitmap backgroundColorbmp = new Bitmap(stringWidth, font.Height);//wrong
        //        Graphics.FromImage(backgroundColorbmp).Clear(color);
        //        g.DrawImage(backgroundColorbmp, x, y);
        //    }
        //}

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