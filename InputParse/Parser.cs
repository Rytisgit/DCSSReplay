using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace InputParse
{
    public class Parser
    {
        const int FullWidth = 80;
        const int AlmostFullWidth = 75;
        const int FullHeight = 24;
        const int GameViewWidth = 33;
        const int GameViewHeight = 17;

        private static char GetCharacter(Putty.TerminalCharacter character) => character.Character == 55328 ? ' ' : character.Character;
        private static string GetColoredCharacter(Putty.TerminalCharacter character) => GetCharacter(character) + Enum.GetName(typeof(ColorList2), character.ForegroundPaletteIndex);
        private static string GetBackgroundColor(Putty.TerminalCharacter character) => Enum.GetName(typeof(ColorList2), character.BackgroundPaletteIndex);

        private static LayoutType GetLayoutType(Putty.TerminalCharacter[,] characters, out string newlocation)
        {
            StringBuilder place = new StringBuilder();
            bool found = false;

            string sideLocation;
            newlocation = "";
            for (int i = 61; i < FullWidth; i++)
            {
                place.Append(GetCharacter(characters[i, 7]));
            }
            sideLocation = place.ToString();
            foreach (var location in Locations.locations)
            {
                if (sideLocation.Contains(location.Substring(0, 3)))
                {
                    newlocation = location;
                    sideLocation = location;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                return LayoutType.Normal;
            }
            place = new StringBuilder();
            string mapLocation;
            for (int i = 0; i < 30; i++)
            {
                place.Append(GetCharacter(characters[i, 0]));
            }
            mapLocation = place.ToString();
            if (!mapLocation.Contains("of")) return LayoutType.TextOnly;
            foreach (var location in Locations.locations)
            {
                if (mapLocation.Contains(location.Substring(0, 3)))
                {
                    newlocation = location;
                    mapLocation = location;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                return LayoutType.MapOnly;
            }
            return LayoutType.TextOnly;
        }

        public static Model ParseData(Putty.TerminalCharacter[,] chars)
        {
            if (chars == null) throw new ArgumentNullException("TerminalCharacter array is null");

            switch (GetLayoutType(chars, out var location))
            {
                case LayoutType.Normal:
                    return parseNormalLayout(chars);
                case LayoutType.TextOnly:
                    return parseTextLayout(chars);
                case LayoutType.MapOnly:
                    return parseMapLayout(chars, location);
            }
            return new Model();
        }

        private static Model parseNormalLayout(Putty.TerminalCharacter[,] characters)
        {
            Model model = new Model();
            model.Layout = LayoutType.Normal;
            model.LineLength = GameViewWidth;
            var coloredStrings = new string[GameViewWidth * GameViewHeight];
            var highlightColorStrings = new string[GameViewWidth * GameViewHeight];
            var curentChar = 0;
            try
            {

                for (int j = 0; j < GameViewHeight; j++)
                    for (int i = 0; i < GameViewWidth; i++)
                    {
                        coloredStrings[curentChar] = GetColoredCharacter(characters[i, j]);
                        highlightColorStrings[curentChar] = GetBackgroundColor(characters[i, j]);
                        curentChar++;
                    }

                model.TileNames = coloredStrings;
                model.HighlightColors = highlightColorStrings;

                model.SideData = ParseSideData(characters);
                
                model.LogData = ParseLogLines(characters); 

                model.MonsterData = ParseMonsterDisplay(characters);
            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return model;
        }

        private static LogData[] ParseLogLines(Putty.TerminalCharacter[,] characters)
        {
            var loglines = new LogData[6] { new LogData(), new LogData(), new LogData(), new LogData(), new LogData(), new LogData() };
            StringBuilder logLine = new StringBuilder();
            var logText = new List<string>();
            var logBackground = new List<string>();
            var loglineRow = 17;
            foreach (var line in loglines)
            {
                for (int i = 0; i < FullWidth; i++)
                {
                    logLine.Append(GetCharacter(characters[i, loglineRow]));
                }
                line.LogTextRaw = logLine.ToString();
                if (line.LogTextRaw.Length > 0)
                {
                    line.empty = false;
                    for (int i = 0; i < line.LogTextRaw.Length; i++)
                    {
                        logText.Add(GetColoredCharacter(characters[i, loglineRow]));
                        logBackground.Add(GetBackgroundColor(characters[i, loglineRow]));
                    }
                    line.LogText = logText.ToArray();
                    line.LogBackground = logBackground.ToArray();
                    logText.Clear();
                    logBackground.Clear();
                }
                logLine.Clear();
                loglineRow++;
            }
            return loglines;
        }

        private static MonsterData[] ParseMonsterDisplay(Putty.TerminalCharacter[,] characters)
        {
            List<MonsterData> monsterDataList = new List<MonsterData>(4);
            StringBuilder monsterLine = new StringBuilder();
            List<string> monsterLineColored = new List<string>(AlmostFullWidth - GameViewWidth - 4);
            List<string> monsterLineBackground = new List<string>(AlmostFullWidth - GameViewWidth - 4);

            var lineOffset = 13;
            int currentChar = 0;
            for (int lineIndex = 0; lineIndex < 4; lineIndex++)
            {
                for (int i = GameViewWidth + 4; i < AlmostFullWidth; i++, currentChar++)
                {
                    monsterLine.Append(GetCharacter(characters[i, lineIndex + lineOffset]));
                    monsterLineColored.Add(GetColoredCharacter(characters[i, lineIndex + lineOffset]));
                    monsterLineBackground.Add(GetBackgroundColor(characters[i, lineIndex + lineOffset]));
                }
                monsterDataList.Add(FormatMonsterData(monsterLine.ToString(), monsterLineColored.ToArray(), monsterLineBackground.ToArray()));
                monsterLine.Clear();
                monsterLineColored.Clear();
                monsterLineBackground.Clear();
            }

            return monsterDataList.ToArray();
        }

        private static MonsterData FormatMonsterData(string monsterLine, string[] monsterLineColored, string[] monsterBackgroundColors)
        {
            if (monsterLine[0].Equals(' '))
            {
                return new MonsterData();
            }
            var chars = new char[] { ' ' };
            var split = monsterLine.ToString().Split(chars, count: 2);
            return new MonsterData() { 
                empty = false,
                MonsterTextRaw = split[1],
                MonsterDisplay = monsterLineColored.Take(split[0].Length).ToArray(),
                MonsterText = monsterLineColored.Skip(split[0].Length).ToArray(),
                MonsterBackground = monsterBackgroundColors
            };
        }

        private static Model parseMapLayout(Putty.TerminalCharacter[,] characters, string location)
        {
            Model model = new Model();
            model.Layout = LayoutType.MapOnly;
            model.LineLength = FullWidth;
            var coloredStrings = new string[GameViewWidth * FullHeight];
            var highlightColorStrings = new string[GameViewWidth * FullHeight];
            var curentChar = 0;
            try
            {

                for (int j = 0; j < FullHeight; j++)
                    for (int i = 0; i < GameViewWidth; i++)
                    {
                        coloredStrings[curentChar] = GetColoredCharacter(characters[i, j]);
                        highlightColorStrings[curentChar] = GetBackgroundColor(characters[i, j]);
                        curentChar++;
                    }

                model.TileNames = coloredStrings;
                model.HighlightColors = highlightColorStrings;

                model.SideData = new SideData();
                model.SideData.Place = location;

            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return model;
        }

        private static Model parseTextLayout(Putty.TerminalCharacter[,] characters)
        {
            Model model = new Model();
            model.Layout = LayoutType.TextOnly;
            model.LineLength = FullWidth;
            var coloredStrings = new string[FullWidth * FullHeight];
            var curentChar = 0;
            try
            {
                for (int j = 0; j < FullHeight; j++)
                    for (int i = 0; i < FullWidth; i++)
                    {
                        coloredStrings[curentChar] = GetColoredCharacter(characters[i, j]);
                        curentChar++;
                    }
                model.TileNames = coloredStrings;

            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return model;
        }


        private static SideData ParseSideData(Putty.TerminalCharacter[,] characters)
        {
            var sideData = new SideData();
            StringBuilder name = new StringBuilder();
            StringBuilder race = new StringBuilder();
            StringBuilder weapon = new StringBuilder();
            StringBuilder quiver = new StringBuilder();
            StringBuilder status = new StringBuilder();
            StringBuilder status2 = new StringBuilder();
            StringBuilder ac = new StringBuilder();
            StringBuilder ev = new StringBuilder();
            StringBuilder sh = new StringBuilder();
            StringBuilder xl = new StringBuilder();
            StringBuilder str = new StringBuilder();
            StringBuilder @int = new StringBuilder();
            StringBuilder dex = new StringBuilder();
            StringBuilder hp = new StringBuilder();
            StringBuilder mp = new StringBuilder();
            StringBuilder place = new StringBuilder();
            StringBuilder time = new StringBuilder();
            StringBuilder next = new StringBuilder();
            for (int i = 37; i < 75; i++)
            {
                name.Append(GetCharacter(characters[i, 0]));
                race.Append(GetCharacter(characters[i, 1]));
                weapon.Append(GetCharacter(characters[i, 9]));
                quiver.Append(GetCharacter(characters[i, 10]));
                status.Append(GetCharacter(characters[i, 11]));
                status2.Append(GetCharacter(characters[i, 12]));
            }
            for (int i = 40; i < 44; i++)
            {
                ac.Append(GetCharacter(characters[i, 4]));
                ev.Append(GetCharacter(characters[i, 5]));
                sh.Append(GetCharacter(characters[i, 6]));
                xl.Append(GetCharacter(characters[i, 7]));
                next.Append(GetCharacter(characters[i+10, 7]));
            }
            for (int i = 59; i < 63; i++)
            {
                str.Append(GetCharacter(characters[i, 4]));
                @int.Append(GetCharacter(characters[i, 5]));
                dex.Append(GetCharacter(characters[i, 6]));

            }
            for (int i = 37; i < 52; i++)
            {
                hp.Append(GetCharacter(characters[i + 1, 2]));
                mp.Append(GetCharacter(characters[i, 3]));

            }
            for (int i = 60; i < 75; i++)
            {
                place.Append(GetCharacter(characters[i + 1, 7]));
                time.Append(GetCharacter(characters[i, 8]));

            }

            var splithp = hp.ToString().Split(':').Length > 1 ? hp.ToString().Split(':')[1].Split('/') : new string[] { "1", "1" };
            var splitmp = mp.ToString().Split(':').Length > 1 ? mp.ToString().Split(':')[1].Split('/') : new string[] { "1", "1" };


            if (splithp.Length > 1)
            {
                sideData.Health = int.Parse(splithp[0]);
                var truehp = splithp[1].Split(' ');
                sideData.MaxHealth = int.Parse(truehp[0]);
                sideData.TrueHealth = truehp.Length > 1 ? truehp[1] : "";
            }
            if (splitmp.Length > 1)
            {
                sideData.Magic = int.Parse(splitmp[0]);
                var truemp = splitmp[1].Split(' ');
                sideData.MaxMagic = int.Parse(truemp[0]);
                sideData.TrueHealth = truemp.Length > 1 ? truemp[1] : "";

            }
            sideData.Name = name.ToString();
            sideData.Race = race.ToString();
            sideData.Weapon = weapon.ToString();
            sideData.Quiver = quiver.ToString();
            sideData.Statuses1 = status.ToString();
            sideData.Statuses2 = status2.ToString();
            sideData.ArmourClass = ac.ToString();
            sideData.Evasion = ev.ToString();
            sideData.Shield = sh.ToString();
            sideData.ExperienceLevel = xl.ToString();
            sideData.Strength = str.ToString();
            sideData.Inteligence = @int.ToString();
            sideData.Dexterity = dex.ToString();
            sideData.Place = place.ToString().Trim();
            sideData.Time = time.ToString();
            sideData.NextLevel = next.ToString();

            var parsed = sideData.Place.Split(':');
            bool found = false;
            foreach (var location in Locations.locations)
            {
                if (parsed[0].Contains(location.Substring(0, 3)))
                {
                    sideData.Place = location;
                    found = true;
                    break;
                }
            }
            if (found && parsed.Length>1)
            {
                sideData.Place += ":" + parsed[1];
            }
            return sideData;
        }
    }
}
