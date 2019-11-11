using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FrameGenerator.FileReading
{
    class ReadFromFile
    {
         public static Dictionary<string, string> GetMonsterData(string gameLocation)

            {
            var monster = new Dictionary<string, string>();

            string[] lines = System.IO.File.ReadAllLines(gameLocation + @"\source\mon-data.h");
       
            for (var i = 0; i<lines.Length; i++)
            {
                if (lines[i].Contains("  MONS_"))
                {
                    string[] tokens = lines[i].Split(',');
        tokens[1] = tokens[1].Replace("'", "").Replace(" ", "");
        tokens[2] = tokens[2].Replace(" ", "");
        tokens[0] = tokens[0].Replace("MONS_", "").Replace(" ", "").ToLower();
        monster[tokens[1] + tokens[2]] = tokens[0];
                }
             }
            return monster;
            }
        public static Dictionary<string, string[]> Get_Floor_And_Wall_Names_For_Dungeons()

        {

            var floorandwall = new Dictionary<string, string[]>();
            string[] lines = System.IO.File.ReadAllLines(@"..\..\..\..\FrameGenerator\Extra\tilefloor.txt");
                
                for (var i = 0; i < lines.Length; i += 3)
                {
                string[] temp = new string[2];
                temp[0] = lines[i + 1];
                temp[1] = lines[i + 2];
                floorandwall[lines[i]] = temp;
                }
            
            return floorandwall;
        }

        public static Dictionary<string, Bitmap> GetMonsterPNG(string gameLocation)

        {

            var monsterPNG = new Dictionary<string, Bitmap>();
            string[] allpngfiles = Directory.GetFiles(gameLocation + @"\source\rltiles\mon", "*.png*", SearchOption.AllDirectories);
            foreach (var file in allpngfiles)
            {
                FileInfo info = new FileInfo(file);
                Bitmap bitmap = new Bitmap(file);
                monsterPNG[info.Name.Replace(".png", "")] = bitmap;

            }
            return monsterPNG;
        }

        public static Dictionary<string, Bitmap> GetFloorPNG(string gameLocation)

        {

            var floorpng = new Dictionary<string, Bitmap>();
            string[] floorpngfiles = Directory.GetFiles(gameLocation + @"\source\rltiles\dngn\floor", "*.png*", SearchOption.AllDirectories);
            foreach (var file in floorpngfiles)
            {
                FileInfo info = new FileInfo(file);
                Bitmap bitmap = new Bitmap(file);
                floorpng[info.Name.Replace(".png", "")] = bitmap;
            }
            return floorpng;
        }

        public static Dictionary<string, Bitmap> GetWallPNG(string gameLocation)

        {

            var wallpng = new Dictionary<string, Bitmap>();
            string[] wallpngfiles = Directory.GetFiles(gameLocation + @"\source\rltiles\dngn\wall", "*.png*", SearchOption.AllDirectories);
            foreach (var file in wallpngfiles)
            {
                FileInfo info = new FileInfo(file);
                Bitmap bitmap = new Bitmap(file);
                wallpng[info.Name.Replace(".png", "")] = bitmap;
            }
            return wallpng;
        }     
    }
}
