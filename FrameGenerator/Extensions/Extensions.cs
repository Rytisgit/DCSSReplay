using InputParser;

namespace FrameGenerator.Extensions
{
    public static class Extensions
    {
        public static bool MonsterIsVisible(this MonsterData[] monsterData, string MonsterName) =>
            !monsterData[0].Empty && monsterData[0].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[1].Empty && monsterData[1].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[2].Empty && monsterData[2].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[3].Empty && monsterData[3].MonsterTextRaw.Contains(MonsterName);


        public static string ParseBasicWeaponName(this string fullstring)
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
            if (fullstring.Contains("staff of")) return parsedstring.Substring(9 + start, end - 8 - start).Replace(' ', '_');

            parsedstring = parsedstring.Substring(start, end - start + 1).Replace(' ', '_').Split('\"')[0];

            return parsedstring;
        }
    }
}