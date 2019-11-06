using System;
using System.Text;
using TtyRecMonkey;

namespace InputParse
{
    public class Parser
    {
        private static char GetCharacter(Putty.TerminalCharacter character)
        {
            return character.Character == 55328 ? ' ' : character.Character;
        }
        const int GameViewWidth = 33;
        const int GameViewHeight = 17;
        public static Model ParseData(Putty.TerminalCharacter[,] chars)
        {
            var mod = new Model();
            mod.LineLength = GameViewWidth;
            return mod;
            var characters = chars;
            if (characters == null) return null;
            Model model = new Model();
            model.LineLength = GameViewWidth;
            var coloredStrings = new string[GameViewWidth * GameViewHeight];
            var highlightColorStrings = new string[GameViewWidth * GameViewHeight];
            var curentChar = 0;
            for (int j =0;j< GameViewHeight; j++)
                for (int i = 0; i < GameViewWidth; i++) 
                {
                    coloredStrings[curentChar] = GetCharacter(characters[i, j]) + Enum.GetName(typeof(ColorList2), characters[i, j].ForegroundPaletteIndex);
                    highlightColorStrings[curentChar] = Enum.GetName(typeof(ColorList2), characters[i, j].BackgroundPaletteIndex);
                    curentChar++;
                }
            model.TileNames = coloredStrings;
            Console.WriteLine(characters.Length);
            StringBuilder name = new StringBuilder();
            StringBuilder race = new StringBuilder();
            StringBuilder weapon = new StringBuilder();
            StringBuilder quiver = new StringBuilder();
            StringBuilder status = new StringBuilder();
            for (int i = 37; i < 75; i++)
            {
                name.Append(GetCharacter(characters[i, 0]));
                race.Append(GetCharacter(characters[i, 1]));
                weapon.Append(GetCharacter(characters[i, 9]));
                quiver.Append(GetCharacter(characters[i, 10]));
                status.Append(GetCharacter(characters[i, 11]));

            }
            model.SideData.Name = name.ToString();
            model.SideData.Race = race.ToString();
            model.SideData.Weapon = weapon.ToString();
            model.SideData.Quiver = quiver.ToString();
            model.SideData.Statuses1 = status.ToString();
            model.SideData.Statuses2 = "";

            StringBuilder ac = new StringBuilder();
            StringBuilder ev = new StringBuilder();
            StringBuilder sh = new StringBuilder();
            StringBuilder xl = new StringBuilder();
            for (int i = 40; i < 44; i++)
            {
                ac.Append(GetCharacter(characters[i, 4]));
                ev.Append(GetCharacter(characters[i, 5]));
                sh.Append(GetCharacter(characters[i, 6]));
                xl.Append(GetCharacter(characters[i, 7]));

            }
            model.SideData.ArmourClass = ac.ToString();
            model.SideData.Evasion = ev.ToString();
            model.SideData.Shield = sh.ToString();
            model.SideData.ExperienceLevel = xl.ToString();

            StringBuilder str = new StringBuilder();
            StringBuilder @int = new StringBuilder();
            StringBuilder dex = new StringBuilder();
            for (int i = 59; i < 63; i++)
            {
                str.Append(GetCharacter(characters[i, 4]));
                @int.Append(GetCharacter(characters[i, 5]));
                dex.Append(GetCharacter(characters[i, 6]));

            }
            model.SideData.Strength = str.ToString();
            model.SideData.Inteligence = @int.ToString();
            model.SideData.Dexterity = dex.ToString();

            StringBuilder hp = new StringBuilder();
            StringBuilder mp = new StringBuilder();
            for (int i = 43; i < 52; i++)
            {
                hp.Append(GetCharacter(characters[i + 1, 2]));
                mp.Append(GetCharacter(characters[i, 3]));

            }
            var splithp = hp.ToString().Split('/');
            var splitmp = mp.ToString().Split('/');
            if (splithp.Length > 1)
            {
                model.SideData.Magic = int.Parse(splitmp[0]);
                model.SideData.Health = int.Parse(splithp[0]);
                model.SideData.MaxMagic = int.Parse(splitmp[1]);
                model.SideData.MaxHealth = int.Parse(splithp[1]);
            }
            StringBuilder logLine1 = new StringBuilder();
            StringBuilder logLine2 = new StringBuilder();
            StringBuilder logLine3 = new StringBuilder();
            StringBuilder logLine4 = new StringBuilder();
            StringBuilder logLine5 = new StringBuilder();
            StringBuilder logLine6 = new StringBuilder();
            for (int i = 0; i < 75; i++)
            {
                logLine1.Append(GetCharacter(characters[i, 17]));
                logLine2.Append(GetCharacter(characters[i, 18]));
                logLine3.Append(GetCharacter(characters[i, 19]));
                logLine4.Append(GetCharacter(characters[i, 20]));
                logLine5.Append(GetCharacter(characters[i, 21]));
                logLine6.Append(GetCharacter(characters[i, 22]));
            }

            model.FullLengthStrings = new string[6] { logLine1.ToString(), logLine2.ToString(), logLine3.ToString(), logLine4.ToString(), logLine5.ToString(), logLine6.ToString() };
            model.FullLengthStringColors = new string[6] { Enum.GetName(typeof(ColorList2), characters[1, 18].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2), characters[1, 19].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2), characters[1, 20].ForegroundPaletteIndex),
                Enum.GetName(typeof(ColorList2),characters[1, 21].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2),characters[1, 22].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2),characters[1, 23].ForegroundPaletteIndex), };
            //foreach (var item in coloredStrings)
            //{
            //    Console.Write('"' + item + "\", ");
            //}
            //Console.WriteLine();
            //model.HighlightColors = highlightColorStrings;
            //foreach (var item in highlightColorStrings)
            //{
            //    Console.Write('"' + item + "\", ");
            //}
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine();
            return model;
        }
    }
}
