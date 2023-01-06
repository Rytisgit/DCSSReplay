﻿using FrameGenerator.Extensions;
using FrameGenerator.FileReading;
using FrameGenerator.OutOfSightCache;
using InputParser;
using Putty;
using System;
using System.Collections.Generic;
using SkiaSharp;
using System.Linq;
using System.Threading.Tasks;
using InputParser.Constant;

namespace FrameGenerator
{

    public class MainGenerator
    {
        private const int BottomRightStartX = 1065;
        private const int BottomRightStartY = 468;
        public bool isGeneratingFrame = false;
        private Cacher _outOfSightCache;
        private VersionSelectableDataHolder _data;
        private SKBitmap _lastFrame = new SKBitmap(1602, 1050);
        private SKTypeface _typeface = SKTypeface.FromFamilyName("Courier New");
        private int previousHP = 0;
        private int previousMP = 0;
        private int _lostHpCheckpoint = 0;
        private int _lostMpCheckpoint = 0;


        public MainGenerator(IReadFromFile fileReader)
        {
            var ReadFromFile = fileReader;
            _data = new VersionSelectableDataHolder(ReadFromFile);
            _outOfSightCache = new Cacher();
        }

        public MainGenerator(IReadFromFileAsync fileReader)
        {
            _data = new VersionSelectableDataHolder(fileReader);
            _outOfSightCache = new Cacher();
        }

        public async Task InitialiseGenerator()
        {
            await _data.InitialiseData();
            _typeface = SKTypeface.FromStream(await _data.ReadFromFile.GetFontStream("cour.ttf"));
        }

        public async Task ReinitializeGenerator()
        {
            _data.NeedRefreshDictionaries = true;
            await InitialiseGenerator();
        }

        public SKBitmap GenerateImage(TerminalCharacter[,] chars, int consoleLevel = 1, Dictionary<string, string> tileoverides = null, string versionSwitch = "Classic")
        {
            //return DrawFrame(new Model());
            if (chars != null)
            {
                _data.version = versionSwitch;
                var model = consoleLevel != 3 ? Parser.ParseData(chars) : Parser.ParseData(chars, true);
                model.OverideTiles(tileoverides);
                if (model.Layout == LayoutType.Normal && consoleLevel == 2)
                {
                    model.Layout = LayoutType.ConsoleSwitch;
                }
                
                var image = DrawFrame(model);
                
#if false //Memory Limit Mode //maybe its been fixed?
                GC.Collect();
#endif
                //var base64String = Convert.ToBase64String(image.Encode(SKEncodedImageFormat.Png, 80).ToArray());
                return image;
                
            }

            return null;
        }

        private SKBitmap DrawFrame(Model model)
        {
            SKBitmap currentFrame = new SKBitmap(1602, 768);
            switch (model.Layout)
            {
                case LayoutType.Normal:
                    currentFrame = DrawNormal(model);
                    _lastFrame = currentFrame.Copy();
                    _lostHpCheckpoint = model.SideData.Health > previousHP ? 0 : model.SideData.Health == previousHP ? _lostHpCheckpoint : previousHP;
                    _lostMpCheckpoint = model.SideData.Magic > previousMP ? 0 : model.SideData.Magic == previousMP ? _lostMpCheckpoint : previousMP;
                    previousHP = model.SideData.Health;
                    previousMP = model.SideData.Magic;
                    break;
                case LayoutType.ConsoleSwitch:
                    currentFrame = DrawConsoleSwitch(model);
                    _lastFrame = currentFrame.Copy();
                    _lostHpCheckpoint = model.SideData.Health > previousHP ? 0 : model.SideData.Health;
                    _lostMpCheckpoint = model.SideData.Magic > previousMP ? 0 : model.SideData.Magic;
                    previousHP = model.SideData.Health;
                    previousMP = model.SideData.Magic;
                    break;
                case LayoutType.TextOnly:
                    currentFrame = DrawTextBox(model, _lastFrame);
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
            currentFrame.Dispose();
            return forupdate;

        }
        // TODO possibly use to override for old and new version
        private Dictionary<string, string> GetOverridesForFrame(MonsterData[] monsters, string location)
        {
            var finalOverrides = new Dictionary<string, string>();
            foreach (var monsterLine in monsters)
            {
                if (monsterLine.Empty) continue;

                var rules = _data.NamedMonsterOverrideData.Where(
                    monsterOverride
                        => !string.IsNullOrWhiteSpace(monsterOverride.Name) &&
                           monsterLine.MonsterTextRaw.Contains(monsterOverride.Name.Substring(0, monsterOverride.Name.Length - 2)));

                foreach (var tileOverride in rules.Where(
                    rule => string.IsNullOrWhiteSpace(rule.Location) || rule.Location == location).SelectMany(rule => rule.TileNameOverrides))
                {
                    finalOverrides.AddOrIgnore(tileOverride.Key, tileOverride.Value);
                }

                finalOverrides.AddColorDependantOverrides(monsterLine);
            }
           
            foreach (var tileOverride in _data.NamedMonsterOverrideData.Where(
                rule => string.IsNullOrWhiteSpace(rule.Name) && rule.Location == location).SelectMany(rule => rule.TileNameOverrides))
            {
                finalOverrides.AddOrIgnore(tileOverride.Key, tileOverride.Value); //add all overrides for current location that arent for specific monsters
            }

            return finalOverrides;
        }


        private SKBitmap DrawMap(Model model)
        {
            SKBitmap bmp = new SKBitmap(1602, 768);
            
            using (SKCanvas g = new SKCanvas(bmp))
            {
                g.Clear(SKColors.Black);

                var font = new SKPaint
                {
                    Typeface = _typeface,
                    TextSize = 22
                };
                float x = 0;
                float y = 0;
                for (int j = 0; j < model.LineLength; j++)//write out first line as text
                {
                    g.WriteCharacter(model.TileNames[j], font, x, y, model.HighlightColors[j]);
                    x += 20;
                }
                var overrides = GetOverridesForFrame(model.MonsterData, model.Location);
                DrawTiles(g, model, 0, 32, model.LineLength, overrides, 0.666f);//draw the rest of the map
            }

            return bmp;
        }

        private SKBitmap DrawTextBox(Model model, SKBitmap lastframe)
        {
            SKBitmap overlayImage = lastframe.Copy();

            using (SKCanvas g = new SKCanvas(overlayImage))
            {
                using var font = new SKPaint {
                    Typeface = _typeface,
                    TextSize = 16,
                    IsAntialias = true,
                };
                using var darkPen = new SKPaint() { 
                Color = SKColors.Black,
                StrokeWidth = 2,
                Style = SKPaintStyle.StrokeAndFill
                };
                var rect2 = new SKRect(25, 25, 25+ 1000, 25 + 430);
                g.DrawRect(rect2, darkPen);
                using var darkPen2 = new SKPaint()
                {
                    Color = new SKColor(255, 125, 98, 60),
                    StrokeWidth = 2,
                    Style = SKPaintStyle.Stroke
                };
                g.DrawRect(rect2, darkPen2);

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

                DrawSideDATA(g, model, _lostHpCheckpoint, _lostMpCheckpoint, _typeface);

                DrawTiles(g, model, 0, 0, 0, overrides);

                DrawPlayer(g, model, 0 + (32 * 16), 0 + (32 * 8));

                DrawMonsterDisplay(g, model, overrides);

                DrawLogs(g, model, _typeface);

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

                DrawSideDATA(g, model, _lostHpCheckpoint, _lostMpCheckpoint, _typeface);

                DrawTiles(g, model, BottomRightStartX, BottomRightStartY, 0, overrides, 0.5f);

                DrawPlayer(g, model, BottomRightStartX + (32 * 16) * 0.5f, BottomRightStartY + (32 * 8) * 0.5f, 0.5f);

                DrawMonsterDisplay(g, model, overrides);

                DrawLogs(g, model, _typeface);

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
            using var font = new SKPaint
            {
                Typeface = _typeface,
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

                g.WriteCharacter(model.TileNames[i], font, currentX, currentY, model.HighlightColors[i], 3);


            }
        }

        private void DrawMonsterDisplay(SKCanvas g, Model model, Dictionary<string, string> overrides)
        {
            var sideOfTilesX = 32 * model.LineLength; var currentLineY = 300;
            using var font = new SKPaint
            {
                Typeface = _typeface,
                TextSize = 20
            };
            foreach (var monsterlist in model.MonsterData)
            {
                var x = sideOfTilesX;
                if (!monsterlist.Empty)
                {
                    for (int i = 0; i < monsterlist.MonsterDisplay.Length; i++)
                    {
                        if (monsterlist.MonsterDisplay[i].TryDrawMonster(monsterlist.MonsterBackground[i], _data.Monsterdata, _data.Monsterpng, overrides, out SKBitmap tileToDraw))
                        {
                            g.DrawBitmap(tileToDraw, x, currentLineY);
                        }
                        else
                        {
                            g.WriteCharacter(monsterlist.MonsterDisplay[i], font, x, currentLineY, monsterlist.MonsterBackground[i]);//not found write as string
                        }
                        x += 32;
                        tileToDraw.Dispose();
                    }
                    var otherx = x;
                    foreach (var backgroundColor in monsterlist.MonsterBackground.Skip(monsterlist.MonsterDisplay.Length))
                    {
                        g.WriteCharacter(" ", font, otherx, currentLineY + 4, backgroundColor);//paint health as a background color
                        otherx += 12;
                    }

                    foreach (var coloredCharacter in monsterlist.MonsterText)//write all name text in 1 line
                    {
                        g.WriteCharacter(coloredCharacter, font, x, currentLineY + 4);
                        x += 12;
                    }

                    currentLineY += 32;
                }
            }
        }

        private static void DrawLogs(SKCanvas g, Model model, SKTypeface typeface)
        {
            int y = 544;
            using var font = new SKPaint
            {
                Typeface = typeface,
                TextSize = 18
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

        public static void DrawSideDATA(SKCanvas g, Model model, int prevHP, int prevMP, SKTypeface typeface)
        {
            using var font = new SKPaint
            {
                Typeface = typeface,
                TextSize = 20,
                IsAntialias = true,
            };
            var yellow = new SKColor(252, 233, 79);
            var gray = new SKColor(186, 189, 182);

            var lineCount = 0;
            var lineHeight = 20;

            font.Color = yellow;

            g.DrawText(model.SideData.Name, 32 * model.LineLength, lineCount * lineHeight + font.TextSize - 5, font);
            lineCount++;
            g.DrawText(model.SideData.Race, 32 * model.LineLength, lineCount * lineHeight + font.TextSize - 5, font);
            lineCount++;
            g.WriteSideDataInfo("Health: ", model.SideData.Health.ToString() + '/' + model.SideData.MaxHealth.ToString(), font, 32 * model.LineLength, lineCount * lineHeight)
            .DrawPercentageBar(model.SideData.Health, model.SideData.MaxHealth, prevHP, SKColors.Green, SKColors.Red, 32 * (model.LineLength + 8), lineCount * lineHeight + 5);
            lineCount++;
            g.WriteSideDataInfo("Mana: ", model.SideData.Magic.ToString() + '/' + model.SideData.MaxMagic.ToString(), font, 32 * model.LineLength, lineCount * lineHeight)
            .DrawPercentageBar(model.SideData.Magic, model.SideData.MaxMagic, prevMP, SKColors.Blue, SKColors.BlueViolet, 32 * (model.LineLength + 8), lineCount * lineHeight + 5);
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
            .WriteSideDataInfo(" Next: ", model.SideData.NextLevel, font, 32 * model.LineLength + font.MeasureText("XL: " + model.SideData.NextLevel), lineCount * lineHeight)
            .WriteSideDataInfo("Place: ", model.SideData.Place, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
            lineCount++;
            (int.TryParse(model.SideData.NoisyGold, out _) ? 
                g.WriteSideDataInfo("Gold:", model.SideData.NoisyGold, font, 32 * model.LineLength, lineCount * lineHeight) :
                g.WriteSideDataInfo("Noise:", model.SideData.NoisyGold, font, 32 * model.LineLength, lineCount * lineHeight))
            .WriteSideDataInfo("Time: ", model.SideData.Time, font, 32 * (model.LineLength + 8), lineCount * lineHeight);
            lineCount++;

            const int maxWeaponNameLength = 43;
            g.WriteSideDataInfo("Wp: ", model.SideData.Weapon.Substring(0, maxWeaponNameLength), font, 32 * model.LineLength, lineCount * lineHeight);
            lineCount++;

            g.WriteSideDataInfo("Qv: ", model.SideData.Quiver.Substring(0, maxWeaponNameLength), font, 32 * model.LineLength, lineCount * lineHeight);
            lineCount++;

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

            string[] BasicStatusArray = { "bat", "dragon", "ice", "mushroom", "pig", "shadow", "spider", "tree" };
            string[] CompStatusArray = { "lich", "statue" };

            string characterRace = model.SideData.Race.Substring(0, 6);
            

            var CharacterSKBitmap = new SKBitmap(32, 32);
            if (!_data.Characterdata.TryGetValue(characterRace, out var pngName)) return false;
            if (!_data.Characterpng.TryGetValue(pngName, out SKBitmap png)) return false;

            string[] location = model.SideData.Place.Split(':');
            if (!_data.Floorandwall.TryGetValue(location[0].ToUpper(), out var currentLocationFloorAndWallName)) return false;

            if (!_data.Floorpng.TryGetValue(currentLocationFloorAndWallName[1], out var floor)) return false;

            
            
            using (SKCanvas characterg = new SKCanvas(CharacterSKBitmap))
            {
                var rect = new SKRect(0, 0, floor.Width, floor.Height);

                if(model.SideData.Statuses1.ToLower().Contains("water"))
                {
                    _outOfSightCache.TryGetLastSeenBitmapByChar('≈', out var lastSeen);
                    if(lastSeen!=null) floor = lastSeen;
                    else if(!_data.Alldngnpng.TryGetValue("shallow_water", out floor)) return false;
                }
                else if(model.SideData.Statuses1.ToLower().Contains("lava"))
                {
                    if (!_data.Alldngnpng.TryGetValue("lava08", out floor)) return false;
                }

                characterg.DrawBitmap(floor, rect);

                foreach (string status in BasicStatusArray)
                {
                    if (model.SideData.Statuses1.ToLowerInvariant().Contains(status) && _data.Weaponpng.TryGetValue(status + "_form", out png)) break;
                }

                foreach (string status in CompStatusArray)
                {
                    if (model.SideData.Statuses1.ToLowerInvariant().Contains(status))
                    {
                        if (!_data.Weaponpng.TryGetValue(status + "_form_" + characterRace.ToLower(), out png)) _data.Weaponpng.TryGetValue(status + "_form_humanoid", out png);
                    }
                }

                characterg.DrawBitmap(png, rect);

                if (_data.Weaponpng.TryGetValue(model.SideData.Weapon.ParseUniqueWeaponName(), out png)) characterg.DrawBitmap(png, rect);

                else if (_data.Weaponpng.TryGetValue(model.SideData.Weapon.GetNonUniqueWeaponName(_data.Weapondata), out png)) characterg.DrawBitmap(png, rect);

                else if (model.SideData.Statuses1.ToLower().Contains("held"))
                {
                    if (location.Contains("Spider"))
                    {
                        if (_data.Alldngnpng.TryGetValue("cobweb_none_0", out var cobweb)) characterg.DrawBitmap(cobweb, rect);
                    }
                    else
                    {
                        if (_data.Alldngnpng.TryGetValue("net_trap", out var net)) characterg.DrawBitmap(net, rect);
                    }
                    
                }

                var rect2 = new SKRect(x, y, x + (CharacterSKBitmap.Width * resize), y + (CharacterSKBitmap.Height * resize));
                g.DrawBitmap(CharacterSKBitmap, rect2);

                return true;
            }
        }

        public void DrawTiles(SKCanvas g, Model model, float startX, float startY, int startIndex, Dictionary<string, string> overrides, float resize = 1)
        {
            var dict = new Dictionary<string, string>();//logging
            var SKBitmapList = new List<Tuple<string, SKBitmap>>(model.TileNames.Length);

            _outOfSightCache.DumpDataOnLocationChange(model.SideData.Place);

            if (!_data.Floorandwall.TryGetValue(model.Location.ToUpper(), out var CurrentLocationFloorAndWallName)) return;

            if (!_data.FloorandwallColor.TryGetValue(model.Location.ToUpper(), out var CurrentLocationFloorAndWallColor)) return;

            if (!_data.Wallpng.TryGetValue(CurrentLocationFloorAndWallName[0], out var wall)) return;
            if (!_data.Floorpng.TryGetValue(CurrentLocationFloorAndWallName[1], out var floor)) return;

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

        private bool DrawCurrentTile(SKCanvas g, Model model, Dictionary<string, string> dict, string tile, string tileHighlight, SKBitmap wall, SKBitmap floor, Tuple<List<string>, List<string>> wallAndFloorColors, Dictionary<string, string> overrides, float x, float y, float resize, out SKBitmap drawnTile)
        {
            if (tile[0] == ' ' && (tileHighlight == Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTGREY) || tileHighlight == Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)) || tile.StartsWith("@BL")) { 
                drawnTile = null; 
                return false;
            }

            SKBitmap brandToDraw = null;
            bool cached = false;
            if (tile.TryDrawWallOrFloor(tileHighlight, wall, floor, wallAndFloorColors, out drawnTile) ||
                tile.TryDrawMonster(tileHighlight, overrides, _data.Monsterpng, _data.Miscallaneous, floor, out drawnTile, out brandToDraw) ||//first try drawing overrides, that include blue color monsters, and monsters in sight
                tile.TryDrawCachedTile(tileHighlight, _outOfSightCache, new List<char> {'!', '?', '=', '"', '$', ')', '[', '_', '}', '/', '(', ':', '|', '%', '÷', '†', '*'}, new List<string> { "≈RED"}, out drawnTile, out cached) ||
                tile.TryDrawMonster(tileHighlight, _data.Monsterdata, _data.Monsterpng, _data.Miscallaneous, floor, out drawnTile, out brandToDraw) ||//draw the rest of the monsters
                tile.TryDrawFeature(tileHighlight, _data.Features, _data.Alldngnpng, _data.Miscallaneous, floor, wall, model.Location, out drawnTile) ||
                tile.TryDrawCloud(_data.Cloudtiles, _data.Alleffects, floor, model.SideData, model.MonsterData, out drawnTile) ||
                tile.TryDrawItem(tileHighlight, _data.Itemdata, _data.Itempng, _data.Miscallaneous, floor, model.Location, out drawnTile) ||
                tile.TryDrawCachedTileInView(tileHighlight, _outOfSightCache, new List<char> { '!', '?', '=', '"', '$', ')', '[', '_', '}', '/', '(', ':', '|', '%', '÷', '†', '*' }, new List<string> { "≈RED" }, out drawnTile)) 
            {
                var rect = new SKRect(x, y, x+ ( drawnTile.Width * resize),y+ ( drawnTile.Height * resize));
                g.DrawBitmap(drawnTile, rect);

                if (brandToDraw != null)
                {
                    g.DrawBitmap(brandToDraw, new SKRect(x, y, x + (brandToDraw.Width * resize), y + (brandToDraw.Height * resize)));
                }
                else if (!tileHighlight.Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)) && (!tile.Substring(1).Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)) || tile[0] == '.'))
                {
                    using var backgroundPaint = new SKPaint();
                    backgroundPaint.Color = ColorList.GetColor(tileHighlight).WithAlpha(100);
                    backgroundPaint.Style = SKPaintStyle.StrokeAndFill;
                    g.DrawRect(rect, backgroundPaint);
                }
                if (cached)//darken to match out of sight
                {
                    using var backgroundPaint = new SKPaint();
                    backgroundPaint.Color = new SKColor(0, 0, 0, 150);
                    backgroundPaint.Style = SKPaintStyle.StrokeAndFill;
                    g.DrawRect(rect, backgroundPaint);
                }

                return true; 
            }

            if (!dict.ContainsKey(tile))
            {
                dict.Add(tile, "");
            }
            using var font = new SKPaint
            {
                Typeface = _typeface,
                TextSize = 24 * resize,
            };
            // centre character in bounding box
            x += (32 - font.FontMetrics.XMax) / 2;
            y -= (32 + font.FontMetrics.Top) / 2;
            g.WriteCharacter(tile, font, x, y, tileHighlight);//unhandled tile, write it as a character instead

            return false;
        }
    }
}
