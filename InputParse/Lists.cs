using System;
using System.Collections.Generic;
using System.Drawing;

namespace InputParser
{
    public enum LayoutType
    {
        Normal,
        TextOnly,
        MapOnly,
        ConsoleSwitch,
        ConsoleFull
    }
    public enum ColorList2
    {
        BLACK,//0 (black if sleeping? color is background for monster list?)
        RED,//1
        GREEN,//2
        BROWN,//3 (wandering)(can hide yellow and brown)
        BLUE,//4
        MAGENTA,//5
        CYAN, //6
        LIGHTGREY,//7 if nothing nothing its black, otherwise default is 7 for lightgrey
        DARKGREY, //8
        LIGHTRED,//9
        LIGHTGREEN,//10
        YELLOW,//11
        LIGHTBLUE,//12
        LIGHTMAGENTA,//13
        LIGHTCYAN,//14
        WHITE,//15
        WHITE2 = 256
    }

    public static class Locations
    {
        public static List<string> locations = new List<string> { "Dungeon","Temple","Orcish Mines","Elven Halls","Lair","Swamp","Shoals","Snake Pit",
    "Spider Nest","Slime Pits","Vaults","Hall of Blades","Crypt","Tomb","Hell","Dis","Gehenna","Cocytus","Tartarus","Zot","Abyss","Pandemonium",
   "Ziggurat", "Bazaar","Trove","Sewer","Ossuary","Bailey","Ice Cave","Volcano","Wizlab","Depths","Desolation of Salt"};
    }
    public class ColorList
    {
        public SolidBrush BLACK = new SolidBrush(Color.FromArgb(0, 0, 0));
        public SolidBrush RED = new SolidBrush(Color.FromArgb(255, 0, 0));
        public SolidBrush GREEN = new SolidBrush(Color.FromArgb(0, 255, 0));
        public SolidBrush BROWN = new SolidBrush(Color.FromArgb(165, 91, 0));
        public SolidBrush BLUE = new SolidBrush(Color.FromArgb(0, 64, 255));
        public SolidBrush MAGENTA = new SolidBrush(Color.FromArgb(192, 0, 255));
        public SolidBrush CYAN = new SolidBrush(Color.FromArgb(0, 255, 255));
        public SolidBrush LIGHTGREY = new SolidBrush(Color.FromArgb(192, 192, 192));
        public SolidBrush DARKGREY = new SolidBrush(Color.FromArgb(128, 128, 128));
        public SolidBrush LIGHTRED = new SolidBrush(Color.FromArgb(255, 128, 128));
        public SolidBrush LIGHTGREEN = new SolidBrush(Color.FromArgb(128, 255, 128));
        public SolidBrush YELLOW = new SolidBrush(Color.FromArgb(255, 255, 0));
        public SolidBrush LIGHTBLUE = new SolidBrush(Color.FromArgb(128, 128, 255));
        public SolidBrush LIGHTMAGENTA = new SolidBrush(Color.FromArgb(255, 128, 255));
        public SolidBrush LIGHTCYAN = new SolidBrush(Color.FromArgb(64, 255, 255));
        public SolidBrush WHITE = new SolidBrush(Color.FromArgb(255, 255, 255));
        public SolidBrush WHITE2 = new SolidBrush(Color.FromArgb(255, 255, 255));

        public static Color GetColor(string name)
        {
            return name switch
            {
                "BLACK" => Color.FromArgb(0, 0, 0),
                "RED" => Color.FromArgb(255, 0, 0),
                "GREEN" => Color.FromArgb(0, 255, 0),
                "BROWN" => Color.FromArgb(165, 91, 0),
                "BLUE" => Color.FromArgb(0, 64, 255),
                "MAGENTA" => Color.FromArgb(192, 0, 255),
                "CYAN" => Color.FromArgb(0, 255, 255),
                "LIGHTGREY" => Color.FromArgb(192, 192, 192),
                "DARKGREY" => Color.FromArgb(128, 128, 128),
                "LIGHTRED" => Color.FromArgb(255, 128, 128),
                "LIGHTGREEN" => Color.FromArgb(128, 255, 128),
                "YELLOW" => Color.FromArgb(255, 255, 0),
                "LIGHTBLUE" => Color.FromArgb(128, 128, 255),
                "LIGHTMAGENTA" => Color.FromArgb(255, 128, 255),
                "LIGHTCYAN" => Color.FromArgb(64, 255, 255),
                "WHITE" => Color.FromArgb(255, 255, 255),
                "WHITE2" => Color.FromArgb(255, 255, 255),
                null => Color.FromArgb(0, 0, 0),
                "" => Color.FromArgb(0, 0, 0),
                _ => throw new ArgumentException($"No Color with name: {name} is defined"),
                
            };
        }
    }
}
