using FrameGenerator.FileReading;
using FrameGenerator.FrameCreation;
using Putty;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Window;

namespace FrameGenerator
{
    public class MainGenerator
    {
        Widow_Display display = new Widow_Display();
        private Dictionary<string, string> _monsterdata;
        private Dictionary<string, string> _characterdata;
        private Dictionary<string, string[]> _floorandwall;
        private Dictionary<string, string> _moretiles;
        private Dictionary<string, string> _cloudtiles;
        private Dictionary<string, Bitmap> _monsterpng;
        private Dictionary<string, Bitmap> _characterpng;
        private Dictionary<string, string> _itemdata;
        private Dictionary<string, Bitmap> _itempng;
        private Dictionary<string, Bitmap> _alldngnpng;
        private Dictionary<string, Bitmap> _alleffects;
        private Dictionary<string, Bitmap> _floorpng;
        private Dictionary<string, Bitmap> _wallpng;
        private Bitmap _lastframe= new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);
        private int previousHealth = 0;


        public MainGenerator()
        {
         
            string GameLocation = File.ReadAllLines(display.Folder + @"\config.ini").First();
            _characterdata = ReadFromFile.CharacterData();
            _moretiles = ReadFromFile.AdditionalTileData();
            _characterpng = ReadFromFile.GetCharacterPNG(GameLocation);
            _monsterdata = ReadFromFile.GetMonsterData(GameLocation);
            _floorandwall = ReadFromFile.Get_Floor_And_Wall_Names_For_Dungeons();
            _monsterpng = ReadFromFile.GetMonsterPNG(GameLocation);
            _itempng = ReadFromFile.ItemsPNG(GameLocation);
            _itemdata = ReadFromFile.ItemData();
            _floorpng = ReadFromFile.GetFloorPNG(GameLocation);
            _wallpng = ReadFromFile.GetWallPNG(GameLocation);
            _alldngnpng = ReadFromFile.GetAllDungeonPNG(GameLocation);
            _alleffects = ReadFromFile.GetAllEffectPNG(GameLocation);
            _cloudtiles = ReadFromFile.GetCloudData();
        }

        public void GenerateImage(TerminalCharacter[,] chars)
        {
            if (chars != null) { 
                var image = CreatingFrame.DrawFrame(ref _lastframe, ref previousHealth, _wallpng, _floorpng, _itempng, _itemdata, _moretiles, _cloudtiles, _alldngnpng, _monsterdata, _monsterpng, _floorandwall, _characterdata, _characterpng, chars, _alleffects);

                display.Update_Window_Image(image);
                GC.Collect();
            }
            return;
        }



    }




}



