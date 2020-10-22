using FrameGenerator.OutOfSightCache;
using InputParser;
using System;
using System.Collections.Generic;
using InputParser.Constant;
using SkiaSharp;

namespace FrameGenerator.Extensions
{
    public static class TileDrawExtensions
    {
        public static bool TryDrawWallOrFloor(this string tile, string background, SKBitmap wall, SKBitmap floor, string[] wallAndFloorColors, out SKBitmap tileToDraw)
        {
            var highlighted = FixHighlight(tile, background, out var correctTile);
            tileToDraw = new SKBitmap(32, 32);

            using (SKCanvas g = new SKCanvas(tileToDraw))
            {
                if (correctTile == "#BLUE")
                {
                    g.DrawBitmap(wall, new SKRect(0, 0, wall.Width, wall.Height));
                    g.DrawColor(new SKColor(0, 0, 0, 150), SKBlendMode.Overlay);
                    return true;
                }

                if (correctTile[0] == '#' && correctTile.Substring(1).Equals(wallAndFloorColors[0]))
                {
                    g.DrawBitmap(wall, new SKRect(0, 0, wall.Width, wall.Height));
                    return true;
                }

                if (correctTile == ".BLUE")
                {
                    g.DrawBitmap(floor, new SKRect(0, 0, floor.Width, floor.Height));
                    g.DrawColor(new SKColor(0, 0, 0, 150), SKBlendMode.Overlay);
                    return true;
                }

                if (correctTile[0] == '.' && correctTile.Substring(1).Equals(wallAndFloorColors[1]))
                {
                    g.DrawBitmap(floor, new SKRect(0, 0, floor.Width, floor.Height));
                    return true;
                }

                if (correctTile == "*BLUE")
                {
                    g.DrawBitmap(wall, new SKRect(0, 0, wall.Width, wall.Height));
                    g.DrawColor(new SKColor(40, 30, 30, 200), SKBlendMode.Overlay);
                    return true;
                }
                if (correctTile == ",BLUE")
                {
                    g.DrawBitmap(floor, new SKRect(0, 0, floor.Width, floor.Height));
                    g.DrawColor(new SKColor(40, 20, 20, 200), SKBlendMode.Overlay);
                    return true;
                }
            }

            return false;
        }

        public static bool TryDrawCachedTile(this string tile, string highlight, Cacher outOfSightCache, List<char> noCache, List<string> list, out SKBitmap lastSeen, out bool Cached)
        {
            Cached = false;
            lastSeen = null;
            if (noCache.Contains(tile[0]))
            {
                return false;
            }
            if (list.Contains(tile))
            {
                return false;
            }
            if (tile.Substring(1) == Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLUE) && outOfSightCache.TryGetLastSeenBitmapByChar(tile[0], out lastSeen))
            {
                Cached = true;
                return true;
            }
            if (tile.Substring(1) == Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK) && highlight != Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK) &&
                outOfSightCache.TryGetLastSeenBitmapByChar(tile[0], out lastSeen))
            {
                return true;
            }
            
            return false;
        }
        

        public static bool TryDrawMonster(this string tile, string background, Dictionary<string, string> monsterData, Dictionary<string, SKBitmap> monsterPng, Dictionary<string, SKBitmap> miscPng, SKBitmap floor, out SKBitmap tileToDraw, out SKBitmap BrandToDraw)
        {
            tileToDraw = new SKBitmap(32, 32);
            BrandToDraw = null;
            using (SKCanvas g = new SKCanvas(tileToDraw))
            {
                var isHighlighted = FixHighlight(tile, background, out var correctTile);

                if (!monsterData.TryGetValue(correctTile, out var pngName)) return false;

                //foreach (var item in monsterData)
                //{
                //    if (item.Key[0] == '*') Console.WriteLine(item.Key);
                //}
                if (!monsterPng.TryGetValue(pngName, out SKBitmap png)) return false;
                //foreach (var monsterTileName in monsterData)
                //{
                //    if (!monsterPng.TryGetValue(monsterTileName.Value, out SKBitmap temp)) Console.WriteLine(monsterTileName.Key + " badPngName: " + monsterTileName.Value);
                //}

                if (tile.Substring(1) != Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK))
                {

                    //if (background == Enum.GetName(typeof(ColorList2), ColorList2.BROWN))//idk
                    //{
                    //    miscPng.TryGetValue("good_neutral", out BrandToDraw);
                    //}
                    if (background == Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTGREY))
                    {
                        miscPng.TryGetValue("neutral", out BrandToDraw);
                    }
                    if (background == Enum.GetName(typeof(ColorListEnum), ColorListEnum.BROWN))
                    {
                        miscPng.TryGetValue("may_stab_brand", out BrandToDraw);
                    }
                    if (background == Enum.GetName(typeof(ColorListEnum), ColorListEnum.GREEN))
                    {
                        miscPng.TryGetValue("friendly", out BrandToDraw);
                    }
                    if (background == Enum.GetName(typeof(ColorListEnum), ColorListEnum.YELLOW))
                    {
                        miscPng.TryGetValue("may_stab_brand", out BrandToDraw);
                    }
                    if (background == Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLUE))
                    {
                        miscPng.TryGetValue("sleeping", out BrandToDraw);
                    }
                }
                var rect = new SKRect(0, 0, floor.Width, floor.Width);
                g.DrawBitmap(floor, rect);
                g.DrawBitmap(png, rect);
            }
            return true;
        }

        public static bool TryDrawMonster(this string tile, string background, Dictionary<string, string> monsterData, Dictionary<string, SKBitmap> monsterPng, Dictionary<string, string> overrides, out SKBitmap tileToDraw)
        {
            tileToDraw = new SKBitmap(32, 32);

            using (SKCanvas g = new SKCanvas(tileToDraw))
            {
                g.Clear(SKColors.Black);
                if (tile.StartsWith("@BL")) return false;//player tile draw override TODO
                var isHighlighted = FixHighlight(tile, background, out var correctTile);
                string pngName;
                if (!overrides.TryGetValue(correctTile, out pngName))
                {
                    if (!monsterData.TryGetValue(correctTile, out pngName)) return false;
                }
                if (!monsterPng.TryGetValue(pngName, out SKBitmap png)) return false;

                g.DrawBitmap(png, new SKRect(0, 0, png.Width, png.Height));

            }
            return true;
        }

        public static bool TryDrawFeature(this string tile, string background, Dictionary<string, string> featureData, Dictionary<string, SKBitmap> allDungeonPngs, Dictionary<string, SKBitmap> misc, SKBitmap floor, SKBitmap wall, string location, out SKBitmap tileToDraw)
        {

            var highlighted = FixHighlight(tile, background, out var correctTile);
            tileToDraw = new SKBitmap(32, 32);

            using (SKCanvas g = new SKCanvas(tileToDraw))
            {

                if (!featureData.TryGetValue(correctTile, out var pngName)) return false;
                if (pngName == "wall")
                {
                    g.DrawBitmap(wall, new SKRect(0, 0, wall.Width, wall.Height));

                    if (correctTile == "#RED")
                    {
                        if (misc.TryGetValue("blood_red00", out SKBitmap blood))
                        {
                            g.DrawBitmap(blood, new SKRect(0, 0, blood.Width, blood.Height));
                        }
                        return true;
                    }
                }
               

                if (pngName == "floor")
                {
                    g.DrawBitmap(floor, new SKRect(0, 0, floor.Width, floor.Height));
                    if (correctTile.Substring(1) == Enum.GetName(typeof(ColorListEnum), ColorListEnum.RED))
                    {
                        if (misc.TryGetValue("blood_puddle_red", out SKBitmap blood))
                        { 
                            g.DrawBitmap(blood, new SKRect(0, 0, blood.Width, blood.Height)); 
                        }
                    }
                    return true;
                }

                SKBitmap png = null;

                if (pngName == "net" && location.Contains("Spider"))
                {
                    if (!allDungeonPngs.TryGetValue("cobweb_NESW", out png)) return false;
                }
                else
                {
                    if (!allDungeonPngs.TryGetValue(pngName, out png)) return false;
                }

                g.DrawBitmap(floor, new SKRect(0, 0, floor.Width, floor.Height));
                g.DrawBitmap(png, new SKRect(0, 0, png.Width, png.Height));
            }
            return true;
        }

        private static bool FixHighlight(string tile, string backgroundColor, out string correctTile)//if highlighted, returns fixed string
        {
            if (backgroundColor == null || backgroundColor.Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)) || !tile.Substring(1).Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLACK)))
            {
                correctTile = tile;
                return false;
            }
            else
            {
                correctTile = tile[0] + backgroundColor;
            }
            return true;
        }

        public static bool TryDrawItem(this string tile, string background, Dictionary<string, string> itemData, Dictionary<string, SKBitmap> itemPngs, Dictionary<string, SKBitmap> miscPngs, SKBitmap floor, string location, out SKBitmap tileToDraw)
        {
            tileToDraw = new SKBitmap(32, 32);

            using (SKCanvas g = new SKCanvas(tileToDraw))
            {
                var isHighlighted = FixHighlight(tile, background, out var correctTile);
                SKBitmap underneathIcon;

                if (!itemData.TryGetValue(correctTile, out var pngName)) return false;

                g.DrawBitmap(floor, new SKRect(0, 0, floor.Width, floor.Height));

                var demonicWeaponLocations = new List<string>() { "Hell", "Dis", "Gehenna", "Cocytus", "Tartarus", "Vaults", "Depths" };

                if (correctTile[0] == ')' //is weapon
                    && correctTile.Substring(1).Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTRED)) //is lightred
                    && demonicWeaponLocations.Contains(location)) // is in a location with a lot of demon weapons
                {
                    if (itemPngs.TryGetValue("demon_blade2", out SKBitmap demonBlade))
                    {
                        g.DrawBitmap(demonBlade, new SKRect(0, 0, demonBlade.Width, demonBlade.Height));

                        if (isHighlighted && miscPngs.TryGetValue("something_under", out underneathIcon)) g.DrawBitmap(underneathIcon, new SKRect(0, 0, underneathIcon.Width, underneathIcon.Height));
                        return true;
                    }
                }

                if (!itemPngs.TryGetValue(pngName, out SKBitmap png)) return false;

                g.DrawBitmap(png, new SKRect(0, 0, png.Width, png.Height));

                if (isHighlighted && miscPngs.TryGetValue("something_under", out underneathIcon)) g.DrawBitmap(underneathIcon, new SKRect(0, 0, underneathIcon.Width, underneathIcon.Height));
            }
            return true;
        }

        public static bool TryDrawCloud(this string tile, Dictionary<string, string> cloudData, Dictionary<string, SKBitmap> effectPngs, SKBitmap floor, SideData sideData, MonsterData[] monsterData, out SKBitmap tileToDraw)
        {
            tileToDraw = new SKBitmap(32, 32);

            using (SKCanvas g = new SKCanvas(tileToDraw))
            {
                if (!cloudData.TryGetValue(tile, out var nam)) return false; //check if valid cloud

                g.DrawBitmap(floor, new SKRect(0, 0, floor.Width, floor.Height));

                //check special rules first before drawing normal

                var durationLength = new Dictionary<char, int>() { { '°', 0 }, { '○', 1 }, { '☼', 2 }, { '§', 3 } };
                var durationChar = new char[4] { '°', '○', '☼', '§' };

                var tileColor = tile.Substring(1);

                if (sideData.Statuses1.Contains("Torna") || sideData.Statuses2.Contains("Torna"))//tornado override
                {
                    var t1Colors = new List<string>() {
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTRED),
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTCYAN),
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTBLUE),
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.WHITE) };
                    var t2Colors = new List<string>() {
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.RED),
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.CYAN),
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.BLUE),
                            Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTGREY) };

                    if (t1Colors.Contains(tileColor))
                    {
                        if (effectPngs.TryGetValue("tornado1", out var bmp))
                        {
                            g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                            return true;
                        }
                    }

                    if (t2Colors.Contains(tileColor))
                    {
                        if (effectPngs.TryGetValue("tornado2", out SKBitmap bmp))
                        {
                            g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                            return true;
                        }
                    }
                }

                if (sideData.Place.Contains("Salt"))
                {
                    var colors = new List<string>() {
                    Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTGREY),
                    Enum.GetName(typeof(ColorListEnum), ColorListEnum.WHITE) };

                    if (tile[0].Equals('§'))
                    {
                        if (colors.Contains(tileColor))
                        {
                            if (effectPngs.TryGetValue("cloud_grey_smoke", out SKBitmap bmp))
                            {
                                g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                                return true;
                            }
                        }
                    }
                }

                if (sideData.Race.Contains("of Qazlal"))
                {

                    var stormColors = new List<string>() {
                    Enum.GetName(typeof(ColorListEnum), ColorListEnum.LIGHTGREY),
                    Enum.GetName(typeof(ColorListEnum), ColorListEnum.DARKGREY) };

                    if (tile[0].Equals(durationChar[3]))
                    {
                        if (stormColors.Contains(tileColor))
                        {
                            if (effectPngs.TryGetValue("cloud_storm2", out SKBitmap bmp))
                            {
                                g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                                return true;
                            }
                        }
                    }

                    if (tileColor == Enum.GetName(typeof(ColorListEnum), ColorListEnum.GREEN)) //replace poison cloud with dust
                    {
                        if (effectPngs.TryGetValue("cloud_dust" + durationLength[tile[0]], out SKBitmap bmp))
                        {
                            g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                            return true;
                        }
                    }

                }

                if (monsterData.MonsterIsVisible("catob"))//when catoblepass is on screen, white clouds are calcifiyng
                {
                    if (tile[0].Equals(durationChar[3]) && tileColor.Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.WHITE)))
                    {
                        if (effectPngs.TryGetValue("cloud_calc_dust2", out SKBitmap bmp))
                        {
                            g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                            return true;
                        }

                    }
                }

                if (sideData.Place.Contains("Shoal"))//in shoals darkgrey clouds are ink
                {
                    if (tile[0].Equals(durationChar[3]) && tileColor.Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.DARKGREY)))
                    {
                        if (effectPngs.TryGetValue("ink_full", out SKBitmap bmp))
                        {
                            g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                            return true;
                        }
                    }
                }

                if (monsterData.MonsterIsVisible("ophan"))//ophans make holy flame
                {
                    if (tile[0].Equals(durationChar[3]) && tileColor.Equals(Enum.GetName(typeof(ColorListEnum), ColorListEnum.WHITE)))
                    {
                        if (effectPngs.TryGetValue("cloud_yellow_smoke", out SKBitmap bmp))
                        {
                            g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                            return true;
                        }

                    }
                }

                if (sideData.Statuses1.Contains("Storm") || sideData.Statuses2.Contains("Storm"))//wu jian heavenly storm
                {
                    var stormColors = new List<string>() {
                    Enum.GetName(typeof(ColorListEnum), ColorListEnum.WHITE),
                    Enum.GetName(typeof(ColorListEnum), ColorListEnum.YELLOW)};

                    if (stormColors.Contains(tileColor))
                    {
                        if (effectPngs.TryGetValue("cloud_gold_dust" + durationLength[tile[0]], out SKBitmap bmp))
                        {
                            g.DrawBitmap(bmp, new SKRect(0, 0, bmp.Width, bmp.Height));
                            return true;
                        }
                    }
                }

                if (!effectPngs.TryGetValue(nam, out SKBitmap chr)) return false;

                g.DrawBitmap(chr, new SKRect(0, 0, chr.Width, chr.Height));
            }
            return true;
        }
    }
}