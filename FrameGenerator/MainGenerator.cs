using FrameGenerator.FileReading;
using FrameGenerator.Extensions;
using InputParse;
using Putty;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Window;

namespace FrameGenerator
{
    public class MainGenerator
    {
        Widow_Display display = new Widow_Display();
        private readonly Dictionary<string, string> _monsterdata;
        private readonly Dictionary<string, string> _characterdata;
        private readonly Dictionary<string, string[]> _floorandwall;
        private readonly Dictionary<string, string> _moretiles;
        private readonly Dictionary<string, string> _cloudtiles;
        private readonly Dictionary<string, string> _itemdata;
        private readonly Dictionary<string, Bitmap> _monsterpng;
        private readonly Dictionary<string, Bitmap> _characterpng;
        private readonly Dictionary<string, Bitmap> _itempng;
        private readonly Dictionary<string, Bitmap> _alldngnpng;
        private readonly Dictionary<string, Bitmap> _alleffects;
        private readonly Dictionary<string, Bitmap> _floorpng;
        private readonly Dictionary<string, Bitmap> _wallpng;
        private Bitmap _lastframe = new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);
        private int previousHP = 0;

        public MainGenerator()
        {

            string gameLocation = File.ReadAllLines(display.Folder + @"\config.ini").First();

            _characterdata = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\racepng.txt");
            _moretiles = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\features.txt");
            _cloudtiles = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\clouds.txt");
            _itemdata = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\items.txt");
            
            _floorandwall = ReadFromFile.GetFloorAndWallNamesForDungeons();

            _monsterdata = ReadFromFile.GetMonsterData(gameLocation);

            _floorpng = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\dngn\floor");
            _wallpng = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\dngn\wall");
            _alldngnpng = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\dngn");
            _alleffects = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\effect");

            _characterpng = ReadFromFile.GetCharacterPNG(gameLocation);
            _monsterpng = ReadFromFile.GetMonsterPNG(gameLocation);
            _itempng = ReadFromFile.ItemsPNG(gameLocation);

        }

        public void GenerateImage(TerminalCharacter[,] chars)
        {
            if (chars != null) {
                var model = Parser.ParseData(chars);

                var image = DrawFrame(model);

                display.Update_Window_Image(image);
                GC.Collect();
            }
            return;
        }

        private Bitmap DrawFrame(Model model)
        {
            Bitmap currentFrame = new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);

            switch (model.Layout)
            {
                case LayoutType.Normal:
                    currentFrame = DrawNormal(model, previousHP);
                    _lastframe = currentFrame;
                    previousHP = model.SideData.Health;
                    break;
                case LayoutType.TextOnly:
                    currentFrame = DrawTextBox(model, _lastframe);
                    break;
                case LayoutType.MapOnly:
                    currentFrame = DrawMap(model);
                    break;
                default:
                    break;
            }
            Bitmap forupdate = new Bitmap(currentFrame);
            return forupdate;

        }

        private Bitmap DrawMap(Model model)
        {
            Bitmap bmp = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            var font = new Font("Courier New", 22);
            //Rectangle rect = new Rectangle(0, 0, 1056, 780);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                //Pen blackPen = new Pen(Color.Black, 2);
                //g.FillRectangle(new SolidBrush(Color.Black), rect);
                //g.DrawRectangle(blackPen, rect);
                float x = 0;
                float y = 0;
                int i = 1;
                int j = 0;
                for (; j < model.TileNames.Length; j++)
                {
                    g.WriteCharacter(model.TileNames[j], font, x, y);
                    x += 20;
                    if (i == model.LineLength)
                    {
                        break;
                    }
                    i++;
                }

                DrawTiles(g, model, 0, 32, j + 1, resize: 1f);
            }

            return bmp;
        }

        private Bitmap DrawTextBox(Model model, Bitmap lastframe)
        {
            Bitmap temp = new Bitmap(lastframe);
            using (Graphics g = Graphics.FromImage(temp))
            {

                var font = new Font("Courier New", 12);
                Pen darkPen = new Pen(new SolidBrush(Color.FromArgb(255, 125, 98, 60)), 2);
                Rectangle rect2 = new Rectangle(25, 25, 1000, 430);
                g.FillRectangle(new SolidBrush(Color.Black), rect2);
                g.DrawRectangle(darkPen, rect2);

                float x = 50;
                float y = 50;
                int i = 1;
                foreach (var tile in model.TileNames)
                {
                    g.WriteCharacter(tile, font, x, y);
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

        private Bitmap DrawNormal(Model model, int prevHP)
        {
            Bitmap bmp = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);

                DrawSideDATA(g, model, prevHP);

                DrawTiles(g, model, 0, 0, 0, resize: 1);

                DrawMonsterDisplay(g, model);

                DrawLogs(model, g);
            }

            return bmp;
        }

        private void DrawMonsterDisplay(Graphics graphics, Model model)
        {
            
            var sideOfTilesX = 32 * model.LineLength; var currentLineY = 300;
            var font = new Font("Courier New", 16);
            foreach (var monsterlist in model.MonsterData)
            {
                var x = sideOfTilesX;
                if (!monsterlist.empty)
                {

                    foreach (var monster in monsterlist.MonsterDisplay)//draw all monsters in 1 line
                    {
                        if (_monsterdata.TryGetValue(monster, out var tileName))
                        {
                            if (_monsterpng.TryGetValue(tileName, out var mnstr))
                            {
                                graphics.DrawImage(mnstr, x, currentLineY, mnstr.Width, mnstr.Height);
                                x += 32;
                            }
                        }
                    }
                    var otherx = x;
                    foreach (var backgroundColor in monsterlist.MonsterBackground.Skip(monsterlist.MonsterDisplay.Length))
                    {
                        graphics.PaintBackground(backgroundColor, font, otherx, currentLineY + 4);
                        otherx += 12;
                    }

                    foreach (var coloredCharacter in monsterlist.MonsterText)//write all text in 1 line
                    {
                        graphics.WriteCharacter(coloredCharacter, font, x, currentLineY + 4);
                        x += 12;
                    }

                    currentLineY += 32;
                }
            }
        }

        private static void DrawLogs(Model model, Graphics g)
        {
            int y = 544;
            var font = new Font("Courier New", 16);
            for (int i = 0; i < model.LogData.Length; i++)
            {
                if (!model.LogData[i].empty)
                {
                    for(int charIndex = 0; charIndex < model.LogData[i].LogTextRaw.Length; charIndex++) { 
                        g.WriteCharacter(model.LogData[i].LogText[charIndex], font, charIndex * 12, y);
                    }
                }                
                y += 32;
            }
        }

        public static void DrawSideDATA(Graphics g, Model model, int prevHP)

        {
            using (Font font = new Font("Courier New", 16))
            {

                var yellow = new SolidBrush(Color.FromArgb(252, 233, 79));
                var brown = new SolidBrush(Color.FromArgb(143, 89, 2));
                var gray = new SolidBrush(Color.FromArgb(186, 189, 182));

                var lineCount = 0;
                var lineHeight = 20;

                g.DrawString(model.SideData.Name, font, yellow, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;
                g.DrawString(model.SideData.Race, font, yellow, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("Health: ", model.SideData.Health.ToString() + '/' + model.SideData.MaxHealth.ToString(), font, 32 * model.LineLength, lineCount * lineHeight);
                g.DrawPercentageBar(model.SideData.Health, model.SideData.MaxHealth, Color.Green, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("Mana: ", model.SideData.Magic.ToString() + '/' + model.SideData.MaxMagic.ToString(), font, 32 * model.LineLength, lineCount * lineHeight);
                g.DrawPercentageBar(model.SideData.Magic, model.SideData.MaxMagic, Color.Blue, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("AC: ", model.SideData.ArmourClass, font, 32 * model.LineLength, lineCount * lineHeight);
                g.WriteSideDataInfo("Str: ", model.SideData.Strength, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("EV: ", model.SideData.Evasion, font, 32 * model.LineLength, lineCount * lineHeight);
                g.WriteSideDataInfo("Int: ", model.SideData.Inteligence, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("SH: ", model.SideData.Shield, font, 32 * model.LineLength, lineCount * lineHeight);
                g.WriteSideDataInfo("Dex: ", model.SideData.Dexterity, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("XL: ", model.SideData.ExperienceLevel, font, 32 * model.LineLength, lineCount * lineHeight);
                g.WriteSideDataInfo(" Next: ", model.SideData.ExperienceLevel, font, 32 * model.LineLength + g.MeasureString("XL: " + model.SideData.ExperienceLevel, font).Width, lineCount * lineHeight);
                g.WriteSideDataInfo("Place: ", model.SideData.Place, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("Noise:", "noise here", font, 32 * model.LineLength, lineCount * lineHeight);
                g.WriteSideDataInfo("Time: ", model.SideData.Time, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;

                g.WriteSideDataInfo("Wp: ", model.SideData.Weapon.Substring(0, 35), font, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;
                if (model.SideData.Weapon.Length > 39)
                {
                    g.DrawString(model.SideData.Weapon.Substring(35), font, gray, 32 * model.LineLength + g.MeasureString("Wp: ", font).Width, lineCount * lineHeight);
                    lineCount++;
                }

                g.WriteSideDataInfo("Qv: ", model.SideData.Quiver.Substring(0, 35), font, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;
                if (model.SideData.Quiver.Length > 39)
                {
                    g.DrawString(model.SideData.Quiver.Substring(35), font, gray, 32 * model.LineLength + g.MeasureString("Qv: ", font).Width, lineCount * lineHeight);
                    lineCount++;
                }

                // TODO better status writing
                g.DrawString(model.SideData.Statuses1, font, gray, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;
                g.DrawString(model.SideData.Statuses2, font, gray, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;

            }

        }

        public void DrawTiles(Graphics g, Model model, float x, float y, int j, float resize)

        {
            var dict = new Dictionary<string, string>();//logging
            string OnlyRace = model.SideData.Race.Substring(0, 6);

            string[] tempo = model.SideData.Place.Split(':');
            if (!_floorandwall.TryGetValue(tempo[0].ToUpper(), out var fnw)) return;
            // Console.WriteLine(fnw[0] + " " + fnw[1]);
            if (!_wallpng.TryGetValue(fnw[0], out var wall)) return;

            if (!_floorpng.TryGetValue(fnw[1], out var floor)) return;
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
                    if (_monsterdata.ContainsKey(tile))
                    {
                        string nam = _monsterdata[tile];

                        if (nam == "roxanne" && _monsterpng.TryGetValue(nam, out Bitmap mnstr))
                        {

                            g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                            g.DrawImage(mnstr, x, y, mnstr.Width * resize, mnstr.Height * resize);
                            var blueTint = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
                            g.FillRectangle(blueTint, x, y, floor.Width * resize, floor.Height * resize);

                        }

                        else if (_monsterpng.TryGetValue(nam, out mnstr))
                        {
                            g.DrawImage(floor, x, y, floor.Width * resize, floor.Height * resize);
                            g.DrawImage(mnstr, x, y, mnstr.Width * resize, mnstr.Height * resize);
                        }
                    }
                    else if (_moretiles.ContainsKey(tile))
                    {
                        string nam = _moretiles[tile];
                        if (_alldngnpng.TryGetValue(nam, out Bitmap chr))
                        {
                            g.DrawImage(chr, x, y, chr.Width * resize, chr.Height * resize);
                        }

                    }
                    else if (_cloudtiles.ContainsKey(tile))
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
                                    if (_alleffects.TryGetValue("tornado1", out Bitmap bmp))
                                    {
                                        g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                    }
                                }

                            }
                            foreach (var color in t2Colors)
                            {
                                if (tile.Contains(color))
                                {
                                    if (_alleffects.TryGetValue("tornado2", out Bitmap bmp))
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
                                        if (_alleffects.TryGetValue("cloud_grey_smoke", out Bitmap bmp))
                                        {
                                            g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                            drawn = true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (model.SideData.Race.Contains("of Qazlal"))
                        {

                            var colors = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY), Enum.GetName(typeof(ColorList2), ColorList2.DARKGREY) };
                            var colors2 = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.GREEN) };
                            if (tile[0].Equals('§'))
                            {
                                foreach (var color in colors)
                                {
                                    if (tile.Contains(color))
                                    {
                                        if (_alleffects.TryGetValue("cloud_storm2", out Bitmap bmp))
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
                                    if (_alleffects.TryGetValue("cloud_dust" + dur.ToString(), out Bitmap bmp))
                                    {
                                        g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                        drawn = true;
                                    }
                                }
                            }
                        }
                        else if (!model.MonsterData[0].empty && model.MonsterData[0].MonsterTextRaw.Contains("catob") ||
                            !model.MonsterData[1].empty && model.MonsterData[1].MonsterTextRaw.Contains("catob") ||
                            !model.MonsterData[2].empty && model.MonsterData[2].MonsterTextRaw.Contains("catob") ||
                            !model.MonsterData[3].empty && model.MonsterData[3].MonsterTextRaw.Contains("catob"))
                        {
                            if (tile.Equals("§WHITE"))
                            {
                                if (_alleffects.TryGetValue("cloud_calc_dust2", out Bitmap bmp))
                                {
                                    g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                    drawn = true;
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
                                        if (_alleffects.TryGetValue("ink_full", out Bitmap bmp))
                                        {
                                            g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                            drawn = true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (model.SideData.Place.Contains("Pand"))//TODO: CHeck For holy enemies?
                        {
                            if (tile[0].Equals("§WHITE"))
                            {
                                if (_alleffects.TryGetValue("cloud_yellow_smoke", out Bitmap bmp))
                                {
                                    g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                    drawn = true;
                                }

                            }
                        }
                        else if (model.SideData.Race.Contains("of Wu Jian"))
                        {
                            var colors = new List<string>() { Enum.GetName(typeof(ColorList2), ColorList2.WHITE), Enum.GetName(typeof(ColorList2), ColorList2.YELLOW) };
                            var durations = new List<char>() { '°', '○', '☼', '§' };
                            int dur = 0;
                            for (int c = 0; c < durations.Count; c++)
                            {
                                if (tile[0] == durations[c])
                                {
                                    dur = c;
                                }
                            }
                            foreach (var color in colors)
                            {
                                if (tile.Contains(color))
                                {
                                    if (_alleffects.TryGetValue("cloud_gold_dust" + dur.ToString(), out Bitmap bmp))
                                    {
                                        g.DrawImage(bmp, x, y, bmp.Width * resize, bmp.Height * resize);
                                        drawn = true;
                                    }
                                }
                            }

                        }

                        string nam = _cloudtiles[tile];
                        if (!drawn && _alleffects.TryGetValue(nam, out Bitmap chr))
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
                    else if (_itemdata.ContainsKey(tile))
                    {
                        string nam = _itemdata[tile];
                        if (_itempng.TryGetValue(nam, out Bitmap chr))
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
                        g.DrawString(tile[0].ToString(), new Font("Courier New", 16), (SolidBrush)Color, x, y);
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
                    if (!string.IsNullOrEmpty(item.Key)) written = true;
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



