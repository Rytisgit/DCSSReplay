﻿using Putty;
using System;
using System.Text;
using InputParser.Abstract;
using InputParser.Constant;
using InputParser.Decorators;
using static InputParser.Constant.Helpers;

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
                if (sideLocation.Contains(".")) return LayoutType.TextOnly;
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
            if (chars == null) throw new ArgumentNullException("chars");

            Model DecorateWithTextAndHighlights()
            {
                var model = new HighLightDecorator(new TextDecorator(new BaseParser())).ParseData(chars);
                return model;
            }

            switch (GetLayoutType(chars, consoleFull, out var location))
            {
                case LayoutType.Normal:
                {
                    var model = new MonsterDataDecorator(new LogDataDecorator(new SideDataDecorator(new GameViewDecorator(new BaseParser())))).ParseData(chars);
                    model.Layout = LayoutType.Normal;
                    model.Location = location;
                    return model;
                }
                case LayoutType.TextOnly:
                {
                    var model = DecorateWithTextAndHighlights();
                    model.Layout = LayoutType.TextOnly;
                    return model;
                }
                    
                case LayoutType.MapOnly:
                {
                    var model = DecorateWithTextAndHighlights();
                    model.Layout = LayoutType.MapOnly;
                    model.Location = location;
                    return model;
                }
                case LayoutType.ConsoleFull:
                {
                    var model = DecorateWithTextAndHighlights();
                    model.Layout = LayoutType.ConsoleFull;
                    return model;
                }
                default:
                    return new Model();
            }
        }

    }
}
