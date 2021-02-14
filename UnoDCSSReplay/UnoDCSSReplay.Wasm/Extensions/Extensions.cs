using System;
using InputParser;
using System.Collections.Generic;

namespace FrameGenerator.wasm.Extensions
{
    public static class Extensions
    {
        public static bool MonsterIsVisible(this MonsterData[] monsterData, string MonsterName) =>
            !monsterData[0].Empty && monsterData[0].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[1].Empty && monsterData[1].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[2].Empty && monsterData[2].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[3].Empty && monsterData[3].MonsterTextRaw.Contains(MonsterName);


        public static string ParseUniqueWeaponName(this string fullstring)
        {

            string parsedstring = fullstring.Replace("\'", "");

            bool onlyonce = true;
            int start = 0;
            int end = 0;
            for (int i = 3; i < parsedstring.Length; i++)
            {
                if (parsedstring[i] == '\"')
                {
                    start = i + 1;
                    end = parsedstring.Length - 1;
                    break;
                }
                if (parsedstring[i] == '\0' || parsedstring[i] == '{' || parsedstring[i] == '(') break;
                if (Char.IsLetter(parsedstring[i]) && onlyonce)
                {
                    start = i;
                    onlyonce = false;
                }
                else if (Char.IsLetter(parsedstring[i]))
                {
                    end = i;
                }
            }

            if (fullstring.Contains("sword of power"))
            {
                return "sword_of_power";
            }
            if (fullstring.Contains("thermic engine"))
            {
                return "maxwells_thermic_engine";
            }


            parsedstring = parsedstring.Substring(start, end - start + 1).Replace(' ', '_').Split('\"')[0];
            return parsedstring.ToLower();
        }
        public static string GetNonUniqueWeaponName(this string fullstring, Dictionary<string, string> weapondata)
        {
            foreach (KeyValuePair<string, string> entry in weapondata)
            {
                if (fullstring.Contains(entry.Key)) return entry.Value;
            }

            return fullstring;
        }

        public static void AddUglyThingOverrides(this Dictionary<string, string> finalOverrides, MonsterData monsterLine)
        {
            if (monsterLine.MonsterTextRaw.Contains("very"))
            {
                foreach (var monstertileName in monsterLine.MonsterDisplay
                ) //can be maelstron on screen and vortex not in list of monster, but ehhh
                {
                    var pngName = "";
                    pngName = (monstertileName.Substring(1)) switch
                    {
                        "BLACK" => "very_ugly_thing",
                        "RED" => "very_ugly_thing",
                        "GREEN" => "very_ugly_thing2",
                        "BROWN" => "very_ugly_thing1",
                        "BLUE" => "very_ugly_thing3",
                        "MAGENTA" => "very_ugly_thing4",
                        "CYAN" => "very_ugly_thing3",
                        "LIGHTGREY" => "very_ugly_thing5",
                        "DARKGREY" => "very_ugly_thing1",
                        "LIGHTRED" => "very_ugly_thing",
                        "LIGHTGREEN" => "very_ugly_thing2",
                        "YELLOW" => "very_ugly_thing1",
                        "LIGHTBLUE" => "very_ugly_thing3",
                        "LIGHTMAGENTA" => "very_ugly_thing4",
                        "LIGHTCYAN" => "very_ugly_thing3",
                        "WHITE" => "very_ugly_thing5",
                        _ => "very_ugly_thing5",
                    };
                    if (!finalOverrides.TryGetValue(monstertileName, out _)) finalOverrides.Add(monstertileName, pngName);
                }
            }
            else
            {
                foreach (var monstertileName in monsterLine.MonsterDisplay
                ) //can be maelstron on screen and vortex not in list of monster, but ehhh
                {
                    var pngName = "";
                    pngName = (monstertileName.Substring(1)) switch
                    {
                        "BLACK" => "ugly_thing",
                        "RED" => "ugly_thing",
                        "GREEN" => "ugly_thing2",
                        "BROWN" => "ugly_thing1",
                        "BLUE" => "ugly_thing3",
                        "MAGENTA" => "ugly_thing4",
                        "CYAN" => "ugly_thing3",
                        "LIGHTGREY" => "ugly_thing5",
                        "DARKGREY" => "ugly_thing1",
                        "LIGHTRED" => "ugly_thing",
                        "LIGHTGREEN" => "ugly_thing2",
                        "YELLOW" => "ugly_thing1",
                        "LIGHTBLUE" => "ugly_thing3",
                        "LIGHTMAGENTA" => "ugly_thing4",
                        "LIGHTCYAN" => "ugly_thing3",
                        "WHITE" => "ugly_thing5",
                        _ => "ugly_thing5",
                    };
                    if (!finalOverrides.TryGetValue(monstertileName, out _)) finalOverrides.Add(monstertileName, pngName);
                }
            }
        }

        public static void AddSlimeOverrides(this Dictionary<string, string> finalOverrides, MonsterData monsterLine)
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

        public static void AddTiamatOverrides(this Dictionary<string, string> finalOverrides, MonsterData monsterLine)
        {
            foreach (var monstertileName in monsterLine.MonsterDisplay)//well these colours are wrong
            {
                var pngName = "";
                pngName = (monstertileName.Substring(1)) switch
                {
                    "BLACK" => "tiamat_black",
                    "RED" => "tiamat_red",
                    "GREEN" => "tiamat_green",
                    "BROWN" => "tiamat_black",
                    "BLUE" => "tiamat_mottled",
                    "MAGENTA" => "tiamat_purple",
                    "CYAN" => "tiamat_pale",
                    "LIGHTGREY" => "tiamat_grey",
                    "DARKGREY" => "tiamat_pale",
                    "LIGHTRED" => "tiamat_red",
                    "LIGHTGREEN" => "tiamat_green",
                    "YELLOW" => "tiamat_yellow",
                    "LIGHTBLUE" => "tiamat_black",
                    "LIGHTMAGENTA" => "tiamat_mottled",
                    "LIGHTCYAN" => "tiamat_pale",
                    "WHITE" => "tiamat_white",
                    _ => "tiamat_yellow",
                };
                if (!finalOverrides.TryGetValue(monstertileName, out _)) finalOverrides.Add(monstertileName, pngName);
            }
        }

        public static void AddVortexOverrides(this Dictionary<string, string> finalOverrides, MonsterData monsterLine)
        {
            foreach (var monstertileName in monsterLine.MonsterDisplay)//can be maelstron on screen and vortex not in list of monster, but ehhh
            {
                var pngName = "";
                pngName = (monstertileName.Substring(1)) switch
                {
                    "BLACK" => "spatial_vortex1",
                    "RED" => "spatial_vortex1",
                    "GREEN" => "spatial_vortex1",
                    "BROWN" => "spatial_vortex1",
                    "BLUE" => "spatial_vortex2",
                    "MAGENTA" => "spatial_vortex2",
                    "CYAN" => "spatial_vortex2",
                    "LIGHTGREY" => "spatial_vortex2",
                    "DARKGREY" => "spatial_vortex3",
                    "LIGHTRED" => "spatial_vortex3",
                    "LIGHTGREEN" => "spatial_vortex3",
                    "YELLOW" => "spatial_vortex3",
                    "LIGHTBLUE" => "spatial_vortex3",
                    "LIGHTMAGENTA" => "spatial_vortex4",
                    "LIGHTCYAN" => "spatial_vortex4",
                    "WHITE" => "spatial_vortex4",
                    _ => "spatial_vortex3",
                };
                if (!finalOverrides.TryGetValue(monstertileName, out _)) finalOverrides.Add(monstertileName, pngName);
            }
        }

        public static void AddChaosSpawnOverrides(this Dictionary<string, string> finalOverrides, MonsterData monsterLine)
        {
            foreach (var monstertileName in monsterLine.MonsterDisplay)
            {
                var pngName = "";
                pngName = (monstertileName.Substring(1)) switch
                {
                    "BLACK" => "chaos_spawn4",
                    "RED" => "chaos_spawn1",
                    "GREEN" => "chaos_spawn5",
                    "BROWN" => "chaos_spawn4",
                    "BLUE" => "chaos_spawn3",
                    "MAGENTA" => "chaos_spawn2",
                    "CYAN" => "chaos_spawn3",
                    "LIGHTGREY" => "chaos_spawn5",
                    "DARKGREY" => "chaos_spawn4",
                    "LIGHTRED" => "chaos_spawn1",
                    "LIGHTGREEN" => "chaos_spawn5",
                    "YELLOW" => "chaos_spawn4",
                    "LIGHTBLUE" => "chaos_spawn3",
                    "LIGHTMAGENTA" => "chaos_spawn2",
                    "LIGHTCYAN" => "chaos_spawn3",
                    "WHITE" => "chaos_spawn2",
                    _ => "chaos_spawn4",
                };
                if (!finalOverrides.TryGetValue(monstertileName, out _)) finalOverrides.Add(monstertileName, pngName);
            }
        }

        public static void AddKlownOverrides(this Dictionary<string, string> finalOverrides, MonsterData monsterLine)
        {
            finalOverrides.Add("pBLACK", "killer_klown_yellow");
            finalOverrides.Add("pRED", "killer_klown_red");
            finalOverrides.Add("pGREEN", "killer_klown_green");
            finalOverrides.Add("pBROWN", "killer_klown_yellow");
            finalOverrides.Add("pBLUE", "killer_klown_blue");
            finalOverrides.Add("pMAGENTA", "killer_klown_purple");
            finalOverrides.Add("pCYAN", "killer_klown_blue");
            finalOverrides.Add("pLIGHTGREY", "killer_klown_green");
            finalOverrides.Add("pDARKGREY", "killer_klown_yellow");
            finalOverrides.Add("pLIGHTRED", "killer_klown_red");
            finalOverrides.Add("pLIGHTGREEN", "killer_klown_green");
            finalOverrides.Add("pYELLOW", "killer_klown_yellow");
            finalOverrides.Add("pLIGHTBLUE", "killer_klown_blue");
            finalOverrides.Add("pLIGHTMAGENTA", "killer_klown_purple");
            finalOverrides.Add("pLIGHTCYAN", "killer_klown_blue");
            finalOverrides.Add("pWHITE", "killer_klown_purple");
        }

        public static void AddColorDependantOverrides(this Dictionary<string, string> finalOverrides, MonsterData monsterLine)
        {
            if (monsterLine.MonsterTextRaw.Contains("Klown"))
            {
                finalOverrides.AddKlownOverrides(monsterLine);
            }

            if (monsterLine.MonsterTextRaw.Contains("chaos"))
            {
                finalOverrides.AddChaosSpawnOverrides(monsterLine);
            }

            if (monsterLine.MonsterTextRaw.Contains("spatial"))
            {
                finalOverrides.AddVortexOverrides(monsterLine);
            }

            if (monsterLine.MonsterTextRaw.Contains("Tiamat"))
            {
                finalOverrides.AddTiamatOverrides(monsterLine);
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
                finalOverrides.AddSlimeOverrides(monsterLine);
            }

            if (monsterLine.MonsterTextRaw.Contains("ugly") && monsterLine.MonsterDisplay[0][0] == 'u')
            {
                finalOverrides.AddUglyThingOverrides(monsterLine);
            }
        }

        public static void OverideTiles(this Model model, Dictionary<string, string> tileOverides)
        {
            if (tileOverides == null) return;
            foreach(var key in tileOverides.Keys)
            {
                for (int i = 0; i < model.TileNames.Length; i++)
                {
                    if (model.TileNames[i].Contains(key))
                    {
                        Console.WriteLine(key);
                       // System.Console.WriteLine(model.TileNames[i]);
                        string newtile = tileOverides[key] + model.TileNames[i].Substring(1);
                        model.TileNames[i] = newtile;
                        Console.WriteLine(model.TileNames[i]);
                    }
                }
            }       
        }


        public static bool IsWallOrFloor(this string tilename) => tilename[0] == '#' || tilename[0] == '.' || tilename[0] == ',' || tilename[0] == '*' || tilename[0] == '≈';
    }
}