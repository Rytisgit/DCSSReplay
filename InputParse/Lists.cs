using System;
using System.Collections.Generic;
using SkiaSharp;

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
    public enum ColorListEnum
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
   "Ziggurat", "Bazaar","Trove","Sewer","Ossuary","Bailey","Ice Cave","Volcano","Wizlab","Depths","Desolation of Salt", "a Labyrinth", "a Gauntlet"};
    }
    public class ColorList
    {
        public SKColor BLACK = new SKColor(0, 0, 0);
        public SKColor RED = new SKColor(255, 0, 0);
        public SKColor GREEN = new SKColor(0, 255, 0);
        public SKColor BROWN = new SKColor(165, 91, 0);
        public SKColor BLUE = new SKColor(0, 64, 255);
        public SKColor MAGENTA = new SKColor(192, 0, 255);
        public SKColor CYAN = new SKColor(0, 255, 255);
        public SKColor LIGHTGREY = new SKColor(192, 192, 192);
        public SKColor DARKGREY = new SKColor(128, 128, 128);
        public SKColor LIGHTRED = new SKColor(255, 128, 128);
        public SKColor LIGHTGREEN = new SKColor(128, 255, 128);
        public SKColor YELLOW = new SKColor(255, 255, 0);
        public SKColor LIGHTBLUE = new SKColor(128, 128, 255);
        public SKColor LIGHTMAGENTA = new SKColor(255, 128, 255);
        public SKColor LIGHTCYAN = new SKColor(64, 255, 255);
        public SKColor WHITE = new SKColor(255, 255, 255);
        public SKColor WHITE2 = new SKColor(255, 255, 255);

        public static SKColor GetColor(string name)
        {
            return name switch
            {
                "BLACK" => new SKColor(0, 0, 0),
                "RED" => new SKColor(255, 0, 0),
                "GREEN" => new SKColor(0, 255, 0),
                "BROWN" => new SKColor(165, 91, 0),
                "BLUE" => new SKColor(0, 64, 255),
                "MAGENTA" => new SKColor(192, 0, 255),
                "CYAN" => new SKColor(0, 255, 255),
                "LIGHTGREY" => new SKColor(192, 192, 192),
                "DARKGREY" => new SKColor(128, 128, 128),
                "LIGHTRED" => new SKColor(255, 128, 128),
                "LIGHTGREEN" => new SKColor(128, 255, 128),
                "YELLOW" => new SKColor(255, 255, 0),
                "LIGHTBLUE" => new SKColor(128, 128, 255),
                "LIGHTMAGENTA" => new SKColor(255, 128, 255),
                "LIGHTCYAN" => new SKColor(64, 255, 255),
                "WHITE" => new SKColor(255, 255, 255),
                "WHITE2" => new SKColor(255, 255, 255),
                null => new SKColor(0, 0, 0),
                "" => new SKColor(0, 0, 0),
                _ => throw new ArgumentException($"No Color with name: {name} is defined"),
                
            };
        }
    }
}
