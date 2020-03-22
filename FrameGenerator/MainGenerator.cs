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
        private readonly Dictionary<string, string> _features;
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
            _features = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\features.txt");
            _cloudtiles = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\clouds.txt");
            _itemdata = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\items.txt");
            
            _floorandwall = ReadFromFile.GetFloorAndWallNamesForDungeons(@"..\..\..\Extra\tilefloor.txt");
            _monsterdata = ReadFromFile.GetMonsterData(gameLocation + @"\mon-data.h");

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
           
            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (var font = new Font("Courier New", 22))
                {
                    float x = 0;
                    float y = 0;
                    for (int j = 0; j < model.LineLength; j++)//write out first line as text
                    {
                        g.WriteCharacter(model.TileNames[j], font, x, y);
                        x += 20;
                    }

                    DrawTiles(g, model, 0, 32, model.LineLength, resize: 1f);//draw the rest of the map
                }
            }

            return bmp;
        }

        private Bitmap DrawTextBox(Model model, Bitmap lastframe)
        {
            Bitmap overlayImage = new Bitmap(lastframe);

            using (Graphics g = Graphics.FromImage(overlayImage))
            {
                using (var font = new Font("Courier New", 12))
                {
                    var darkPen = new Pen(new SolidBrush(Color.FromArgb(255, 125, 98, 60)), 2);
                    Rectangle rect2 = new Rectangle(25, 25, 1000, 430);
                    g.DrawRectangle(darkPen, rect2);
                    g.FillRectangle(new SolidBrush(Color.Black), rect2);

                    float x = 50;
                    float y = 34;
                    for (int i = 0; i < model.TileNames.Length; i++)
                    {
                        if (i % model.LineLength == 0)//next line
                        {
                            x = 50;
                            y += 16;
                        }
                        g.WriteCharacter(model.TileNames[i], font, x, y);
                        x += 12;
                    }
                }
            }

            return overlayImage;
        }

        private Bitmap DrawNormal(Model model, int prevHP)
        {
            Bitmap newFrame = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newFrame))
            {
                g.Clear(Color.Black);

                DrawSideDATA(g, model, prevHP);

                DrawTiles(g, model, 0, 0, 0, resize: 1);

                DrawMonsterDisplay(g, model);

                DrawLogs(g, model);
            }

            return newFrame;
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
                            }
                            else graphics.WriteCharacter(monster, font, x, currentLineY);//not found write as string
                            x += 32;
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

        private static void DrawLogs(Graphics g, Model model)
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
                var gray = new SolidBrush(Color.FromArgb(186, 189, 182));

                var lineCount = 0;
                var lineHeight = 20;

                g.DrawString(model.SideData.Name, font, yellow, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;
                g.DrawString(model.SideData.Race, font, yellow, 32 * model.LineLength, lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("Health: ", model.SideData.Health.ToString() + '/' + model.SideData.MaxHealth.ToString(), font, 32 * model.LineLength, lineCount * lineHeight)
                .DrawPercentageBar(model.SideData.Health, model.SideData.MaxHealth, Color.Green, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("Mana: ", model.SideData.Magic.ToString() + '/' + model.SideData.MaxMagic.ToString(), font, 32 * model.LineLength, lineCount * lineHeight)
                .DrawPercentageBar(model.SideData.Magic, model.SideData.MaxMagic, Color.Blue, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("AC: ", model.SideData.ArmourClass, font, 32 * model.LineLength, lineCount * lineHeight)
                .WriteSideDataInfo("Str: ", model.SideData.Strength, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("EV: ", model.SideData.Evasion, font, 32 * model.LineLength, lineCount * lineHeight)
                .WriteSideDataInfo("Int: ", model.SideData.Inteligence, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("SH: ", model.SideData.Shield, font, 32 * model.LineLength, lineCount * lineHeight)
                .WriteSideDataInfo("Dex: ", model.SideData.Dexterity, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("XL: ", model.SideData.ExperienceLevel, font, 32 * model.LineLength, lineCount * lineHeight)
                .WriteSideDataInfo(" Next: ", model.SideData.ExperienceLevel, font, 32 * model.LineLength + g.MeasureString("XL: " + model.SideData.ExperienceLevel, font).Width, lineCount * lineHeight)
                .WriteSideDataInfo("Place: ", model.SideData.Place, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
                lineCount++;
                g.WriteSideDataInfo("Noise:", "noise here", font, 32 * model.LineLength, lineCount * lineHeight)
                .WriteSideDataInfo("Time: ", model.SideData.Time, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
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

        public void DrawTiles(Graphics g, Model model, float startX, float startY, int startIndex, float resize)

        {
            var dict = new Dictionary<string, string>();//logging

            string characterRace = model.SideData.Race.Substring(0, 6);
            string[] location = model.SideData.Place.Split(':');

            if (!_floorandwall.TryGetValue(location[0].ToUpper(), out var CurrentLocationFloorAndWallName)) return;

            if (!_wallpng.TryGetValue(CurrentLocationFloorAndWallName[0], out var wall)) return;
            if (!_floorpng.TryGetValue(CurrentLocationFloorAndWallName[1], out var floor)) return;

            var currentTileX = startX;
            var currentTileY = startY;
            if (startIndex == 0) currentTileY -= 32 * resize;//since start of loop begins on a newline we back up one so it isn't one line too low.

            for (int i = startIndex; i < model.TileNames.Length; i++)
            {
                if (i % model.LineLength == 0)
                {
                    currentTileX = 0;
                    currentTileY += 32 * resize;
                }
                else
                    currentTileX += 32 * resize;
                //TODO if location is middle and tile name starts with @ draw character
                DrawCurrentTile(g, model, dict, characterRace, wall, floor, currentTileX, currentTileY, model.TileNames[i]);
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

        private void DrawCurrentTile(Graphics g, Model model, Dictionary<string, string> dict, string OnlyRace, Bitmap wall, Bitmap floor, float x, float y, string tile)
        {
            if (g.TryDrawWallOrFloor(tile, wall, floor, x, y)) return;

            if (g.TryDrawMonster(tile, _monsterdata, _monsterpng, floor, x, y)) return;

            if (g.TryDrawFeature(tile, _features, _alldngnpng, floor, x, y)) return;

            if (g.TryDrawCloud(tile, _cloudtiles, _alleffects, floor, model.SideData, model.MonsterData, x, y)) return;

            if (g.TryDrawPlayer(tile, _characterdata, _characterpng, floor, OnlyRace, x, y)) return;//TODO player drawing should not be here any more

            if (g.TryDrawItem(tile, _itemdata, _itempng, floor, x, y)) return;

            else if (tile[0] != ' ')//unhandled tile, write it as a character instead
            {
                if (!dict.ContainsKey(tile))
                {
                    dict.Add(tile, "");
                }

                g.WriteCharacter(tile, new Font("Courier New", 16), x, y);
            }
        }
    }
}



