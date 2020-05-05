using InputParser;
using System.Drawing;

namespace FrameGenerator.Extensions
{
    public static class TextWriteExtensions
    {
        public static void WriteCharacter(this Graphics g, string coloredCharacter, Font font, float x, float y)
        {
            var brush = new SolidBrush(ColorList.GetColor(coloredCharacter.Substring(1)));
            g.DrawString(coloredCharacter[0].ToString(), font, brush, x, y);
        }
        private static int MeasureDisplayStringWidth(this Graphics graphics, string text, Font font)
        {

            StringFormat format = new StringFormat(StringFormat.GenericDefault);
            RectangleF rect = new RectangleF(0, 0, 1000, 1000);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };

            format.SetMeasurableCharacterRanges(ranges);
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

            Region[] regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right);
        }
        public static void WriteCharacter(this Graphics g, string coloredCharacter, Font font, float x, float y, string backgroundColor, float OffsetY = 3)
        {
            var black = ColorList.GetColor("BLACK");
            var yellow = ColorList.GetColor("YELLOW");
            var brown = ColorList.GetColor("BROWN");
            var brush = new SolidBrush(ColorList.GetColor(coloredCharacter.Substring(1)));
            var color = ColorList.GetColor(backgroundColor);
            if (color.ToArgb() != black.ToArgb())
            {
                if (color.ToArgb() == brown.ToArgb())
                {
                    color = yellow;
                }
                int stringWidth = g.MeasureDisplayStringWidth(coloredCharacter[0].ToString(), font);
                Bitmap backgroundColorbmp = new Bitmap(stringWidth, (int)font.Size);
                Graphics.FromImage(backgroundColorbmp).Clear(color);
                g.DrawImage(backgroundColorbmp, x, y + OffsetY);
            }
            g.DrawString(coloredCharacter[0].ToString(), font, brush, x, y);
        }

        public static void PaintBackground(this Graphics g, string backgroundColor, Font font, float x, float y)
        {
            var black = ColorList.GetColor("BLACK");
            var yellow = ColorList.GetColor("YELLOW");
            var brown = ColorList.GetColor("BROWN");
            var color = ColorList.GetColor(backgroundColor);
            if (color.ToArgb() != black.ToArgb())
            {
                if (color.ToArgb() == brown.ToArgb())
                {
                    color = yellow;
                }
                int stringWidth = g.MeasureDisplayStringWidth(" ", font);
                Bitmap backgroundColorbmp = new Bitmap(stringWidth, font.Height);//wrong
                Graphics.FromImage(backgroundColorbmp).Clear(color);
                g.DrawImage(backgroundColorbmp, x, y);
            }
        }

        public static Graphics WriteSideDataInfo(this Graphics g, string title, string info, Font font, float x, float y)
        {
            var brown = new SolidBrush(Color.FromArgb(143, 89, 2));
            var gray = new SolidBrush(Color.FromArgb(186, 189, 182));

            g.DrawString(title, font, brown, x, y);
            g.DrawString(info, font, gray, x + g.MeasureString(title, font).Width, y);

            return g;
        }

        public static Graphics DrawPercentageBar(this Graphics g, int amount, int maxAmount, int previousAmount, Color barColor, Color LostAmmountColor, float x, float y)
        {
            Bitmap bar = new Bitmap(250, 16);
            Graphics temp = Graphics.FromImage(bar);
            temp.Clear(Color.Gray);
            g.DrawImage(bar, x, y);

            if (amount > 0)
            {
                int barLength = (int)(250 * ((float)amount / maxAmount));
                bar = new Bitmap(barLength, 16);
                temp = Graphics.FromImage(bar);
                temp.Clear(barColor);
                g.DrawImage(bar, x, y);
                if (barLength != 250 && previousAmount - amount > 0)
                {
                    int prevBarLength = (int)(250 * ((float)(previousAmount - amount) / maxAmount)) + 1;
                    Bitmap losthealthbar = new Bitmap(prevBarLength, 16);
                    Graphics.FromImage(losthealthbar).Clear(LostAmmountColor);
                    g.DrawImage(losthealthbar, x + barLength, y);
                }
            }

            return g;
        }
    }
}