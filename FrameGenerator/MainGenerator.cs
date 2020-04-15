using FrameGenerator.Extensions;
using FrameGenerator.FileReading;
using FrameGenerator.Models;
using FrameGenerator.OutOfSightCache;
using InputParser;
using Putty;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace FrameGenerator
{
    public class MainGenerator
    {
        public string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DCSSReplay");
        public bool isGeneratingFrame = false;
        private readonly Dictionary<string, string> _monsterdata;
        private readonly List<NamedMonsterOverride> _namedMonsterOverrideData;
        private readonly Dictionary<string, string> _characterdata;
        private readonly Dictionary<string, string[]> _floorandwall;
        private readonly Dictionary<string, string> _features;
        private readonly Dictionary<string, string> _cloudtiles;
        private readonly Dictionary<string, string> _itemdata;
        private readonly Dictionary<string, string> _weapondata;
        private readonly Dictionary<string, Bitmap> _monsterpng;
        private readonly Cacher _outOfSightCache;
        private readonly Dictionary<string, Bitmap> _characterpng;
        private readonly Dictionary<string, Bitmap> _weaponpng;
        private readonly Dictionary<string, Bitmap> _itempng;
        private readonly Dictionary<string, Bitmap> _alldngnpng;
        private readonly Dictionary<string, Bitmap> _alleffects;
        private readonly Dictionary<string, Bitmap> _miscallaneous;
        private readonly Dictionary<string, Bitmap> _floorpng;
        private readonly Dictionary<string, Bitmap> _wallpng;
        private Bitmap _lastframe = new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);
        private int previousHP = 0;
        public static Bitmap CharacterBitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        string CharacterLocationRecognition;

        public MainGenerator()
        {

            string gameLocation = @"..\..\..\Extra";

            _characterdata = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\racepng.txt");
            _features = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\features.txt");
            _cloudtiles = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\clouds.txt");
            _itemdata = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\items.txt");
            _weapondata = ReadFromFile.GetDictionaryFromFile(@"..\..\..\Extra\weapons.txt");

            _floorandwall = ReadFromFile.GetFloorAndWallNamesForDungeons(@"..\..\..\Extra\tilefloor.txt");
            _monsterdata = ReadFromFile.GetMonsterData(gameLocation + @"\mon-data.h", @"..\..\..\Extra\monsteroverrides.txt");
            _namedMonsterOverrideData = ReadFromFile.GetNamedMonsterOverrideData(@"..\..\..\Extra\namedmonsteroverrides.txt");

            _floorpng = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\dngn\floor");
            _wallpng = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\dngn\wall");
            _alldngnpng = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\dngn");
            _alleffects = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\effect");
            _miscallaneous = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\misc");
            _itempng = ReadFromFile.GetBitmapDictionaryFromFolder(gameLocation + @"\rltiles\item");

            _characterpng = ReadFromFile.GetCharacterPNG(gameLocation);
            _monsterpng = ReadFromFile.GetMonsterPNG(gameLocation);

            _outOfSightCache = new Cacher();
            _weaponpng = ReadFromFile.GetWeaponPNG(gameLocation);
        }

        public Bitmap GenerateImage(TerminalCharacter[,] chars)
        {
            if (chars != null)
            {
                var model = Parser.ParseData(chars);

                var image = DrawFrame(model);

#if false //Memory Limit Mode
                GC.Collect();
#endif
                return image;
            }
            return null;
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

        private Dictionary<string, string> GetOverridesForFrame(MonsterData[] monsters, string location)
        {
            var finalOverrides = new Dictionary<string, string>();
            foreach (var monsterLine in monsters)
            {
                if (!monsterLine.Empty)
                {
                    var rules = _namedMonsterOverrideData.Where((o) =>
                    {
                        if (string.IsNullOrWhiteSpace(o.Name)) return false;
                        else return monsterLine.MonsterTextRaw.Contains(o.Name.Substring(0, o.Name.Length - 2));
                    });
                    foreach (var rule in rules.ToList())
                    {
                        if (string.IsNullOrWhiteSpace(rule.Location) || rule.Location == location)
                        {
                            foreach (var tileOverride in rule.TileNameOverrides)
                            {
                                finalOverrides.Add(tileOverride.Key, tileOverride.Value);
                            }
                        }
                    }
                }
            }
            foreach (var rule in _namedMonsterOverrideData)//no monster name in rule
            {
                if (string.IsNullOrWhiteSpace(rule.Name) && rule.Location == location)
                {
                    foreach (var tileOverride in rule.TileNameOverrides)
                    {
                        finalOverrides.Add(tileOverride.Key, tileOverride.Value);
                    }
                }
            }
            return finalOverrides;
        }

        private Bitmap DrawMap(Model model)
        {
            Bitmap bmp = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                using var font = new Font("Courier New", 22);
                float x = 0;
                float y = 0;
                for (int j = 0; j < model.LineLength; j++)//write out first line as text
                {
                    g.WriteCharacter(model.TileNames[j], font, x, y);
                    x += 20;
                }
                var overrides = GetOverridesForFrame(model.MonsterData, model.Location);
                DrawTiles(g, model, 0, 32, model.LineLength, overrides, true);//draw the rest of the map
            }

            return bmp;
        }

        private Bitmap DrawTextBox(Model model, Bitmap lastframe)
        {
            Bitmap overlayImage = new Bitmap(lastframe);

            using (Graphics g = Graphics.FromImage(overlayImage))
            {
                using var font = new Font("Courier New", 12);
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

            return overlayImage;
        }

        private Bitmap DrawNormal(Model model, int prevHP)
        {
            Bitmap newFrame = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newFrame))
            {
                g.Clear(Color.Black);

                var overrides = GetOverridesForFrame(model.MonsterData, model.Location);

                DrawSideDATA(g, model, prevHP);

                DrawTiles(g, model, 0, 0, 0, overrides, false);

                DrawMonsterDisplay(g, model, overrides);

                DrawLogs(g, model);

                DrawConsole(g, model);

            }

            return newFrame;
        }

        private void DrawConsole(Graphics g, Model model)
        {
            float currentTileY = 468;
            float currentTileX = 1075;
            using var font2 = new Font("Courier New", 16);

            for (var i = 0; i < model.TileNames.Length; i++)
            {

                if (i % model.LineLength == 0)
                {
                    currentTileX = 1065;
                    currentTileY += 16;
                }
                else currentTileX += 16;

                g.WriteCharacter(model.TileNames[i], font2, currentTileX, currentTileY, model.HighlightColors[i]);


            }
        }

        private void DrawMonsterDisplay(Graphics g, Model model, Dictionary<string, string> overrides)
        {

            var sideOfTilesX = 32 * model.LineLength; var currentLineY = 300;
            using var font = new Font("Courier New", 16);
            foreach (var monsterlist in model.MonsterData)
            {
                var x = sideOfTilesX;
                if (!monsterlist.Empty)
                {
                    for (int i = 0; i < monsterlist.MonsterDisplay.Length; i++)
                    {
                        if (monsterlist.MonsterDisplay[i].TryDrawMonster(monsterlist.MonsterBackground[i], _monsterdata, _monsterpng, overrides, out Bitmap tileToDraw))
                        {
                            g.DrawImage(tileToDraw, x, currentLineY);
                        }
                        else
                        {
                            g.WriteCharacter(monsterlist.MonsterDisplay[i], font, x, currentLineY);//not found write as string
                        }
                        x += 32;
                    }
                    foreach (var monster in monsterlist.MonsterDisplay)//draw all monsters in 1 line
                    {


                    }
                    var otherx = x;
                    foreach (var backgroundColor in monsterlist.MonsterBackground.Skip(monsterlist.MonsterDisplay.Length))
                    {
                        g.PaintBackground(backgroundColor, font, otherx, currentLineY + 4);
                        otherx += 12;
                    }

                    foreach (var coloredCharacter in monsterlist.MonsterText)//write all text in 1 line
                    {
                        g.WriteCharacter(coloredCharacter, font, x, currentLineY + 4);
                        x += 12;
                    }

                    currentLineY += 32;
                }
            }
        }

        private static void DrawLogs(Graphics g, Model model)
        {
            int y = 544;
            using var font = new Font("Courier New", 16);
            for (int i = 0; i < model.LogData.Length; i++)
            {
                if (!model.LogData[i].Empty)
                {
                    for (int charIndex = 0; charIndex < model.LogData[i].LogTextRaw.Length; charIndex++)
                    {
                        g.WriteCharacter(model.LogData[i].LogText[charIndex], font, charIndex * 12, y);
                    }
                }
                y += 32;
            }
        }

        public static void DrawSideDATA(Graphics g, Model model, int prevHP)

        {
            using Font font = new Font("Courier New", 16);
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

        

        public bool isWallOrFloor(string tilename) => tilename[0] == '#' || tilename[0] == '.' || tilename[0] == ',' || tilename[0] == '*' || tilename[0] == '≈';

        public void DrawTiles(Graphics g, Model model, float startX, float startY, int startIndex, Dictionary<string, string> overrides, bool ForMap)

        {
            var dict = new Dictionary<string, string>();//logging
            var BitmapList = new List<Tuple<string, Bitmap>>(model.TileNames.Length);

            string characterRace = model.SideData.Race.Substring(0, 6);
            string[] location = model.SideData.Place.Split(':');

            if (!_floorandwall.TryGetValue(location[0].ToUpper(), out var CurrentLocationFloorAndWallName)) return;

            if (!_wallpng.TryGetValue(CurrentLocationFloorAndWallName[0], out var wall)) return;
            if (!_floorpng.TryGetValue(CurrentLocationFloorAndWallName[1], out var floor)) return;

            var currentTileX = startX;
            var currentTileY = startY;

           

            if (startIndex == 0) currentTileY -= 32;//since start of loop begins on a newline we back up one so it isn't one line too low.

            for (int i = startIndex; i < model.TileNames.Length; i++)
            {
                if (i % model.LineLength == 0)
                {
                    currentTileX = 0;
                    currentTileY += 32;
                }
                else
                    currentTileX += 32;
                //TODO if location is middle and tile name starts with @ draw character
                DrawCurrentTile(g, model, dict, model.TileNames[i], model.HighlightColors[i], wall, floor, overrides, currentTileX, currentTileY, out Bitmap drawnTile);

                if (!isWallOrFloor(model.TileNames[i]))
                {
                    BitmapList.Add(new Tuple<string, Bitmap>(model.TileNames[i], drawnTile));
                }
                
            }
            _outOfSightCache.UpdateCache(BitmapList);

           if(!ForMap) g.TryDrawPlayer( _characterdata, _characterpng, floor, characterRace, 32 * 16, 32 * 8, _weaponpng, model.SideData.Weapon.ToLower(), ref CharacterBitmap, model.SideData.Statuses1.ToLower(), _weapondata);
#if DEBUG
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
#endif
        }

        private bool DrawCurrentTile(Graphics g, Model model, Dictionary<string, string> dict, string tile, string tileHighlight, Bitmap wall, Bitmap floor, Dictionary<string, string> overrides, float x, float y, out Bitmap drawnTile)
        {
            if (tile[0] == ' ' && (tileHighlight == Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY) || tileHighlight == Enum.GetName(typeof(ColorList2), ColorList2.BLACK)) || tile.StartsWith("@BL")) { 
                drawnTile = null; 
                return false;
            }

            //if blue its cache time
            if(tile.Substring(1) == Enum.GetName(typeof(ColorList2), ColorList2.BLUE) && _outOfSightCache.TryGetLastSeenBitmapByChar(tile[0], out var lastSeen))
            {
                drawnTile = lastSeen;
                g.DrawImage(lastSeen, x, y);
                var outOfSightTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                g.FillRectangle(outOfSightTint, x, y, lastSeen.Width, lastSeen.Height);
                return true;
            }

            if (tile.TryDrawWallOrFloor(wall, floor, out drawnTile) ||
                tile.TryDrawMonster(tileHighlight, _monsterdata, _monsterpng, overrides, floor, out drawnTile) ||
                tile.TryDrawFeature(_features, _alldngnpng, floor, out drawnTile) ||
                tile.TryDrawCloud(_cloudtiles, _alleffects, floor, model.SideData, model.MonsterData, out drawnTile) ||
                tile.TryDrawItem(tileHighlight, _itemdata, _itempng, _miscallaneous, floor, model.Location, out drawnTile)) 
            {
                g.DrawImage(drawnTile, x, y);
                return true; 
            }

            if (!dict.ContainsKey(tile))
            {
                dict.Add(tile, "");
            }

            g.WriteCharacter(tile, new Font("Courier New", 16), x, y);//unhandled tile, write it as a character instead

            return false;
        }
    }
}



