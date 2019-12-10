using InputParse;
using Putty;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace FrameGenerator.FrameCreation
{

    class CreatingFrame
    {
        public static void DrawFrame(Dictionary<string, Bitmap> itempng, Dictionary<string, string> itemdata, Dictionary<string, string> moretiles, Dictionary<string, Bitmap> alldngnpng, Dictionary<string, string> monsterdata, Dictionary<string, Bitmap> monsterPNG, Dictionary<string, string[]> floorandwall, Dictionary<string, string> _characterdata, Dictionary<string, Bitmap> _characterpng, Window.Widow_Display display, TerminalCharacter[,] chars)
        {
            var dict = new Dictionary<string, string>();
            var model = Parser.ParseData(chars);


            Bitmap bmp = new Bitmap(1602, 1050,PixelFormat.Format32bppArgb);
          
            

            using (Graphics g = Graphics.FromImage(bmp))
            {   
                g.Clear(Color.Black);
                using (Font arialFont = new Font("Courier New", 16))
                {
                    string OnlyRace = model.SideData.Race.Substring(0, 6);
                    var yellow = new SolidBrush(Color.FromArgb(252, 233, 79));
                    var brown = new SolidBrush(Color.FromArgb(143, 89, 2));
                    var gray = new SolidBrush(Color.FromArgb(186, 189, 182));


                    g.DrawString(model.SideData.Name, arialFont, yellow, 32 * model.LineLength, 0);
                    g.DrawString(model.SideData.Race, arialFont, yellow, 32 * model.LineLength, 20);
                    g.DrawString("Health: ", arialFont, brown, 32 * model.LineLength, 40);
                    g.DrawString(model.SideData.Health.ToString() + '/' + model.SideData.MaxHealth.ToString(), arialFont, gray, 32 * model.LineLength + g.MeasureString("Health: ", arialFont).Width, 40);
                    g.DrawString("Mana: ", arialFont, brown, 32 * model.LineLength, 60);
                    g.DrawString(model.SideData.Magic.ToString() + '/' + model.SideData.MaxMagic.ToString(), arialFont, gray, 32 * model.LineLength + g.MeasureString("Mana: ", arialFont).Width, 60);
                    g.DrawString("AC: ", arialFont, brown, 32 * model.LineLength, 80);
                    g.DrawString(model.SideData.ArmourClass, arialFont, gray, 32 * model.LineLength + g.MeasureString("AC: ", arialFont).Width, 80);
                    g.DrawString("EV: ", arialFont, brown, 32 * model.LineLength, 100);
                    g.DrawString(model.SideData.Evasion, arialFont, gray, 32 * model.LineLength + g.MeasureString("EV: ", arialFont).Width, 100);
                    g.DrawString("SH: ", arialFont, brown, 32 * model.LineLength, 120);
                    g.DrawString(model.SideData.Shield, arialFont, gray, 32 * model.LineLength + g.MeasureString("SH: ", arialFont).Width, 120);
                    g.DrawString("XL: ", arialFont, brown, 32 * model.LineLength, 140);
                    g.DrawString(model.SideData.ExperienceLevel, arialFont, gray, 32 * model.LineLength + g.MeasureString("XL: ", arialFont).Width, 140);
                    g.DrawString(" Next: ", arialFont, brown, 32 * model.LineLength + g.MeasureString("XL: " + model.SideData.ExperienceLevel, arialFont).Width, 140);
                    g.DrawString(model.SideData.NextLevel, arialFont, gray, 32 * model.LineLength + g.MeasureString("XL: " + model.SideData.ExperienceLevel + " Next: ", arialFont).Width, 140);
                    g.DrawString("Noise:", arialFont, brown, 32 * model.LineLength, 160);

                    int increase = 0;

                    g.DrawString("Wp: ", arialFont, brown, 32 * model.LineLength, 180);
                    if (model.SideData.Weapon.Length > 39)
                    {
                        g.DrawString(model.SideData.Weapon.Substring(0, 35), arialFont, gray, 32 * model.LineLength + g.MeasureString("Wp: ", arialFont).Width, 180);
                        g.DrawString(model.SideData.Weapon.Substring(35), arialFont, gray, 32 * model.LineLength + g.MeasureString("Wp: ", arialFont).Width, 200);
                        increase += 20;

                    }
                    else g.DrawString(model.SideData.Weapon, arialFont, gray, 32 * model.LineLength + g.MeasureString("Wp: ", arialFont).Width, 180);

                    g.DrawString("Qv: ", arialFont, brown, 32 * model.LineLength, 200 + increase);

                    if (model.SideData.Quiver.Length > 39)

                    {
                        g.DrawString(model.SideData.Quiver.Substring(0, 35), arialFont, gray, 32 * model.LineLength + g.MeasureString("Qv: ", arialFont).Width, 200 + increase);
                        g.DrawString(model.SideData.Quiver.Substring(35), arialFont, gray, 32 * model.LineLength + g.MeasureString("Qv: ", arialFont).Width, 220 + increase);
                        increase += 20;
                    }
                    else g.DrawString(model.SideData.Quiver, arialFont, gray, 32 * model.LineLength + g.MeasureString("Qv: ", arialFont).Width, 200 + increase);

                    g.DrawString(model.SideData.Statuses1, arialFont, gray, 32 * model.LineLength, 220 + increase);
                    g.DrawString(model.SideData.Statuses2, arialFont, gray, 32 * model.LineLength, 240 + increase);
                    g.DrawString("Str: ", arialFont, brown, 32 * (model.LineLength + 8), 80);
                    g.DrawString(model.SideData.Strength, arialFont, gray, 32 * (model.LineLength + 8) + g.MeasureString("Str: ", arialFont).Width, 80);
                    g.DrawString("Int: ", arialFont, brown, 32 * (model.LineLength + 8), 100);
                    g.DrawString(model.SideData.Inteligence, arialFont, gray, 32 * (model.LineLength + 8) + g.MeasureString("Int: ", arialFont).Width, 100);
                    g.DrawString("Dex: ", arialFont, brown, 32 * (model.LineLength + 8), 120);
                    g.DrawString(model.SideData.Dexterity, arialFont, gray, 32 * (model.LineLength + 8) + g.MeasureString("Dex: ", arialFont).Width, 120);
                    g.DrawString("Place: ", arialFont, brown, 32 * (model.LineLength + 8), 140);
                    g.DrawString(model.SideData.Place, arialFont, gray, 32 * (model.LineLength + 8) + g.MeasureString("Place: ", arialFont).Width, 140);
                    g.DrawString("Time: ", arialFont, brown, 32 * (model.LineLength + 8), 160);
                    g.DrawString(model.SideData.Time, arialFont, gray, 32 * (model.LineLength + 8) + g.MeasureString("Time: ", arialFont).Width, 160);


                    Bitmap bar = new Bitmap(250, 16);
                    Graphics temp = Graphics.FromImage(bar);
                    temp.Clear(Color.Gray);
                    g.DrawImage(bar, 32 * (model.LineLength + 8), 40);
                    g.DrawImage(bar, 32 * (model.LineLength + 8), 60);
                    int percentage;
                    if (model.SideData.Health > 0)
                    {
                        percentage = (int)(250 * ((float)model.SideData.Health / model.SideData.MaxHealth));
                        Bitmap heathbar = new Bitmap(percentage, 16);
                        temp = Graphics.FromImage(heathbar);
                        temp.Clear(Color.Green);
                        g.DrawImage(heathbar, 32 * (model.LineLength + 8), 40);

                    }
                    if (model.SideData.Magic > 0)
                    {
                        percentage = (int)(250 * ((float)model.SideData.Magic / model.SideData.MaxMagic));
                        Bitmap mana = new Bitmap(percentage, 16);
                        temp = Graphics.FromImage(mana);
                        temp.Clear(Color.Blue);
                        g.DrawImage(mana, 32 * (model.LineLength + 8), 60);
                    }

                    int x = 0;
                    int y = 0;
                    string[] tempo = model.SideData.Place.Split(':');

                    if (!floorandwall.TryGetValue(tempo[0].ToUpper(), out var fnw)) return;
                   // Console.WriteLine(fnw[0] + " " + fnw[1]);
                    if (!alldngnpng.TryGetValue(fnw[0], out var wall)) return;

                    if (!alldngnpng.TryGetValue(fnw[1], out var floor)) return;
                    int i = 1;
                    foreach (var tile in model.TileNames)
                    {
                        if (tile == "#BLUE")
                        {
                            g.DrawImage(wall, x, y, wall.Width, wall.Height);
                            var blueTint = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
                            g.FillRectangle(blueTint, x, y, wall.Width, wall.Height);
                        }
                        else if (tile[0] == '#' && !tile.Equals("#LIGHTCYAN"))
                        {
                            g.DrawImage(wall, x, y, wall.Width, wall.Height);

                        }

                        else if (tile == ".BLUE")
                        {
                            g.DrawImage(floor, x, y, floor.Width, floor.Height);
                            var blueTint = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
                            g.FillRectangle(blueTint, x, y, floor.Width, floor.Height);
                        }
                        else if (tile[0] == '.')
                        {
                            g.DrawImage(floor, x, y, floor.Width, floor.Height);
                        }

                        else if (tile == "*BLUE")
                        {
                            g.DrawImage(wall, x, y, wall.Width, wall.Height);
                            var blueTint = new SolidBrush(Color.FromArgb(50, 0, 0, 200));
                            g.FillRectangle(blueTint, x, y, wall.Width, wall.Height);


                        }
                        else if (tile == ",BLUE")
                        {
                            g.DrawImage(floor, x, y, floor.Width, floor.Height);
                            var blueTint = new SolidBrush(Color.FromArgb(20, 0, 0, 200));
                            g.FillRectangle(blueTint, x, y, floor.Width, floor.Height);
                        }
                        else if (tile[0] == '@')
                        {


                            if (_characterdata.ContainsKey(OnlyRace))
                            {
                                string nam = _characterdata[OnlyRace];
                                if (_characterpng.TryGetValue(nam, out Bitmap chr))
                                {
                                    g.DrawImage(floor, x, y, floor.Width, floor.Height);
                                    g.DrawImage(chr, x, y, chr.Width, chr.Height);
                                }

                            }

                        }
                        else
                        {
                            if (monsterdata.ContainsKey(tile))
                            {
                                string nam = monsterdata[tile];

                                if (nam == "roxanne" && monsterPNG.TryGetValue(nam, out Bitmap mnstr))
                                {                                                                     

                                    g.DrawImage(floor, x, y, floor.Width, floor.Height);
                                    g.DrawImage(mnstr, x, y, mnstr.Width, mnstr.Height);
                                    var blueTint = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
                                    g.FillRectangle(blueTint, x, y, floor.Width, floor.Height);
                                    
                                }

                                else if (monsterPNG.TryGetValue(nam, out mnstr))
                                {
                                    g.DrawImage(floor, x, y, floor.Width, floor.Height);
                                    g.DrawImage(mnstr, x, y, mnstr.Width, mnstr.Height);
                                }
                            }
                            else if (moretiles.ContainsKey(tile))
                            {
                                string nam = moretiles[tile];
                                if (alldngnpng.TryGetValue(nam, out Bitmap chr))
                                {
                                    g.DrawImage(chr, x, y, chr.Width, chr.Height);
                                }

                            }
                            else if (itemdata.ContainsKey(tile))
                            {
                                string nam = itemdata[tile];
                                if (itempng.TryGetValue(nam, out Bitmap chr))
                                {
                                    g.DrawImage(floor, x, y, floor.Width, floor.Height);
                                    g.DrawImage(chr, x, y, chr.Width, chr.Height);
                                }

                            }
                            else if (tile[0] != ' ')
                            {
                                if (!dict.ContainsKey(tile))
                                {
                                    dict.Add(tile, "");
                                }
                                var Color = model.ColorList.GetType().GetField(tile.Substring(1)).GetValue(model.ColorList);
                                g.DrawString(tile[0] + "?", arialFont, (SolidBrush)Color, x, y);
                            }

                         

                        }
                        x += 32;
                        if (i == model.LineLength)
                        {
                            i = 0;
                            x = 0;
                            y += 32;
                        }
                        i++;
                    }

                    for (i = 0; i < model.FullLengthStrings.Length; i++)
                    {
                       // var Color = model.ColorList.GetType().GetField(model.FullLengthStringColors[i]).GetValue(model.ColorList);
                        //g.DrawString(model.FullLengthStrings[i], arialFont, (SolidBrush)Color, 0, y);
                        y += 32;
                    }

                    if (dict.Count < 10)
                    {
                        foreach (var item in dict)
                        {
                           Console.Write(item.Key + " ");
                        }
                        Console.WriteLine();

                    }
                }
                display.Update_Window_Image(bmp);
            }
            GC.Collect();
        }

    }
}
