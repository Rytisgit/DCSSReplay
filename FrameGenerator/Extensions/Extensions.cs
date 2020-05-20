using InputParser;
using System.Collections.Generic;

namespace FrameGenerator.Extensions
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
                if (char.IsLetter(parsedstring[i]) && onlyonce)
                {
                    start = i;
                    onlyonce = false;
                }
                else if (char.IsLetter(parsedstring[i]))
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

        public static void OverideTiles(this Model model, Dictionary<string, string> tileOverides)
        {
            if (tileOverides == null) return;
            foreach(var key in tileOverides.Keys)
            {
                for (int i = 0; i < model.TileNames.Length; i++)
                {
                    if (model.TileNames[i].Contains(key))
                    {
                        System.Console.WriteLine(key);
                       // System.Console.WriteLine(model.TileNames[i]);
                        string newtile = tileOverides[key] + model.TileNames[i].Substring(1);
                        model.TileNames[i] = newtile;
                        System.Console.WriteLine(model.TileNames[i]);
                    }
                }
            }       
        }


        public static bool IsWallOrFloor(this string tilename) => tilename[0] == '#' || tilename[0] == '.' || tilename[0] == ',' || tilename[0] == '*' || tilename[0] == '≈';
    }
}