using System;
using System.Collections.Generic;
using System.Text;
using Putty;

namespace InputParser
{
    public static class Helpers
    {
        public const int FullWidth = 80;
        public const int AlmostFullWidth = 75;
        public const int FullHeight = 24;
        public const int GameViewWidth = 33;
        public const int GameViewHeight = 17;

        public static char GetCharacter(TerminalCharacter character) => character.Character == 55328 ? ' ' : character.Character;
        public static string GetColoredCharacter(TerminalCharacter character) => GetCharacter(character) + Enum.GetName(typeof(ColorListEnum), character.ForegroundPaletteIndex);
        public static string GetBackgroundColor(TerminalCharacter character) => Enum.GetName(typeof(ColorListEnum), character.BackgroundPaletteIndex);
    }
}
