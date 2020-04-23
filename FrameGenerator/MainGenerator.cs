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
        private const int BottomRightStartX = 1065;
        private const int BottomRightStartY = 468;
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
        private int previousMP = 0;
        private int _lostHpCheckpoint = 0;
        private int _lostMpCheckpoint = 0;
        public static Bitmap CharacterBitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        

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

        public Bitmap GenerateImage(TerminalCharacter[,] chars, int consoleLevel = 1)
        {
            if (chars != null)
            {

                var model = consoleLevel != 3 ? Parser.ParseData(chars) : Parser.ParseData(chars, true);

                if (model.Layout == LayoutType.Normal && consoleLevel == 2)
                {
                    model.Layout = LayoutType.ConsoleSwitch;
                }
                

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
                    currentFrame = DrawNormal(model);
                    _lastframe = currentFrame;
                    _lostHpCheckpoint = model.SideData.Health > previousHP ? 0 : model.SideData.Health == previousHP ? _lostHpCheckpoint : previousHP;
                    _lostMpCheckpoint = model.SideData.Magic > previousMP ? 0 : model.SideData.Magic == previousMP ? _lostMpCheckpoint : previousMP;
                    previousHP = model.SideData.Health;
                    previousMP = model.SideData.Magic;
                    break;
                case LayoutType.ConsoleSwitch:
                    currentFrame = DrawConsoleSwitch(model);
                    _lastframe = currentFrame;
                    _lostHpCheckpoint = model.SideData.Health > previousHP ? 0 : model.SideData.Health;
                    _lostMpCheckpoint = model.SideData.Magic > previousMP ? 0 : model.SideData.Magic;
                    previousHP = model.SideData.Health;
                    previousMP = model.SideData.Magic;
                    break;
                case LayoutType.TextOnly:
                    currentFrame = DrawTextBox(model, _lastframe);
                    break;
                case LayoutType.MapOnly:
                    currentFrame = DrawMap(model);
                    break;
                case LayoutType.ConsoleFull:
                    currentFrame = DrawConsoleFull(model);
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
                DrawTiles(g, model, 0, 32, model.LineLength, overrides);//draw the rest of the map
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

        private Bitmap DrawNormal(Model model)
        {
            Bitmap newFrame = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newFrame))
            {
                g.Clear(Color.Black);

                var overrides = GetOverridesForFrame(model.MonsterData, model.Location);

                DrawSideDATA(g, model, _lostHpCheckpoint, _lostMpCheckpoint);

                DrawTiles(g, model, 0, 0, 0, overrides);

                DrawPlayer(g, model, 0 + (32 * 16), 0 + (32 * 8));

                DrawMonsterDisplay(g, model, overrides);

                DrawLogs(g, model);

                DrawConsole(g, model, BottomRightStartX, BottomRightStartY);

            }

            return newFrame;
        }

        private Bitmap DrawConsoleSwitch(Model model)
        {
            Bitmap newFrame = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newFrame))
            {
                g.Clear(Color.Black);

                var overrides = GetOverridesForFrame(model.MonsterData, model.Location);

                DrawSideDATA(g, model, _lostHpCheckpoint, _lostMpCheckpoint);

                DrawTiles(g, model, BottomRightStartX, BottomRightStartY, 0, overrides, 0.5f);

                DrawPlayer(g, model, BottomRightStartX + (32 * 16) * 0.5f, BottomRightStartY + (32 * 8) * 0.5f, 0.5f);

                DrawMonsterDisplay(g, model, overrides);

                DrawLogs(g, model);

                DrawConsole(g, model, 0, 0, 2f, 16, 2f, 24);

            }

            return newFrame;
        }

        private Bitmap DrawConsoleFull(Model model)
        {
            Bitmap newFrame = new Bitmap(1602, 768, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newFrame))
            {
                g.Clear(Color.Black);

                DrawConsole(g, model, 0, 0, 1.27f, 16, 2f, 20);

            }

            return newFrame;
        }

        private void DrawConsole(Graphics g, Model model, float startX, float startY, float xResize = 1, float yWidth = 16, float yResize = 1, float fontSize = 16)
        {
            var currentX = startX;
            var currentY = startY - yWidth * yResize;
            using var font2 = new Font("Courier New", fontSize);

            for (var i = 0; i < model.TileNames.Length; i++)
            {
                if (i % model.LineLength == 0)
                {
                    currentX = startX;
                    currentY += yWidth * yResize;
                }
                else currentX += yWidth * xResize;

                g.WriteCharacter(model.TileNames[i], font2, currentX, currentY, model.HighlightColors[i], 0);


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

        public static void DrawSideDATA(Graphics g, Model model, int prevHP, int prevMP)
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
            .DrawPercentageBar(model.SideData.Health, model.SideData.MaxHealth, prevHP, Color.Green, Color.Red, 32 * (model.LineLength + 8), lineCount * lineHeight);
            lineCount++;
            g.WriteSideDataInfo("Mana: ", model.SideData.Magic.ToString() + '/' + model.SideData.MaxMagic.ToString(), font, 32 * model.LineLength, lineCount * lineHeight)
            .DrawPercentageBar(model.SideData.Magic, model.SideData.MaxMagic, prevMP, Color.Blue, Color.BlueViolet, 32 * (model.LineLength + 8), lineCount * lineHeight);
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
            (int.TryParse(model.SideData.NoisyGold, out _) ? 
                g.WriteSideDataInfo("Gold:", model.SideData.NoisyGold, font, 32 * model.LineLength, lineCount * lineHeight) :
                g.WriteSideDataInfo("Noise:", model.SideData.NoisyGold, font, 32 * model.LineLength, lineCount * lineHeight))
            .WriteSideDataInfo("Time: ", model.SideData.Time, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
            lineCount++;

            g.WriteSideDataInfo("Wp: ", model.SideData.Weapon.Substring(0, 35), font, 32 * model.LineLength, lineCount * lineHeight);
            lineCount++;
            var substring = model.SideData.Weapon.Substring(35);
            if (!string.IsNullOrWhiteSpace(substring))
            {
                g.DrawString(substring, font, gray, 32 * model.LineLength + g.MeasureString("Wp: ", font).Width, lineCount * lineHeight);
                lineCount++;
            }

            g.WriteSideDataInfo("Qv: ", model.SideData.Quiver.Substring(0, 35), font, 32 * model.LineLength, lineCount * lineHeight);
            lineCount++;
            substring = model.SideData.Quiver.Substring(35);
            if (!string.IsNullOrWhiteSpace(substring))
            {
                g.DrawString(substring, font, gray, 32 * model.LineLength + g.MeasureString("Qv: ", font).Width, lineCount * lineHeight);
                lineCount++;
            }

            var x = 32 * model.LineLength;
            foreach (var coloredChar in model.SideDataColored.Statuses1)
            {
                g.WriteCharacter(coloredChar, font, x, lineCount * lineHeight);
                x += 6;
            }
            lineCount++;
            x = 32 * model.LineLength;
            foreach (var coloredChar in model.SideDataColored.Statuses2)
            {
                g.WriteCharacter(coloredChar, font, 32 * model.LineLength, lineCount * lineHeight);
                x += 6;
            }

        }

        public bool DrawPlayer(Graphics g, Model model, float x, float y, float resize = 1)
        {

            string[] BasicStatusArray = { "bat", "dragon", "ice", "mushroom", "pig", "shadow", "spider" };
            string[] CompStatusArray = { "lich", "statue" };

            string characterRace = model.SideData.Race.Substring(0, 6);
            

            var CharacterBitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
            if (!_characterdata.TryGetValue(characterRace, out var pngName)) return false;
            if (!_characterpng.TryGetValue(pngName, out Bitmap png)) return false;

            string[] location = model.SideData.Place.Split(':'); //TODO add floor is lava and water based on status
            if (!_floorandwall.TryGetValue(location[0].ToUpper(), out var CurrentLocationFloorAndWallName)) return false;

            if (!_wallpng.TryGetValue(CurrentLocationFloorAndWallName[0], out var wall)) return false;
            if (!_floorpng.TryGetValue(CurrentLocationFloorAndWallName[1], out var floor)) return false;

            using (Graphics characterg = Graphics.FromImage(CharacterBitmap))
            {
                characterg.DrawImage(floor, 0, 0, floor.Width, floor.Height);

                foreach (string status in BasicStatusArray)
                {
                    if (model.SideData.Statuses1.Contains(status) && _weaponpng.TryGetValue(status + "_form", out png)) break;
                }

                foreach (string status in CompStatusArray)
                {
                    if (model.SideData.Statuses1.Contains(status))
                    {
                        if (!_weaponpng.TryGetValue(status + "_form_" + characterRace.ToLower(), out png)) _weaponpng.TryGetValue(status + "_form_humanoid", out png);
                    }
                }

                characterg.DrawImage(png, 0, 0, png.Width, png.Height);

                if (_weaponpng.TryGetValue(model.SideData.Weapon.ParseUniqueWeaponName(), out png)) characterg.DrawImage(png, 0, 0, png.Width, png.Height);

                else if (_weaponpng.TryGetValue(model.SideData.Weapon.GetNonUniqueWeaponName(_weapondata), out png)) characterg.DrawImage(png, 0, 0, png.Width, png.Height);

                g.DrawImage(CharacterBitmap, x, y, CharacterBitmap.Width * resize, CharacterBitmap.Height * resize);

                return true;
            }
        }

        public void DrawTiles(Graphics g, Model model, float startX, float startY, int startIndex, Dictionary<string, string> overrides, float resize = 1)
        {
            var dict = new Dictionary<string, string>();//logging
            var BitmapList = new List<Tuple<string, Bitmap>>(model.TileNames.Length);

            string characterRace = model.SideData.Race.Substring(0, 6);
            string[] location = model.SideData.Place.Split(':');

            _outOfSightCache.DumpDataOnLocationChange(model.SideData.Place);

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
                    currentTileX = startX;
                    currentTileY += 32 * resize;
                }
                else
                    currentTileX += 32 * resize;

                var tileDrawn = DrawCurrentTile(g, model, dict, model.TileNames[i], model.HighlightColors[i], wall, floor, overrides, currentTileX, currentTileY, resize, out Bitmap drawnTile);

                if (tileDrawn && !model.TileNames[i].IsWallOrFloor())
                {
                    BitmapList.Add(new Tuple<string, Bitmap>(model.TileNames[i], drawnTile));
                }
                
            }
            _outOfSightCache.UpdateCache(BitmapList);

#if DEBUG //log unknown characters
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

        private bool DrawCurrentTile(Graphics g, Model model, Dictionary<string, string> dict, string tile, string tileHighlight, Bitmap wall, Bitmap floor, Dictionary<string, string> overrides, float x, float y, float resize, out Bitmap drawnTile)
        {
            if (tile[0] == ' ' && (tileHighlight == Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY) || tileHighlight == Enum.GetName(typeof(ColorList2), ColorList2.BLACK)) || tile.StartsWith("@BL")) { 
                drawnTile = null; 
                return false;
            }

            //if blue its cache time
            if(tile.Substring(1) == Enum.GetName(typeof(ColorList2), ColorList2.BLUE) && _outOfSightCache.TryGetLastSeenBitmapByChar(tile[0], out var lastSeen))
            {
                drawnTile = lastSeen;
                g.DrawImage(lastSeen, x, y, lastSeen.Width * resize, lastSeen.Height * resize);
                var outOfSightTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                g.FillRectangle(outOfSightTint, x, y, lastSeen.Width * resize, lastSeen.Height * resize);
                return true;
            }
            //TODO cache into main flow? wall/floor then prioritymonsters by side display, then cacher, then all other monsters, tehn the rest
            /*  dBLUE:black_draconian
                qBLUE:draconian_monk
                hBLUE:black_bear
                kBLUE:rime_drake
                lBLUE:iguana
                pBLUE:servant_of_whispers
                rBLUE:porcupine
                sBLUE:death_scarab
                DBLUE:steam_dragon
                NBLUE:naga_warrior
                QBLUE:tengu_conjurer
                SBLUE:black_mamba
                WBLUE:phantom
                XBLUE:starcursed_mass
                YBLUE:dire_elephant
                6BLUE:gelid_demonspawn
                2BLUE:lorocyproca
                *BLUE:battlesphere
                eBLUE:duvessa
                gBLUE:ijyb
                KBLUE:pikel
                5BLUE:grinder
                8BLUE:roxanne
                @BLUE:donald
                {BLUE:sensed_trivial
                EBLUE:water_elemental
                bBLUE:butterfly9
           */
            if (tile.TryDrawWallOrFloor(wall, floor, out drawnTile) ||
                tile.TryDrawMonster(tileHighlight, _monsterdata, _monsterpng, overrides, floor, out drawnTile) ||
                tile.TryDrawFeature(_features, _alldngnpng, floor, out drawnTile) ||
                tile.TryDrawCloud(_cloudtiles, _alleffects, floor, model.SideData, model.MonsterData, out drawnTile) ||
                tile.TryDrawItem(tileHighlight, _itemdata, _itempng, _miscallaneous, floor, model.Location, out drawnTile)) 
            {
                g.DrawImage(drawnTile, x, y, drawnTile.Width * resize, drawnTile.Height * resize);
                return true; 
            }

            if (!dict.ContainsKey(tile))
            {
                dict.Add(tile, "");
            }

            g.WriteCharacter(tile, new Font("Courier New", 24 * resize), x, y, tileHighlight);//unhandled tile, write it as a character instead

            return false;
        }
    }
}
