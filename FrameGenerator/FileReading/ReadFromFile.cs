using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FrameGenerator.FileReading
{
    public static class ReadFromFile
    {
        public static Dictionary<string, string> GetDictionaryFromFile(string path)
        {
            var dict = new Dictionary<string, string>();

            string[] lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i += 2)
            {
                dict[lines[i]] = lines[i + 1];
            }
            return dict;
        }

        public static Dictionary<string, string> GetMonsterData(string file)

        {
            var monster = new Dictionary<string, string>();

            string[] lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i++)
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

            //Fun Overrides for duplicates

            var names = new List<string> {"DYELLOW" };

            var png = new List<string> { "golden_dragon" };

            for (int i = 0; i < names.Count; i++)
            {
                monster[names.ElementAt(i)] = png.ElementAt(i);
            }

            return monster;
        }

        public static Dictionary<string, string[]> GetFloorAndWallNamesForDungeons(string file)

        {

            var floorandwall = new Dictionary<string, string[]>();
            string[] lines = File.ReadAllLines(file);

            for (var i = 0; i < lines.Length; i += 3)
            {
                string[] temp = new string[2];
                temp[0] = lines[i + 1];
                temp[1] = lines[i + 2];
                floorandwall[lines[i].ToUpper()] = temp;
            }

            return floorandwall;
        }
        
        public static Dictionary<string, Bitmap> GetBitmapDictionaryFromFolder(string folder)
        {
            var dict = new Dictionary<string, Bitmap>();
            string[] pngFiles = Directory.GetFiles(folder, "*.png*", SearchOption.AllDirectories);
            foreach (var file in pngFiles)
            {
                FileInfo info = new FileInfo(file);
                Bitmap bitmap = new Bitmap(file);
                dict[info.Name.Replace(".png", "")] = bitmap;
            }
            return dict;
        }
    
        public static Dictionary<string, Bitmap> GetCharacterPNG(string gameLocation)
        {

            var GetCharacterPNG = new Dictionary<string, Bitmap>();

            List<string> allpngfiles = Directory.GetFiles(gameLocation + @"\rltiles\player\base", "*.png*", SearchOption.AllDirectories).ToList();
            allpngfiles.AddRange(Directory.GetFiles(gameLocation + @"\rltiles\player\felids", "*.png*", SearchOption.AllDirectories).ToList());
            foreach (var file in allpngfiles)
            {
                FileInfo info = new FileInfo(file);
                Bitmap bitmap = new Bitmap(file);


                GetCharacterPNG[info.Name.Replace(".png", "")] = bitmap;

            }
            return GetCharacterPNG;
        }

        public static Dictionary<string, Bitmap> GetMonsterPNG(string gameLocation)
        {

            var monsterPNG = new Dictionary<string, Bitmap>();
            string[] allpngfiles = Directory.GetFiles(gameLocation + @"\rltiles\mon", "*.png*", SearchOption.AllDirectories);
            foreach (var file in allpngfiles)
            {
                FileInfo info = new FileInfo(file);
                Bitmap bitmap = new Bitmap(file);
                monsterPNG[info.Name.Replace(".png", "")] = bitmap;

            }
            Bitmap bmp = new Bitmap(gameLocation + @"\rltiles\dngn\statues\statue_triangle.png");
            monsterPNG["roxanne"] = bmp;
            return monsterPNG;
        }

        public static Dictionary<string, Bitmap> ItemsPNG(string gameLocation)

        {

            var wallpng = new Dictionary<string, Bitmap>();

            List<string> wallpngfiles = Directory.GetFiles(gameLocation + @"\rltiles\item", "*.png*", SearchOption.AllDirectories).ToList();
            wallpngfiles.AddRange(Directory.GetFiles(@"..\..\..\Extra", "*.png*", SearchOption.AllDirectories).ToList());
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
