using FrameGenerator.OutOfSightCache;
using InputParser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace FrameGenerator.Extensions
{
    public static class TileDrawExtensions
    {
        public static bool TryDrawWallOrFloor(this string tile, Bitmap wall, Bitmap floor, out Bitmap tileToDraw)
        {
            tileToDraw = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tileToDraw))
            {
                if (tile == "#BLUE")
                {
                    g.DrawImage(wall, 0, 0, wall.Width, wall.Height);
                    var blueTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                    g.FillRectangle(blueTint, 0, 0, wall.Width, wall.Height);
                    return true;
                }

                if (tile[0] == '#' && !tile.Equals("#LIGHTCYAN"))
                {
                    g.DrawImage(wall, 0, 0, wall.Width, wall.Height);
                    return true;
                }

                if (tile == ".BLUE")
                {
                    g.DrawImage(floor, 0, 0, floor.Width, floor.Height);
                    var blueTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                    g.FillRectangle(blueTint, 0, 0, floor.Width, floor.Height);
                    return true;
                }

                if (tile[0] == '.')
                {
                    g.DrawImage(floor, 0, 0, floor.Width, floor.Height);
                    return true;
                }

                if (tile == "*BLUE")
                {
                    g.DrawImage(wall, 0, 0, wall.Width, wall.Height);
                    var blueTint = new SolidBrush(Color.FromArgb(40, 30, 30, 200));
                    g.FillRectangle(blueTint, 0, 0, wall.Width, wall.Height);
                    return true;
                }
                if (tile == ",BLUE")
                {
                    g.DrawImage(floor, 0, 0, floor.Width, floor.Height);
                    var blueTint = new SolidBrush(Color.FromArgb(40, 20, 20, 200));
                    g.FillRectangle(blueTint, 0, 0, floor.Width, floor.Height);
                    return true;
                }
            }

            return false;
        }

        public static bool TryDrawCachedTile(this string tile, Cacher outOfSightCache, out Bitmap lastSeen, out bool Cached)
        {
            //add highlight check?
            if (tile.Substring(1) == Enum.GetName(typeof(ColorList2), ColorList2.BLUE) && outOfSightCache.TryGetLastSeenBitmapByChar(tile[0], out lastSeen))
            {
                Cached = true;
                return true;
            }
            Cached = false;
            lastSeen = null;
            return false;
        }
        

        public static bool TryDrawMonster(this string tile, string background, Dictionary<string, string> monsterData, Dictionary<string, Bitmap> monsterPng, Bitmap floor, out Bitmap tileToDraw)
        {
            tileToDraw = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tileToDraw))
            {
                var isHighlighted = FixHighlight(tile, background, out var correctTile);

                if (!monsterData.TryGetValue(correctTile, out var pngName)) return false;

                //foreach (var item in monsterData)
                //{
                //    if (item.Key[0] == '*') Console.WriteLine(item.Key);
                //}
                if (!monsterPng.TryGetValue(pngName, out Bitmap png)) return false;
                //foreach (var monsterTileName in monsterData)
                //{
                //    if (!monsterPng.TryGetValue(monsterTileName.Value, out Bitmap temp)) Console.WriteLine(monsterTileName.Key + " badPngName: " + monsterTileName.Value);
                //}

                g.DrawImage(floor, 0, 0, floor.Width, floor.Height);
                g.DrawImage(png, 0, 0, png.Width, png.Height);
            }
            return true;
        }

        public static bool TryDrawMonster(this string tile, string background, Dictionary<string, string> monsterData, Dictionary<string, Bitmap> monsterPng, Dictionary<string, string> overrides, out Bitmap tileToDraw)
        {
            tileToDraw = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tileToDraw))
            {
                if (tile.StartsWith("@BL")) return false;//player tile draw override TODO
                var isHighlighted = FixHighlight(tile, background, out var correctTile);
                string pngName;
                if (!overrides.TryGetValue(correctTile, out pngName))
                {
                    if (!monsterData.TryGetValue(correctTile, out pngName)) return false;
                }
                if (!monsterPng.TryGetValue(pngName, out Bitmap png)) return false;

                g.DrawImage(png, 0, 0, png.Width, png.Height);
            }
            return true;
        }

        public static bool TryDrawFeature(this string tile, Dictionary<string, string> featureData, Dictionary<string, Bitmap> allDungeonPngs, Bitmap floor, out Bitmap tileToDraw)
        {
            tileToDraw = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tileToDraw))
            {
                if (!featureData.TryGetValue(tile, out var pngName)) return false;
                if (!allDungeonPngs.TryGetValue(pngName, out Bitmap png)) return false;

                g.DrawImage(floor, 0, 0, floor.Width, floor.Height);
                g.DrawImage(png, 0, 0, png.Width, png.Height);
            }
            return true;
        }

        private static bool FixHighlight(string tile, string backgroundColor, out string correctTile)//if highlighted, returns fixed string
        {
            if (backgroundColor.Equals(Enum.GetName(typeof(ColorList2), ColorList2.BLACK)) || !tile.Substring(1).Equals(Enum.GetName(typeof(ColorList2), ColorList2.BLACK)))
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

        public static bool TryDrawItem(this string tile, string background, Dictionary<string, string> itemData, Dictionary<string, Bitmap> itemPngs, Dictionary<string, Bitmap> miscPngs, Bitmap floor, string location, out Bitmap tileToDraw)
        {
            tileToDraw = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tileToDraw))
            {
                var isHighlighted = FixHighlight(tile, background, out var correctTile);
                Bitmap underneathIcon;

                if (!itemData.TryGetValue(correctTile, out var pngName)) return false;

                g.DrawImage(floor, 0, 0, floor.Width, floor.Height);

                var demonicWeaponLocations = new List<string>() { "Hell", "Dis", "Gehenna", "Cocytus", "Tartarus", "Vaults", "Depths" };

                if (correctTile[0] == ')' //is weapon
                    && correctTile.Substring(1).Equals(Enum.GetName(typeof(ColorList2), ColorList2.LIGHTRED)) //is lightred
                    && demonicWeaponLocations.Contains(location)) // is in a location with a lot of demon weapons
                {
                    if (itemPngs.TryGetValue("demon_blade2", out Bitmap demonBlade))
                    {
                        g.DrawImage(demonBlade, 0, 0, demonBlade.Width, demonBlade.Height);

                        if (isHighlighted && miscPngs.TryGetValue("something_under", out underneathIcon)) g.DrawImage(underneathIcon, 0, 0, underneathIcon.Width, underneathIcon.Height);
                        return true;
                    }
                }

                if (!itemPngs.TryGetValue(pngName, out Bitmap png)) return false;

                g.DrawImage(png, 0, 0, png.Width, png.Height);

                if (isHighlighted && miscPngs.TryGetValue("something_under", out underneathIcon)) g.DrawImage(underneathIcon, 0, 0, underneathIcon.Width, underneathIcon.Height);
            }
            return true;
        }

        public static bool TryDrawCloud(this string tile, Dictionary<string, string> cloudData, Dictionary<string, Bitmap> effectPngs, Bitmap floor, SideData sideData, MonsterData[] monsterData, out Bitmap tileToDraw)
        {
            tileToDraw = new Bitmap(32, 32, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(tileToDraw))
            {
                if (!cloudData.TryGetValue(tile, out var nam)) return false; //check if valid cloud

                g.DrawImage(floor, 0, 0, floor.Width, floor.Height);

                //check special rules first before drawing normal

                var durationLength = new Dictionary<char, int>() { { '°', 0 }, { '○', 1 }, { '☼', 2 }, { '§', 3 } };
                var durationChar = new char[4] { '°', '○', '☼', '§' };

                var tileColor = tile.Substring(1);

                if (sideData.Statuses1.Contains("Torna") || sideData.Statuses2.Contains("Torna"))//tornado override
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

                    if (t1Colors.Contains(tileColor))
                    {
                        if (effectPngs.TryGetValue("tornado1", out var bmp))
                        {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                            return true;
                        }
                    }

                    if (t2Colors.Contains(tileColor))
                    {
                        if (effectPngs.TryGetValue("tornado2", out Bitmap bmp))
                        {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                            return true;
                        }
                    }
                }

                if (sideData.Place.Contains("Salt"))
                {
                    var colors = new List<string>() {
                    Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY),
                    Enum.GetName(typeof(ColorList2), ColorList2.WHITE) };

                    if (tile[0].Equals('§'))
                    {
                        if (colors.Contains(tileColor))
                        {
                            if (effectPngs.TryGetValue("cloud_grey_smoke", out Bitmap bmp))
                            {
                                g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                                return true;
                            }
                        }
                    }
                }

                if (sideData.Race.Contains("of Qazlal"))
                {

                    var stormColors = new List<string>() {
                    Enum.GetName(typeof(ColorList2), ColorList2.LIGHTGREY),
                    Enum.GetName(typeof(ColorList2), ColorList2.DARKGREY) };

                    if (tile[0].Equals(durationChar[3]))
                    {
                        if (stormColors.Contains(tileColor))
                        {
                            if (effectPngs.TryGetValue("cloud_storm2", out Bitmap bmp))
                            {
                                g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                                return true;
                            }
                        }
                    }

                    if (tileColor == Enum.GetName(typeof(ColorList2), ColorList2.GREEN)) //replace poison cloud with dust
                    {
                        if (effectPngs.TryGetValue("cloud_dust" + durationLength[tile[0]], out Bitmap bmp))
                        {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                            return true;
                        }
                    }

                }

                if (monsterData.MonsterIsVisible("catob"))//when catoblepass is on screen, white clouds are calcifiyng
                {
                    if (tile[0].Equals(durationChar[3]) && tileColor.Equals(Enum.GetName(typeof(ColorList2), ColorList2.WHITE)))
                    {
                        if (effectPngs.TryGetValue("cloud_calc_dust2", out Bitmap bmp))
                        {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                            return true;
                        }

                    }
                }

                if (sideData.Place.Contains("Shoal"))//in shoals darkgrey clouds are ink
                {
                    if (tile[0].Equals(durationChar[3]) && tileColor.Equals(Enum.GetName(typeof(ColorList2), ColorList2.DARKGREY)))
                    {
                        if (effectPngs.TryGetValue("ink_full", out Bitmap bmp))
                        {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                            return true;
                        }
                    }
                }

                if (monsterData.MonsterIsVisible("ophan"))//ophans make holy flame
                {
                    if (tile[0].Equals(durationChar[3]) && tileColor.Equals(Enum.GetName(typeof(ColorList2), ColorList2.WHITE)))
                    {
                        if (effectPngs.TryGetValue("cloud_yellow_smoke", out Bitmap bmp))
                        {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                            return true;
                        }

                    }
                }

                if (sideData.Statuses1.Contains("Storm") || sideData.Statuses2.Contains("Storm"))//wu jian heavenly storm
                {
                    var stormColors = new List<string>() {
                    Enum.GetName(typeof(ColorList2), ColorList2.WHITE),
                    Enum.GetName(typeof(ColorList2), ColorList2.YELLOW)};

                    if (stormColors.Contains(tileColor))
                    {
                        if (effectPngs.TryGetValue("cloud_gold_dust" + durationLength[tile[0]], out Bitmap bmp))
                        {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                            return true;
                        }
                    }
                }

                if (!effectPngs.TryGetValue(nam, out Bitmap chr)) return false;

                g.DrawImage(chr, 0, 0, chr.Width, chr.Height);
            }
            return true;
        }
    }
}