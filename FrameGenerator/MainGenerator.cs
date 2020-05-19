using FrameGenerator.Extensions;
using FrameGenerator.FileReading;
using FrameGenerator.Models;
using FrameGenerator.OutOfSightCache;
using InputParser;
using Putty;
using System;
using System.Collections.Generic;
using SkiaSharp;
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
        private readonly Dictionary<string, string[]> _floorandwallColor;
        private readonly Dictionary<string, string> _features;
        private readonly Dictionary<string, string> _cloudtiles;
        private readonly Dictionary<string, string> _itemdata;
        private readonly Dictionary<string, string> _weapondata;
        private readonly Dictionary<string, SKBitmap> _monsterpng;
        private readonly Cacher _outOfSightCache;
        private readonly Dictionary<string, SKBitmap> _characterpng;
        private readonly Dictionary<string, SKBitmap> _weaponpng;
        private readonly Dictionary<string, SKBitmap> _itempng;
        private readonly Dictionary<string, SKBitmap> _alldngnpng;
        private readonly Dictionary<string, SKBitmap> _alleffects;
        private readonly Dictionary<string, SKBitmap> _miscallaneous;
        private readonly Dictionary<string, SKBitmap> _floorpng;
        private readonly Dictionary<string, SKBitmap> _wallpng;
        private SKBitmap _lastframe = new SKBitmap(1602, 1050);
        private int previousHP = 0;
        private int previousMP = 0;
        private int _lostHpCheckpoint = 0;
        private int _lostMpCheckpoint = 0;
        public static SKBitmap CharacterSKBitmap = new SKBitmap(32, 32);
        

        public MainGenerator(string gameLocation = @"..\..\..\Extra")
        {
            _characterdata = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/racepng.txt");
            _features = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/features.txt");
            _cloudtiles = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/clouds.txt");
            _itemdata = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/items.txt");
            _weapondata = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/weapons.txt");

            _floorandwall = ReadFromFile.GetFloorAndWallNamesForDungeons(gameLocation + @"/tilefloor.txt");
            _floorandwallColor = ReadFromFile.GetFloorAndWallNamesForDungeons(gameLocation + @"/tilefloorColors.txt");
            _monsterdata = ReadFromFile.GetMonsterData(gameLocation + @"/mon-data.h", gameLocation + @"/monsteroverrides.txt");
            _namedMonsterOverrideData = ReadFromFile.GetNamedMonsterOverrideData(gameLocation + @"/namedmonsteroverrides.txt");

            _floorpng = ReadFromFile.GetSKBitmapDictionaryFromFolder(gameLocation + @"/rltiles/dngn/floor");
            _wallpng = ReadFromFile.GetSKBitmapDictionaryFromFolder(gameLocation + @"/rltiles/dngn/wall");
            _alldngnpng = ReadFromFile.GetSKBitmapDictionaryFromFolder(gameLocation + @"/rltiles/dngn");
            _alleffects = ReadFromFile.GetSKBitmapDictionaryFromFolder(gameLocation + @"/rltiles/effect");
            _miscallaneous = ReadFromFile.GetSKBitmapDictionaryFromFolder(gameLocation + @"/rltiles/misc");
            _itempng = ReadFromFile.GetSKBitmapDictionaryFromFolder(gameLocation + @"/rltiles/item");

            _characterpng = ReadFromFile.GetCharacterPNG(gameLocation);
            _monsterpng = ReadFromFile.GetMonsterPNG(gameLocation);

            _outOfSightCache = new Cacher();
            _weaponpng = ReadFromFile.GetWeaponPNG(gameLocation);
        }

        public SKBitmap GenerateImage(TerminalCharacter[,] chars, int consoleLevel = 1, Dictionary<string, string> tileoverides = null)
        {
            //return DrawFrame(new Model());
            if (chars != null)
            {

                var model = consoleLevel != 3 ? Parser.ParseData(chars) : Parser.ParseData(chars, true);
                model.OverideTiles(tileoverides);
                if (model.Layout == LayoutType.Normal && consoleLevel == 2)
                {
                    model.Layout = LayoutType.ConsoleSwitch;
                }
                
                var image = DrawFrame(model);
                
#if true //Memory Limit Mode
                GC.Collect();
#endif
                var base64String = Convert.ToBase64String(image.Encode(SKEncodedImageFormat.Png, 80).ToArray());
                return image;
                
            }
                return null;
        }

        private SKBitmap DrawFrame(Model model)
        {
            SKBitmap currentFrame = new SKBitmap(1602, 1050);
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
            SKBitmap forupdate = currentFrame.Copy();

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
                    if (monsterLine.MonsterTextRaw.Contains("Klown"))
                    {
                            finalOverrides.Add("pBLACK","killer_klown_yellow");
                            finalOverrides.Add("pRED","killer_klown_red");
                            finalOverrides.Add("pGREEN","killer_klown_green");
                            finalOverrides.Add("pBROWN","killer_klown_yellow");
                            finalOverrides.Add("pBLUE","killer_klown_blue");
                            finalOverrides.Add("pMAGENTA","killer_klown_purple");
                            finalOverrides.Add("pCYAN","killer_klown_blue");
                            finalOverrides.Add("pLIGHTGREY","killer_klown_green");
                            finalOverrides.Add("pDARKGREY","killer_klown_yellow");
                            finalOverrides.Add("pLIGHTRED","killer_klown_red");
                            finalOverrides.Add("pLIGHTGREEN","killer_klown_green");
                            finalOverrides.Add("pYELLOW","killer_klown_yellow");
                            finalOverrides.Add("pLIGHTBLUE","killer_klown_blue");
                            finalOverrides.Add("pLIGHTMAGENTA","killer_klown_purple");
                            finalOverrides.Add("pLIGHTCYAN","killer_klown_blue");
                            finalOverrides.Add("pWHITE","killer_klown_purple");
                    }
                    if (monsterLine.MonsterTextRaw.Contains("chaos"))
                    {
                        foreach (var monstertileName in monsterLine.MonsterDisplay)
                        {
                            var pngName = "";
                            switch (monstertileName.Substring(1))
                            {
                                case "BLACK": pngName = "chaos_spawn4"; break;
                                case "RED": pngName = "chaos_spawn1"; break;
                                case "GREEN": pngName = "chaos_spawn5"; break;
                                case "BROWN": pngName = "chaos_spawn4"; break;
                                case "BLUE": pngName = "chaos_spawn3"; break;
                                case "MAGENTA": pngName = "chaos_spawn2"; break;
                                case "CYAN": pngName = "chaos_spawn3"; break;
                                case "LIGHTGREY": pngName = "chaos_spawn5"; break;
                                case "DARKGREY": pngName = "chaos_spawn4"; break;
                                case "LIGHTRED": pngName = "chaos_spawn1"; break;
                                case "LIGHTGREEN": pngName = "chaos_spawn5"; break;
                                case "YELLOW": pngName = "chaos_spawn4"; break;
                                case "LIGHTBLUE": pngName = "chaos_spawn3"; break;
                                case "LIGHTMAGENTA": pngName = "chaos_spawn2"; break;
                                case "LIGHTCYAN": pngName = "chaos_spawn3"; break;
                                case "WHITE": pngName = "chaos_spawn2"; break;
                                default: pngName = "chaos_spawn4"; break;
                            }
                            finalOverrides.Add(monstertileName, pngName);
                        }
                    }
                    if (monsterLine.MonsterTextRaw.Contains("spatial"))
                    {
                        foreach (var monstertileName in monsterLine.MonsterDisplay)//can be maelstron on screen and vortex not in list of monster, but ehhh
                        {
                            var pngName = "";
                            switch (monstertileName.Substring(1))
                            {
                                case "BLACK": pngName = "spatial_vortex1"; break;
                                case "RED": pngName = "spatial_vortex1"; break;
                                case "GREEN": pngName = "spatial_vortex1"; break;
                                case "BROWN": pngName = "spatial_vortex1"; break;
                                case "BLUE": pngName = "spatial_vortex2"; break;
                                case "MAGENTA": pngName = "spatial_vortex2"; break;
                                case "CYAN": pngName = "spatial_vortex2"; break;
                                case "LIGHTGREY": pngName = "spatial_vortex2"; break;
                                case "DARKGREY": pngName = "spatial_vortex3"; break;
                                case "LIGHTRED": pngName = "spatial_vortex3"; break;
                                case "LIGHTGREEN": pngName = "spatial_vortex3"; break;
                                case "YELLOW": pngName = "spatial_vortex3"; break;
                                case "LIGHTBLUE": pngName = "spatial_vortex3"; break;
                                case "LIGHTMAGENTA": pngName = "spatial_vortex4"; break;
                                case "LIGHTCYAN": pngName = "spatial_vortex4"; break;
                                case "WHITE": pngName = "spatial_vortex4"; break;
                                default: pngName = "spatial_vortex3"; break;
                            }
                            finalOverrides.Add(monstertileName, pngName);
                        }
                    }
                    if (monsterLine.MonsterTextRaw.Contains("Tiamat"))
                    {
                        foreach (var monstertileName in monsterLine.MonsterDisplay)//well these colours are wrong
                        {
                            var pngName = "";
                            switch (monstertileName.Substring(1))
                            {
                                case "BLACK": pngName = "tiamat_black"; break;
                                case "RED": pngName = "tiamat_red"; break;
                                case "GREEN": pngName = "tiamat_green"; break;
                                case "BROWN": pngName = "tiamat_black"; break;
                                case "BLUE": pngName = "tiamat_mottled"; break;
                                case "MAGENTA": pngName = "tiamat_purple"; break;
                                case "CYAN": pngName = "tiamat_pale"; break;
                                case "LIGHTGREY": pngName = "tiamat_grey"; break;
                                case "DARKGREY": pngName = "tiamat_pale"; break;
                                case "LIGHTRED": pngName = "tiamat_red"; break;
                                case "LIGHTGREEN": pngName = "tiamat_green"; break;
                                case "YELLOW": pngName = "tiamat_yellow"; break;
                                case "LIGHTBLUE": pngName = "tiamat_black"; break;
                                case "LIGHTMAGENTA": pngName = "tiamat_mottled"; break;
                                case "LIGHTCYAN": pngName = "tiamat_pale"; break;
                                case "WHITE": pngName = "tiamat_white"; break;
                                default: pngName = "tiamat_yellow"; break;
                            }
                            finalOverrides.Add(monstertileName, pngName);
                        }
                    }
                    if (monsterLine.MonsterTextRaw.Contains("Yiuf"))
                    {
                        foreach (var monstertileName in monsterLine.MonsterDisplay)
                        {
                            finalOverrides.Add(monstertileName, "crazy_yiuf");
                        }
                    }
                    if (monsterLine.MonsterTextRaw.Contains("slime"))
                    {
                        if (monsterLine.MonsterTextRaw.Contains("large"))
                        {
                            if (monsterLine.MonsterTextRaw.Contains("very"))
                            {
                                foreach (var monstertileName in monsterLine.MonsterDisplay)
                                {
                                    finalOverrides.Add(monstertileName, "slime_creature3");
                                }
                            }
                            else
                            {
                                foreach (var monstertileName in monsterLine.MonsterDisplay)
                                {
                                    finalOverrides.Add(monstertileName, "slime_creature2");
                                }
                            }
                        }
                        if (monsterLine.MonsterTextRaw.Contains("enorm"))
                        {
                            foreach (var monstertileName in monsterLine.MonsterDisplay)
                            {
                                finalOverrides.Add(monstertileName, "slime_creature4");
                            }
                        }
                        if (monsterLine.MonsterTextRaw.Contains("titan"))
                        {
                            foreach (var monstertileName in monsterLine.MonsterDisplay)
                            {
                                finalOverrides.Add(monstertileName, "slime_creature5");
                            }
                        }

                    }
                    if (monsterLine.MonsterTextRaw.Contains("ugly") && monsterLine.MonsterDisplay[0][0] == 'u')
                    {
                        if (monsterLine.MonsterTextRaw.Contains("very"))
                        {
                            foreach (var monstertileName in monsterLine.MonsterDisplay)//can be maelstron on screen and vortex not in list of monster, but ehhh
                            {
                                var pngName = "";
                                switch (monstertileName.Substring(1))
                                {
                                    case "BLACK": pngName = "very_ugly_thing"; break;
                                    case "RED": pngName = "very_ugly_thing"; break;
                                    case "GREEN": pngName = "very_ugly_thing2"; break;
                                    case "BROWN": pngName = "very_ugly_thing1"; break;
                                    case "BLUE": pngName = "very_ugly_thing3"; break;
                                    case "MAGENTA": pngName = "very_ugly_thing4"; break;
                                    case "CYAN": pngName = "very_ugly_thing3"; break;
                                    case "LIGHTGREY": pngName = "very_ugly_thing5"; break;
                                    case "DARKGREY": pngName = "very_ugly_thing1"; break;
                                    case "LIGHTRED": pngName = "very_ugly_thing"; break;
                                    case "LIGHTGREEN": pngName = "very_ugly_thing2"; break;
                                    case "YELLOW": pngName = "very_ugly_thing1"; break;
                                    case "LIGHTBLUE": pngName = "very_ugly_thing3"; break;
                                    case "LIGHTMAGENTA": pngName = "very_ugly_thing4"; break;
                                    case "LIGHTCYAN": pngName = "very_ugly_thing3"; break;
                                    case "WHITE": pngName = "very_ugly_thing5"; break;
                                    default: pngName = "very_ugly_thing5"; break;
                                }
                                finalOverrides.Add(monstertileName, pngName);
                            }
                        }
                        else
                        {
                            foreach (var monstertileName in monsterLine.MonsterDisplay)//can be maelstron on screen and vortex not in list of monster, but ehhh
                            {
                                var pngName = "";
                                switch (monstertileName.Substring(1))
                                {
                                    case "BLACK": pngName = "ugly_thing"; break;
                                    case "RED": pngName = "ugly_thing"; break;
                                    case "GREEN": pngName = "ugly_thing2"; break;
                                    case "BROWN": pngName = "ugly_thing1"; break;
                                    case "BLUE": pngName = "ugly_thing3"; break;
                                    case "MAGENTA": pngName = "ugly_thing4"; break;
                                    case "CYAN": pngName = "ugly_thing3"; break;
                                    case "LIGHTGREY": pngName = "ugly_thing5"; break;
                                    case "DARKGREY": pngName = "ugly_thing1"; break;
                                    case "LIGHTRED": pngName = "ugly_thing"; break;
                                    case "LIGHTGREEN": pngName = "ugly_thing2"; break;
                                    case "YELLOW": pngName = "ugly_thing1"; break;
                                    case "LIGHTBLUE": pngName = "ugly_thing3"; break;
                                    case "LIGHTMAGENTA": pngName = "ugly_thing4"; break;
                                    case "LIGHTCYAN": pngName = "ugly_thing3"; break;
                                    case "WHITE": pngName = "ugly_thing5"; break;
                                    default: pngName = "ugly_thing5"; break;
                                }
                                finalOverrides.Add(monstertileName, pngName);
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

        private SKBitmap DrawMap(Model model)
        {
            SKBitmap bmp = new SKBitmap(1602, 768);
            
            using (SKCanvas g = new SKCanvas(bmp))
            {
                var font = new SKPaint
                {
                    Typeface = SKTypeface.FromFamilyName("Courier New"),
                    TextSize = 22
                };
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

        private SKBitmap DrawTextBox(Model model, SKBitmap lastframe)
        {
            SKBitmap overlayImage = lastframe.Copy();

            using (SKCanvas g = new SKCanvas(overlayImage))
            {
                var font = new SKPaint {
                    Typeface = SKTypeface.FromFamilyName("Courier New"),
                    TextSize = 12,
                    IsAntialias = true,
                };
                var darkPen = new SKPaint() { 
                Color = SKColors.Black,
                StrokeWidth = 2,
                Style = SKPaintStyle.StrokeAndFill
                };
                var rect2 = new SKRect(25, 25, 25+ 1000, 25 + 430);
                g.DrawRect(rect2, darkPen);
                darkPen = new SKPaint()
                {
                    Color = new SKColor(255, 125, 98, 60),
                    StrokeWidth = 2,
                    Style = SKPaintStyle.Stroke
                };
                g.DrawRect(rect2, darkPen);

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

        private SKBitmap DrawNormal(Model model)
        {
            SKBitmap newFrame = new SKBitmap(1602, 768);
            using (SKCanvas g = new SKCanvas(newFrame))
            {
                g.Clear(SKColors.Black);

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

        private SKBitmap DrawConsoleSwitch(Model model)
        {
            SKBitmap newFrame = new SKBitmap(1602, 768);
            using (SKCanvas g = new SKCanvas(newFrame))
            {
                g.Clear(SKColors.Black);

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

        private SKBitmap DrawConsoleFull(Model model)
        {
            SKBitmap newFrame = new SKBitmap(1602, 768);
            using (SKCanvas g = new SKCanvas(newFrame))
            {
                g.Clear(SKColors.Black);

                DrawConsole(g, model, 0, 0, 1.27f, 16, 2f, 20);

            }

            return newFrame;
        }

        private void DrawConsole(SKCanvas g, Model model, float startX, float startY, float xResize = 1, float yWidth = 16, float yResize = 1, float fontSize = 16)
        {
            var currentX = startX;
            var currentY = startY - yWidth * yResize;
            var font = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Courier New"),
                TextSize = fontSize
            };

            for (var i = 0; i < model.TileNames.Length; i++)
            {
                if (i % model.LineLength == 0)
                {
                    currentX = startX;
                    currentY += yWidth * yResize;
                }
                else currentX += yWidth * xResize;

                g.WriteCharacter(model.TileNames[i], font, currentX, currentY, model.HighlightColors[i], 5);


            }
        }

        private void DrawMonsterDisplay(SKCanvas g, Model model, Dictionary<string, string> overrides)
        {
            var sideOfTilesX = 32 * model.LineLength; var currentLineY = 300;
            var font = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Courier New"),
                TextSize = 16
            };
            foreach (var monsterlist in model.MonsterData)
            {
                var x = sideOfTilesX;
                if (!monsterlist.Empty)
                {
                    for (int i = 0; i < monsterlist.MonsterDisplay.Length; i++)
                    {
                        if (monsterlist.MonsterDisplay[i].TryDrawMonster(monsterlist.MonsterBackground[i], _monsterdata, _monsterpng, overrides, out SKBitmap tileToDraw))
                        {
                            g.DrawBitmap(tileToDraw, x, currentLineY);
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
                        //g.PaintBackground(backgroundColor, font, otherx, currentLineY + 4);
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

        private static void DrawLogs(SKCanvas g, Model model)
        {
            int y = 544;
            var font = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Courier New"),
                TextSize = 16
            };
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

        public static void DrawSideDATA(SKCanvas g, Model model, int prevHP, int prevMP)
        {
            var font = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Courier New"),
                TextSize = 16,
                IsAntialias = true,
            };
            var yellow = new SKColor(252, 233, 79);
            var gray = new SKColor(186, 189, 182);

            var lineCount = 0;
            var lineHeight = 20;

            font.Color = yellow;

            g.DrawText(model.SideData.Name, 32 * model.LineLength, lineCount * lineHeight + font.TextSize, font);
            lineCount++;
            g.DrawText(model.SideData.Race, 32 * model.LineLength, lineCount * lineHeight + font.TextSize, font);
            lineCount++;
            g.WriteSideDataInfo("Health: ", model.SideData.Health.ToString() + '/' + model.SideData.MaxHealth.ToString(), font, 32 * model.LineLength, lineCount * lineHeight)
            .DrawPercentageBar(model.SideData.Health, model.SideData.MaxHealth, prevHP, SKColors.Green, SKColors.Red, 32 * (model.LineLength + 8), lineCount * lineHeight);
            lineCount++;
            g.WriteSideDataInfo("Mana: ", model.SideData.Magic.ToString() + '/' + model.SideData.MaxMagic.ToString(), font, 32 * model.LineLength, lineCount * lineHeight)
            .DrawPercentageBar(model.SideData.Magic, model.SideData.MaxMagic, prevMP, SKColors.Blue, SKColors.BlueViolet, 32 * (model.LineLength + 8), lineCount * lineHeight);
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
            .WriteSideDataInfo(" Next: ", model.SideData.ExperienceLevel, font, 32 * model.LineLength + font.MeasureText("XL: " + model.SideData.ExperienceLevel), lineCount * lineHeight)
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
            font.Color = gray;
            if (!string.IsNullOrWhiteSpace(substring))
            {
                g.DrawText(substring, 32 * model.LineLength + font.MeasureText("Wp: "), lineCount * lineHeight + font.TextSize, font);
                lineCount++;
            }

            g.WriteSideDataInfo("Qv: ", model.SideData.Quiver.Substring(0, 35), font, 32 * model.LineLength, lineCount * lineHeight);
            lineCount++;
            substring = model.SideData.Quiver.Substring(35);
            if (!string.IsNullOrWhiteSpace(substring))
            {
                g.DrawText(substring, 32 * model.LineLength + font.MeasureText("Qv: "), lineCount * lineHeight + font.TextSize, font);
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

        public bool DrawPlayer(SKCanvas g, Model model, float x, float y, float resize = 1)
        {

            string[] BasicStatusArray = { "bat", "dragon", "ice", "mushroom", "pig", "shadow", "spider" };
            string[] CompStatusArray = { "lich", "statue" };

            string characterRace = model.SideData.Race.Substring(0, 6);
            

            var CharacterSKBitmap = new SKBitmap(32, 32);
            if (!_characterdata.TryGetValue(characterRace, out var pngName)) return false;
            if (!_characterpng.TryGetValue(pngName, out SKBitmap png)) return false;

            string[] location = model.SideData.Place.Split(':'); //TODO add floor is lava and water based on status
            if (!_floorandwall.TryGetValue(location[0].ToUpper(), out var CurrentLocationFloorAndWallName)) return false;

            if (!_floorpng.TryGetValue(CurrentLocationFloorAndWallName[1], out var floor)) return false;

            
            
            using (SKCanvas characterg = new SKCanvas(CharacterSKBitmap))
            {
                var rect = new SKRect(0, 0, floor.Width, floor.Height);

                if(model.SideData.Statuses1.ToLower().Contains("water"))
                {
                    _outOfSightCache.TryGetLastSeenBitmapByChar('≈', out var lastSeen);
                    if(lastSeen!=null) floor = lastSeen;
                    else if(!_alldngnpng.TryGetValue("shallow_water", out floor)) return false;
                }
                else if(model.SideData.Statuses1.ToLower().Contains("lava"))
                {
                    if (!_alldngnpng.TryGetValue("lava08", out floor)) return false;
                }
                else if(model.SideData.Statuses1.ToLower().Contains("net"))
                {
                    if (!_alldngnpng.TryGetValue("net_trap", out floor)) return false;
                }

                characterg.DrawBitmap(floor, rect);

                foreach (string status in BasicStatusArray)
                {
                    if (model.SideData.Statuses1.ToLowerInvariant().Contains(status) && _weaponpng.TryGetValue(status + "_form", out png)) break;
                }

                foreach (string status in CompStatusArray)
                {
                    if (model.SideData.Statuses1.ToLowerInvariant().Contains(status))
                    {
                        if (!_weaponpng.TryGetValue(status + "_form_" + characterRace.ToLower(), out png)) _weaponpng.TryGetValue(status + "_form_humanoid", out png);
                    }
                }

                characterg.DrawBitmap(png, rect);

                if (_weaponpng.TryGetValue(model.SideData.Weapon.ParseUniqueWeaponName(), out png)) characterg.DrawBitmap(png, rect);

                else if (_weaponpng.TryGetValue(model.SideData.Weapon.GetNonUniqueWeaponName(_weapondata), out png)) characterg.DrawBitmap(png, rect);

                var rect2 = new SKRect(x, y, x + (CharacterSKBitmap.Width * resize), y + (CharacterSKBitmap.Height * resize));
                g.DrawBitmap(CharacterSKBitmap, rect2);

                return true;
            }
        }

        public void DrawTiles(SKCanvas g, Model model, float startX, float startY, int startIndex, Dictionary<string, string> overrides, float resize = 1)
        {
            var dict = new Dictionary<string, string>();//logging
            var SKBitmapList = new List<Tuple<string, SKBitmap>>(model.TileNames.Length);

            string characterRace = model.SideData.Race.Substring(0, 6);
            string[] location = model.SideData.Place.Split(':');

            _outOfSightCache.DumpDataOnLocationChange(model.SideData.Place);

            if (!_floorandwall.TryGetValue(location[0].ToUpper(), out var CurrentLocationFloorAndWallName)) return;

            if (!_floorandwallColor.TryGetValue(location[0].ToUpper(), out var CurrentLocationFloorAndWallColor)) return;

            if (!_wallpng.TryGetValue(CurrentLocationFloorAndWallName[0], out var wall)) return;
            if (!_floorpng.TryGetValue(CurrentLocationFloorAndWallName[1], out var floor)) return;

            //TODO special handling for pan abyss zot and elf

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

                var tileDrawn = DrawCurrentTile(g, model, dict, model.TileNames[i], model.HighlightColors[i], wall, floor, CurrentLocationFloorAndWallColor, overrides, currentTileX, currentTileY, resize, out SKBitmap drawnTile);

                if (tileDrawn && !model.TileNames[i].IsWallOrFloor())
                {
                    SKBitmapList.Add(new Tuple<string, SKBitmap>(model.TileNames[i], drawnTile));
                }
                
            }
            _outOfSightCache.UpdateCache(SKBitmapList);

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

        private bool DrawCurrentTile(SKCanvas g, Model model, Dictionary<string, string> dict, string tile, string tileHighlight, SKBitmap wall, SKBitmap floor, string[] wallAndFloorColors, Dictionary<string, string> overrides, float x, float y, float resize, out SKBitmap drawnTile)
        {
            if (tile[0] == ' ' && (tileHighlight == Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTGREY) || tileHighlight == Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)) || tile.StartsWith("@BL")) { 
                drawnTile = null; 
                return false;
            }

            SKBitmap brandToDraw = null;
            bool cached = false;
            if (tile.TryDrawWallOrFloor(tileHighlight, wall, floor, wallAndFloorColors, out drawnTile) ||
                tile.TryDrawMonster(tileHighlight, overrides, _monsterpng, _miscallaneous, floor, out drawnTile, out brandToDraw) ||//first try drawing overrides, that include blue color monsters, and monsters in sight
                tile.TryDrawCachedTile(tileHighlight, _outOfSightCache, new List<char> {'!', '?', '=', '"', '$', ')', '[', '_', '}', '/', '(', ':', '|', '%', '÷', '†'}, new List<string> { "≈RED"}, out drawnTile, out cached) ||
                tile.TryDrawMonster(tileHighlight, _monsterdata, _monsterpng, _miscallaneous, floor, out drawnTile, out brandToDraw) ||//draw the rest of the monsters
                tile.TryDrawFeature(tileHighlight, _features, _alldngnpng, _miscallaneous, floor, wall, out drawnTile) ||
                tile.TryDrawCloud(_cloudtiles, _alleffects, floor, model.SideData, model.MonsterData, out drawnTile) ||
                tile.TryDrawItem(tileHighlight, _itemdata, _itempng, _miscallaneous, floor, model.Location, out drawnTile)) 
            {
                var rect = new SKRect(x, y, x+ ( drawnTile.Width * resize),y+ ( drawnTile.Height * resize));
                g.DrawBitmap(drawnTile, rect);

                if (brandToDraw != null)
                {
                    g.DrawBitmap(brandToDraw, new SKRect(x, y, x + (brandToDraw.Width * resize), y + (brandToDraw.Height * resize)));
                }
                else if (!tileHighlight.Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)) && (!tile.Substring(1).Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)) || tile[0] == '.'))
                {
                    var backgroundPaint = new SKPaint()
                    {
                        Color = ColorList.GetColor(tileHighlight).WithAlpha(100),
                        Style = SKPaintStyle.StrokeAndFill
                    };

                    g.DrawRect(rect, backgroundPaint);
                }
                if (cached)//darken to match out of sight
                {
                    var backgroundPaint = new SKPaint()
                    {
                        Color = new SKColor(0, 0, 0, 150),
                        Style = SKPaintStyle.StrokeAndFill
                    };

                    g.DrawRect(rect, backgroundPaint);
                }

                return true; 
            }

            if (!dict.ContainsKey(tile))
            {
                dict.Add(tile, "");
            }
            var font = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Courier New"),
                TextSize = 24 * resize,
                
            };
            g.WriteCharacter(tile, font, x, y, tileHighlight);//unhandled tile, write it as a character instead

            return false;
        }
    }
}
