using FrameGenerator.wasm.Models;
using System.Collections.Generic;
using SkiaSharp;
using System;
using System.Net;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;

namespace FrameGenerator.FileReading
{
    public static class ReadFromBase64
    {
        public static Dictionary<string, string> GetDictionaryFromFile(string path)
        {
            var dict = new Dictionary<string, string>();

            var textFromFile = Encoding.UTF8.GetString(Convert.FromBase64String(path));

            string[] lines = textFromFile.Split(
    new[] { "\r\n", "\r", "\n" },
    StringSplitOptions.None);
           
            for (var i = 0; i < lines.Length; i += 2)
            {
                dict[lines[i]] = lines[i + 1];
            }
            return dict;
        }

        public static Dictionary<string, string> GetMonsterData(string file, string monsterOverrideFile)
        {
            var monster = new Dictionary<string, string>();

            var textFromFile = Encoding.UTF8.GetString(Convert.FromBase64String(file));

            string[] lines = textFromFile.Split(
    new[] { "\r\n", "\r", "\n" },
    StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("  MONS_"))
                {
                    string[] tokens = lines[i].Split(',');
                    tokens[1] = tokens[1].Replace("'", "").Replace(" ", "");
                    tokens[2] = tokens[2].Replace(" ", "");
                    tokens[0] = tokens[0].Replace("MONS_", "").Replace(" ", "").ToLower();
                    //if(!Enum.TryParse(tokens[2], out ColorList2 res)) Console.WriteLine(tokens[1] + tokens[2] + " badly colored: " + tokens[0]);
                    if (monster.TryGetValue(tokens[1] + tokens[2], out var existing))
                    {
                        //Console.WriteLine(tokens[1] + tokens[2] + "exist: " + existing + " new: " + tokens[0]); 
                    }
                    else monster[tokens[1] + tokens[2]] = tokens[0];
                }
            }

            //Overrides for duplicates, others handled by name from monster log

            textFromFile = Encoding.UTF8.GetString(Convert.FromBase64String(monsterOverrideFile));
            lines = textFromFile.Split(
    new[] { "\r\n", "\r", "\n" },
    StringSplitOptions.None);

            foreach (var line in lines)
            {
                var keyValue = line.Split(' ');
                monster[keyValue[0]] = keyValue[1];
            }
            monster.Remove("8BLUE");//remove roxanne impersonating statue
            return monster;
        }


        public static List<NamedMonsterOverride> GetNamedMonsterOverrideData(string monsterOverrideFile)
        {
            var monster = new List<NamedMonsterOverride>();

            var textFromFile = Encoding.UTF8.GetString(Convert.FromBase64String(monsterOverrideFile));

            string[] lines = textFromFile.Split(
    new[] { "\r\n", "\r", "\n" },
    StringSplitOptions.None);

            var name = "";
            var location = "";
            var tileNameOverrides = new Dictionary<string, string>(20);

            bool pngParse = false;

            for (var i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    monster.Add(new NamedMonsterOverride(name, location, tileNameOverrides));
                    name = "";
                    location = "";
                    tileNameOverrides = new Dictionary<string, string>(20);
                    pngParse = false;
                    continue;
                }
                if (pngParse)
                {
                    string[] tokens = lines[i].Split(' ');
                    tileNameOverrides.Add(tokens[0], tokens[1]);
                }
                else
                {
                    string[] tokens = lines[i].Split(';');
                    name = tokens[0];
                    location = tokens.Length > 1 ? tokens[1] : "";
                    pngParse = true;
                }

            }

            return monster;
        }

        public static Dictionary<string, string[]> GetFloorAndWallNamesForDungeons(string file)
        {

            var floorandwall = new Dictionary<string, string[]>();
            var textFromFile = Encoding.UTF8.GetString(Convert.FromBase64String(file));

            string[] lines = textFromFile.Split(
new[] { "\r\n", "\r", "\n" },
StringSplitOptions.None);
            for (var i = 0; i < lines.Length; i += 3)
            {
                string[] temp = new string[2];
                temp[0] = lines[i + 1];
                temp[1] = lines[i + 2];
                floorandwall[lines[i].ToUpper()] = temp;
            }

            return floorandwall;
        }

        public static Dictionary<string, SKBitmap> GetSKBitmapDictionaryFromFolder(string folder)
        {
            var dict = new Dictionary<string, SKBitmap>();
            var st = new MemoryStream(Convert.FromBase64String(folder));
            SKBitmap SKBitmap;
            using (ZipFile archive = new ZipFile(st))
            {
                foreach (ZipEntry entry in archive)
                {
                    string name = entry.Name;
                    var zipEntryStream = archive.GetInputStream(entry);
                    using (MemoryStream s = new MemoryStream())
                    {
                        zipEntryStream.CopyTo(s);
                        byte[] arr = ToByteArray(s);

                        SKBitmap = SKBitmap.Decode(arr);
                        dict[name.Replace(".png", "")] = SKBitmap;
                    }


                }
                return dict;
            }
        }
        

        public static byte[] ToByteArray(MemoryStream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static Dictionary<string, SKBitmap> GetCharacterPNG(string gameLocation)
        {

            var dict = new Dictionary<string, SKBitmap>();

            var st = new MemoryStream(Convert.FromBase64String(gameLocation));
            SKBitmap SKBitmap;
            using (ZipFile archive = new ZipFile(st))
            {
                foreach (ZipEntry entry in archive)
                {
                    string name = entry.Name;
                    var zipEntryStream = archive.GetInputStream(entry);
                    using (MemoryStream s = new MemoryStream())
                    {
                        zipEntryStream.CopyTo(s);
                        byte[] arr = ToByteArray(s);

                        SKBitmap = SKBitmap.Decode(arr);
                        dict[name.Replace(".png", "")] = SKBitmap;
                    }


                }
                return dict;
            }
        }

        public static Dictionary<string, SKBitmap> GetMonsterPNG(string gameLocation)
        {


            var dict = new Dictionary<string, SKBitmap>();
            var st = new MemoryStream(Convert.FromBase64String(gameLocation));
            SKBitmap SKBitmap;
            using (ZipFile archive = new ZipFile(st))
            {
                foreach (ZipEntry entry in archive)
                {
                    string name = entry.Name;
                    var zipEntryStream = archive.GetInputStream(entry);
                    using (MemoryStream s = new MemoryStream())
                    {
                        zipEntryStream.CopyTo(s);
                        byte[] arr = ToByteArray(s);

                        SKBitmap = SKBitmap.Decode(arr);
                        dict[name.Replace(".png", "")] = SKBitmap;
                    }
                }
                return dict;
            }
        }
        public static Dictionary<string, SKBitmap> GetWeaponPNG(string gameLocation)
        {


            var dict = new Dictionary<string, SKBitmap>();
            var st = new MemoryStream(Convert.FromBase64String(gameLocation));
            SKBitmap SKBitmap;
            using (ZipFile archive = new ZipFile(st))
            {
                foreach (ZipEntry entry in archive)
                {
                    string name = entry.Name;
                    var zipEntryStream = archive.GetInputStream(entry);
                    using (MemoryStream s = new MemoryStream())
                    {
                        zipEntryStream.CopyTo(s);
                        byte[] arr = ToByteArray(s);

                        SKBitmap = SKBitmap.Decode(arr);
                        dict[name.Replace(".png", "")] = SKBitmap;
                    }
                       
                }
                return dict;
            }
        }
    }

}
