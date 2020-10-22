using Putty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InputParser.Decorators;
using static InputParser.Helpers;

namespace InputParser
{
    public static class Parser
    {
        private static LayoutType GetLayoutType(TerminalCharacter[,] characters, bool consoleFull, out string newlocation)
        {
            StringBuilder place = new StringBuilder();
            bool found = false;

            newlocation = "";
            if (consoleFull) return LayoutType.ConsoleFull;
            for (int i = 61; i < FullWidth; i++)
            {
                place.Append(GetCharacter(characters[i, 7]));
            }
            var sideLocation = place.ToString();
            foreach (var location in Locations.locations)
            {
                if (!sideLocation.Contains(location.Substring(0, 3))) continue;
                newlocation = location;
                return LayoutType.Normal;
            }

            place = new StringBuilder();
            for (var i = 0; i < FullWidth; i++)
            {
                place.Append(GetCharacter(characters[i, 0]));
            }
            if (!place.ToString().Contains("Press ?")) return LayoutType.TextOnly;

            var mapLocation = place.ToString().Substring(0, 30);
            foreach (var location in Locations.locations)
            {
                if (!mapLocation.Contains(location.Substring(0, 3))) continue;
                newlocation = location;
                return LayoutType.MapOnly;
            }
            return LayoutType.TextOnly;
        }

        public static Model ParseData(TerminalCharacter[,] chars, bool consoleFull = false)
        {
            if (chars == null) throw new ArgumentNullException("TerminalCharacter array is null");

            var model = new Model();
            switch ((GetLayoutType(chars, consoleFull, out var location)))
            {
                case LayoutType.Normal:
                {
                    model = new GameViewParser(model).ParseData(chars);
                    model = new SideDataDecorator(model).ParseData(chars);
                    model = new LogDataDecorator(model).ParseData(chars);
                    model = new MonsterDataDecorator(model).ParseData(chars);
                    model.Layout = LayoutType.Normal;
                    model.Location = location;
                    return model;
                }
                case LayoutType.TextOnly:
                {
                    model = new TextDecorator(model).ParseData(chars);
                    model = new HighLightDecorator(model).ParseData(chars);
                    model.Layout = LayoutType.TextOnly;
                    return model;
                }
                    
                case LayoutType.MapOnly:
                {
                    model = new TextDecorator(model).ParseData(chars);
                    model = new HighLightDecorator(model).ParseData(chars);
                    model.Layout = LayoutType.MapOnly;
                    model.Location = location;
                    return model;
                }
                case LayoutType.ConsoleFull:
                {
                    model = new TextDecorator(model).ParseData(chars);
                    model = new HighLightDecorator(model).ParseData(chars);
                    model.Layout = LayoutType.ConsoleFull;
                    return model;
                }
                default:
                    return new Model();
            }
        }

    }
}
