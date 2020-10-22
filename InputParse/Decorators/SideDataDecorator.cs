using System;
using System.Text;
using InputParser.Abstract;
using InputParser.Constant;
using Putty;
using static InputParser.Constant.Helpers;

namespace InputParser.Decorators
{
    class SideDataDecorator : AbstractDecorator
    {
        public SideDataDecorator(Model model) : base(model) { }

        public override Model ParseData(TerminalCharacter[,] characters)
        {
            base.model.ParseData(characters);
            if (!(base.model is Model)) return new Model();
            var model = (Model)base.model;
            model.SideData = ParseSideData(characters);
            model.SideDataColored = ParseColoredSideData(characters);
            return model;
        }

        private static SideDataColored ParseColoredSideData(TerminalCharacter[,] characters)
        {
            var coloredSideData = new SideDataColored();
            for (int i = 37; i < 75; i++)
            {
                coloredSideData.Statuses1.Add(GetColoredCharacter(characters[i, 11]));
                coloredSideData.Statuses1.Add(GetColoredCharacter(characters[i, 12]));
            }
            return coloredSideData;
        }

        private static SideData ParseSideData(TerminalCharacter[,] characters)
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
            StringBuilder noiseOrGold = new StringBuilder();
            StringBuilder time = new StringBuilder();
            StringBuilder next = new StringBuilder();

            for (int i = 37; i < 75; i++)
            {
                name.Append(GetCharacter(characters[i, 0]));
                race.Append(GetCharacter(characters[i, 1]));
                status.Append(GetCharacter(characters[i, 11]));
                status2.Append(GetCharacter(characters[i, 12]));
            }
            for (int i = 37; i < 80; i++)
            {
                weapon.Append(GetCharacter(characters[i, 9]));
                quiver.Append(GetCharacter(characters[i, 10]));
            }
            for (int i = 40; i < 44; i++)
            {
                ac.Append(GetCharacter(characters[i, 4]));
                ev.Append(GetCharacter(characters[i, 5]));
                sh.Append(GetCharacter(characters[i, 6]));
                xl.Append(GetCharacter(characters[i, 7]));
                next.Append(GetCharacter(characters[i + 10, 7]));
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
                noiseOrGold.Append(GetCharacter(characters[i, 8]));

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

            var split = noiseOrGold.ToString().Split(':');
            sideData.NoisyGold = split.Length > 1 ? split[1] : "noise here";
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
            if (found && parsed.Length > 1)
            {
                sideData.Place += ":" + parsed[1];
            }

            return sideData;
        }
    }
}
