using InputParse;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FrameGenerator.Extensions
{
    public static class GraphicsExtensions
    {      
        public static void WriteCharacter(this Graphics g, string coloredCharacter, Font font, float x, float y)
        {
            var brush = new SolidBrush(ColorList.GetColor(coloredCharacter.Substring(1)));
            g.DrawString(coloredCharacter[0].ToString(), font, brush, x, y);
        }
        public static void WriteCharacter(this Graphics g, string coloredCharacter, Font font, float x, float y, string backgroundColor)
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
                Bitmap backgroundColorbmp = new Bitmap(font.Height, (int)font.Size);
                Graphics.FromImage(backgroundColorbmp).Clear(color);
                g.DrawImage(backgroundColorbmp, x, y);
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
                Bitmap backgroundColorbmp = new Bitmap(font.Height, (int)font.Size);
                Graphics.FromImage(backgroundColorbmp).Clear(color);
                g.DrawImage(backgroundColorbmp, x, y);
            }
        }
    }
}
