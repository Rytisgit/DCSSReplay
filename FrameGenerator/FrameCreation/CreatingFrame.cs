using InputParse;
using Putty;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace FrameGenerator.FrameCreation
{

    class CreatingFrame
    {
        public static Bitmap DrawFrame(
            ref Bitmap lastframe,
            ref int previousHP,
            Dictionary<string, Bitmap> wallpng,
            Dictionary<string, Bitmap> floorpng,
            Dictionary<string, Bitmap> itempng,
            Dictionary<string, string> itemdata,
            Dictionary<string, string> moretiles,
            Dictionary<string, string> cloudtiles,
            Dictionary<string, Bitmap> alldngnpng,
            Dictionary<string, string> monsterdata,
            Dictionary<string, Bitmap> monsterPNG,
            Dictionary<string, string[]> floorandwall,
            Dictionary<string, string> _characterdata,
            Dictionary<string, Bitmap> _characterpng,
            TerminalCharacter[,] chars,
            Dictionary<string, Bitmap> _alleffects
            )
        {
            var dict = new Dictionary<string, string>();//logging
            var model = Parser.ParseData(chars);

            Bitmap currentFrame = new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);

            switch (model.Layout)
                {
                case LayoutType.Normal:
                    currentFrame = DrawNormal(wallpng, floorpng, itempng, itemdata, moretiles, cloudtiles, alldngnpng, monsterdata, monsterPNG, floorandwall, _characterdata, _characterpng, dict, model, _alleffects, previousHP);
                    lastframe = currentFrame;
                    previousHP = model.SideData.Health;
                    break;
                case LayoutType.TextOnly:
                    currentFrame = DrawTextBox(lastframe, model);
                    break;
                case LayoutType.MapOnly:
                    currentFrame = DrawMap(lastframe, wallpng, floorpng, itempng, itemdata, moretiles, cloudtiles, alldngnpng, monsterdata, monsterPNG, floorandwall, _characterdata, _characterpng, dict, model, _alleffects);
                    break;
                default:
                        break;                 
                }
            Bitmap forupdate = new Bitmap(currentFrame);
            return forupdate;
        
        }

        private static Bitmap DrawMap(Bitmap lastframe, Dictionary<string, Bitmap> wallpng, Dictionary<string, Bitmap> floorpng, Dictionary<string, Bitmap> itempng, Dictionary<string, string> itemdata, Dictionary<string, string> moretiles, Dictionary<string, string> cloudTiles, Dictionary<string, Bitmap> alldngnpng, Dictionary<string, string> monsterdata, Dictionary<string, Bitmap> monsterPNG, Dictionary<string, string[]> floorandwall, Dictionary<string, string> _characterdata, Dictionary<string, Bitmap> _characterpng, Dictionary<string, string> dict, Model model, Dictionary<string, Bitmap> effects)
        {
            Bitmap bmp = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);

            //Rectangle rect = new Rectangle(0, 0, 1056, 780);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen blackPen = new Pen(Color.Black, 2);
                //g.FillRectangle(new SolidBrush(Color.Black), rect);
                //g.DrawRectangle(blackPen, rect);
                float x = 0;
                float y = 0;
                int i = 1;
                int j = 0;
                for (; j < model.TileNames.Length; j++)
                {
                    var Color = model.ColorList.GetType().GetField(model.TileNames[j].Substring(1)).GetValue(model.ColorList);
                    g.DrawString(model.TileNames[j][0].ToString(), new Font("Courier New", 22), (SolidBrush)Color, x, y);
                    x += 20;
                    if (i == model.LineLength)
                    {
                        break;
                    }
                    i++;
                }

                DrawTiles(0, 32, j+1, resize: 1f, g, model, dict, itempng, itemdata, moretiles, cloudTiles, alldngnpng, monsterdata, monsterPNG, floorandwall, _characterdata, _characterpng, wallpng, floorpng, effects);
            }

            return bmp;
        }

        private static Bitmap DrawTextBox(Bitmap lastframe, Model model)
        {
            Bitmap temp = new Bitmap(lastframe);
            using (Graphics g = Graphics.FromImage(temp))
            {

                Pen blackPen = new Pen(new SolidBrush(Color.FromArgb(255, 125, 98, 60)), 2);
                Rectangle rect2 = new Rectangle(25, 25, 1000, 430);
                g.FillRectangle(new SolidBrush(Color.Black), rect2);
                g.DrawRectangle(blackPen, rect2);


                float x = 50;
                float y = 50;
                int i = 1;
                foreach (var tile in model.TileNames)
                {
                    var Color = model.ColorList.GetType().GetField(tile.Substring(1)).GetValue(model.ColorList);
                    g.DrawString(tile[0].ToString(), new Font("Courier New", 12), (SolidBrush)Color, x, y);
                    x += 12;
                    if (i == model.LineLength)
                    {
                        i = 0;
                        x = 50;
                        y += 16;
                    }
                    i++;
                }
            }

            return temp;
        }

        private static Bitmap DrawNormal(
            Dictionary<string, Bitmap> wallpng,
            Dictionary<string, Bitmap> floorpng,
            Dictionary<string, Bitmap> itempng,
            Dictionary<string, string> itemdata,
            Dictionary<string, string> moretiles,
            Dictionary<string, string> cloudTiles,
            Dictionary<string, Bitmap> alldngnpng,
            Dictionary<string, string> monsterdata,
            Dictionary<string, Bitmap> monsterPNG,
            Dictionary<string, string[]> floorandwall,
            Dictionary<string, string> _characterdata,
            Dictionary<string, Bitmap> _characterpng,
            Dictionary<string, string> dict,
            Model model,
            Dictionary<string, Bitmap> effects,
            int prevHP)
        {
            Bitmap bmp = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                DrawSideDATA(g, model, prevHP);

                DrawTiles(0, 0, 0, resize: 1, g, model, dict, itempng, itemdata, moretiles, cloudTiles, alldngnpng, monsterdata, monsterPNG, floorandwall, _characterdata, _characterpng, wallpng, floorpng, effects);

                DrawMonsterDisplay(g, model, monsterPNG, monsterdata);

                DrawLogs(model, g);
            }

            return bmp;
        }

        private static void DrawMonsterDisplay(Graphics g, Model model, Dictionary<string, Bitmap> monsterPNG, Dictionary<string, string> monsterdata)
        {
            var sideOfTilesX = 32 * model.LineLength; var currentLineY = 300;
            foreach (var monsterlist in model.MonsterData)
            {
                var x = sideOfTilesX;
                if (!monsterlist.empty)
                {
                    foreach (var monster in monsterlist.MonsterDisplay)//draw all monsters in 1 line
                    {
                        if (monsterdata.TryGetValue(monster, out var tileName))
                        {
                            if (monsterPNG.TryGetValue(tileName, out var mnstr))
                            {
                                g.DrawImage(mnstr, x, currentLineY, mnstr.Width, mnstr.Height);
                                x += 32;
                            }
                        }
                    }
                    foreach (var coloredCharacter in monsterlist.MonsterText)//write all text in 1 line
                    {
                        var Color = model.ColorList.GetType().GetField(coloredCharacter.Substring(1)).GetValue(model.ColorList);
                        g.DrawString(coloredCharacter[0].ToString(), new Font("Courier New", 16), (SolidBrush)Color, x, currentLineY + 6);
                        x += 12;
                    }

                    currentLineY += 32;
                }
            }

        }

        private static void DrawLogs(Model model, Graphics g)
        {
            int y = 544;
            for (int i = 0; i < model.FullLengthStrings.Length; i++)
            {
                var Color = model.ColorList.GetType().GetField(model.FullLengthStringColors[i]).GetValue(model.ColorList);
                g.DrawString(model.FullLengthStrings[i], new Font("Courier New", 16), (SolidBrush)Color, 0, y);
                y += 32;
            }
        }

        public static void DrawSideDATA(Graphics g, Model model, int prevHP)

        {
            g.Clear(Color.Black);
            using (Font arialFont = new Font("Courier New", 16))
            {

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
                int barLength;
                if (model.SideData.Health > 0)
                {
                    barLength = (int)(250 * ((float)model.SideData.Health / model.SideData.MaxHealth));
                    Bitmap healthbar = new Bitmap(barLength, 16);
                    temp = Graphics.FromImage(healthbar);
                    temp.Clear(Color.Green);
                    var x = 32 * (model.LineLength + 8);
                    g.DrawImage(healthbar, x, 40);
                    if (barLength != 250 && prevHP - model.SideData.Health > 0)
                    {
                        int prevBarLength = (int)(250 * ((float)(prevHP - model.SideData.Health) / model.SideData.Health));
                        Bitmap losthealthbar = new Bitmap(prevBarLength, 16);
                        Graphics.FromImage(losthealthbar).Clear(Color.Red);
                        g.DrawImage(losthealthbar, x + barLength, 40);
                    }

                }
                if (model.SideData.Magic > 0)
                {
                    barLength = (int)(250 * ((float)model.SideData.Magic / model.SideData.MaxMagic));
                    Bitmap mana = new Bitmap(barLength, 16);
                    temp = Graphics.FromImage(mana);
                    temp.Clear(Color.Blue);
                    g.DrawImage(mana, 32 * (model.LineLength + 8), 60);
                }
            }

        }

        public static void DrawTiles(float x, float y, int j,float resize, Graphics g, Model model, Dictionary<string, string> dict, Dictionary<string, Bitmap> itempng, Dictionary<string, string> itemdata, Dictionary<string, string> moretiles, Dictionary<string, string> cloudTiles, Dictionary<string, Bitmap> alldngnpng, Dictionary<string, string> monsterdata, Dictionary<string, Bitmap> monsterPNG, Dictionary<string, string[]> floorandwall, Dictionary<string, string> _characterdata, Dictionary<string, Bitmap> _characterpng, Dictionary<string, Bitmap> wallpng, Dictionary<string, Bitmap> floorpng, Dictionary<string, Bitmap> effects)

        {

            string OnlyRace = model.SideData.Race.Substring(0, 6);
          
            string[] tempo = model.SideData.Place.Split(':');
            if (!floorandwall.TryGetValue(tempo[0].ToUpper(), out var fnw)) return;
            // Console.WriteLine(fnw[0] + " " + fnw[1]);
            if (!wallpng.TryGetValue(fnw[0], out var wall)) return;

            if (!floorpng.TryGetValue(fnw[1], out var floor)) return;
            int i = 1;

            for (; j < model.TileNames.Length; j++)
            {
                var tile = model.TileNames[j];
                if (tile == "#BLUE")
                {
                    g.DrawImage(wall, x, y, wall.Width * resize, wall.Height * resize);
                    var blueTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                    g.FillRectangle(blueTint, x, y, wall.Width * resize, wall.Height * resize);
                }
                else if (tile[0] == '#' && !tile.Equals("#LIGHTCYAN"))
                {
                    g.DrawImage(wall, x, y, wall.Width * resize, wall.Height * resize);

                }

                else if (tile == ".BLUE")
                {
                    g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                    var blueTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                    g.FillRectangle(blueTint, x, y, floor.Width * resize, floor.Height * resize);
                }
                else if (tile[0] == '.')
                {
                    g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                }

                else if (tile == "*BLUE")
                {
                    g.DrawImage(wall, x, y, wall.Width * resize, wall.Height * resize);
                    var blueTint = new SolidBrush(Color.FromArgb(40, 30, 30, 200));
                    g.FillRectangle(blueTint, x, y, wall.Width * resize, wall.Height * resize);


                }
                else if (tile == ",BLUE")
                {
                    g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                    var blueTint = new SolidBrush(Color.FromArgb(40, 20, 20, 200));
                    g.FillRectangle(blueTint, x, y, floor.Width * resize, floor.Height * resize);
                }
               
                else
                {
                    if (monsterdata.ContainsKey(tile))
                    {
                        string nam = monsterdata[tile];

                        if (nam == "roxanne" && monsterPNG.TryGetValue(nam, out Bitmap mnstr))
                        {

                            g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                            g.DrawImage(mnstr, x, y, mnstr.Width * resize, mnstr.Height * resize);
                            var blueTint = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
                            g.FillRectangle(blueTint, x, y, floor.Width * resize, floor.Height * resize);

                        }

                        else if (monsterPNG.TryGetValue(nam, out mnstr))
                        {
                            g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                            g.DrawImage(mnstr, x, y, mnstr.Width * resize, mnstr.Height * resize);
                        }
                    }
                    else if (moretiles.ContainsKey(tile))
                    {
                        string nam = moretiles[tile];
                        if (alldngnpng.TryGetValue(nam, out Bitmap chr))
                        {
                            g.DrawImage(chr, x, y, chr.Width * resize, chr.Height * resize);
                        }

                    }
                    else if (cloudTiles.ContainsKey(tile))
                    {
                        bool drawn = false;
                            g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                        //special rules first
                        if (model.SideData.Statuses1.Contains("Torna") || model.SideData.Statuses2.Contains("Torna"))//tornado override
                        {
                            var t1Colors = new List<string>() {
                                Enum.GetName(typeof(ColorList2), ColorList2.LIGHTRED),
                                Enum.GetName(typeof(ColorList2), ColorList2.LIGHTCYAN),
                                Enum.GetName(typeof(ColorList2), ColorList2.LIGHTBLUE),
                                Enum.GetName(typeof(ColorList2), ColorList2.WHITE) };
                            var t2Colors = new List<string>() { 
                                Enum.GetName(typeof(ColorList2), ColorList2.RED),
                                Enum.GetName(typeof(ColorList2), ColorList2.CYAN),
                                Enum.GetName(typeof(ColorList2), ColorList2.BLUE),
                                Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY) };
                            foreach (var color in t1Colors)
                            {
                                if (tile.Contains(color))
                                {
                                    if (effects.TryGetValue("tornado1", out Bitmap bmp))
                                    {
                                        g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                    }
                                }
                                
                            }
                            foreach (var color in t2Colors)
                            {
                                if (tile.Contains(color))
                                {
                                    if (effects.TryGetValue("tornado2", out Bitmap bmp))
                                    {
                                        g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                    }
                                }

                            }
                        }
                        else if (model.SideData.Place.Contains("Salt"))
                        {
                            var colors = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY), Enum.GetName(typeof(ColorList2), ColorList2.WHITE) };
                            if (tile[0].Equals('§'))
                            {
                                foreach (var color in colors)
                                {
                                    if (tile.Contains(color))
                                    {
                                        if (effects.TryGetValue("cloud_grey_smoke", out Bitmap bmp))
                                        {
                                            g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                            drawn = true;
                                        }
                                    }
                                }
                            }
                        }
                        else if(model.SideData.Race.Contains("of Qazlal"))
                        {
                            
                            var colors = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY), Enum.GetName(typeof(ColorList2), ColorList2.DARKGREY) };
                            var colors2 = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.GREEN) };
                            if (tile[0].Equals('§'))
                            {
                                foreach (var color in colors)
                                {
                                    if (tile.Contains(color))
                                    {
                                        if (effects.TryGetValue("cloud_storm2", out Bitmap bmp))
                                        {
                                            g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                            drawn = true;
                                        }
                                    }
                                }
                            }
                            var durations = new List<char>() { '°', '○', '☼', '§' };
                            int dur = 0;
                            for (int c = 0; c < durations.Count; c++)
                            {
                                if (tile[0] == durations[c])
                                {
                                    dur = c;
                                }
                            }
                            foreach (var color in colors2)
                            {
                                if (tile.Contains(color))
                                {
                                    if (effects.TryGetValue("cloud_dust" + dur.ToString(), out Bitmap bmp))
                                    {
                                        g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                        drawn = true;
                                    }
                                }
                            }
                        }
                        else if (model.SideData.Place.Contains("Shoal"))
                        {
                            var colors = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.DARKGREY) };
                            if (tile[0].Equals('§'))
                            {
                                foreach (var color in colors)
                                {
                                    if (tile.Contains(color))
                                    {
                                        if (effects.TryGetValue("ink_full", out Bitmap bmp))
                                        {
                                            g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                            drawn = true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (model.SideData.Place.Contains("Dung") || model.SideData.Place.Contains("Lair"))
                        {
                            if (tile[0].Equals("§WHITE"))
                            {
                                if (effects.TryGetValue("cloud_calc_dust2", out Bitmap bmp))
                                {
                                    g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                    drawn = true;
                                }

                            }
                        }
                        else if (model.SideData.Place.Contains("Pand"))//TODO: CHeck For holy enemies?
                        {
                            if (tile[0].Equals("§WHITE"))
                            {
                                if (effects.TryGetValue("cloud_yellow_smoke", out Bitmap bmp))
                                {
                                    g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                    drawn = true;
                                }

                            }
                        }
                        else if (model.SideData.Race.Contains("of Wu Jian"))
                        {
                            var colors = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.WHITE), Enum.GetName(typeof(ColorList2), ColorList2.YELLOW) };
                            var durations = new List<char>() {'°', '○', '☼', '§' };
                            int dur = 0;
                            for (int c = 0; c < durations.Count; c++)
                            {
                                if (tile[0]==durations[c])
                                {
                                    dur = c;
                                }
                            }
                            foreach (var color in colors)
                                {
                                    if (tile.Contains(color))
                                    {
                                        if (effects.TryGetValue("cloud_gold_dust" + dur.ToString(), out Bitmap bmp))
                                        {
                                            g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                            drawn = true;
                                        }
                                    }
                                }
                            
                        }

                            string nam = cloudTiles[tile];
                            if (!drawn && effects.TryGetValue(nam, out Bitmap chr))
                            {
                                g.DrawImage(chr, x, y, chr.Width * resize, chr.Height * resize);
                            }

                    }
                    else if (tile[0] == '@')
                    {


                        if (_characterdata.ContainsKey(OnlyRace))
                        {
                            string nam = _characterdata[OnlyRace];
                            if (_characterpng.TryGetValue(nam, out Bitmap chr))
                            {
                                g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                                g.DrawImage(chr, x, y, chr.Width * resize, chr.Height * resize);
                            }

                        }

                    }
                    else if (itemdata.ContainsKey(tile))
                    {
                        string nam = itemdata[tile];
                        if (itempng.TryGetValue(nam, out Bitmap chr))
                        {
                            g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                            g.DrawImage(chr, x, y, chr.Width * resize, chr.Height * resize);
                        }

                    }
                    else if (tile[0] != ' ')
                    {
                        if (!dict.ContainsKey(tile))
                        {
                            dict.Add(tile, "");
                        }
                        var Color = model.ColorList.GetType().GetField(tile.Substring(1)).GetValue(model.ColorList);
                        g.DrawString(tile[0] + "?", new Font("Courier New", 16), (SolidBrush)Color, x, y);
                    }



                }
                x += 32 * resize;
                if (i == model.LineLength)
                {
                    i = 0;
                    x = 0;
                    y += 32 * resize;
                }
                i++;
            }

            if (dict.Count < 10)
            {
                bool written = false;
                foreach (var item in dict)
                {
                    if (!string.IsNullOrEmpty(item.Key))written = true;
                    Console.Write(item.Key + " ");
                }
                if (written)
                {
                    Console.WriteLine();
                }

            }
        }
    }
}
