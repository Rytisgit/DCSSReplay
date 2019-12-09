using FrameGenerator.FileReading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Drawing;
using FrameGenerator.FrameCreation;
using System.Threading.Tasks;
using Window;
using System.Linq;
using Putty;

namespace FrameGenerator
{
    public class MainGenerator
    {
        Widow_Display display = new Widow_Display();
        private Dictionary<string, string> _monsterdata;
        private Dictionary<string, string> _characterdata;
        private Dictionary<string, string[]> _floorandwall;
        private Dictionary<string, string> moretiles;
        private Dictionary<string, Bitmap> _monsterpng;
        private Dictionary<string, Bitmap> _characterpng;
        private Dictionary<string, string> itemdata;
        private Dictionary<string, Bitmap> itempng;
        private Dictionary<string, Bitmap> alldngnpng;


        public MainGenerator()
        {
            string GameLocation = File.ReadAllLines(display.Folder + @"\config.ini").First();
            _characterdata = ReadFromFile.CharacterData();
            moretiles = ReadFromFile.AdditionalTileData();
            _characterpng = ReadFromFile.GetCharacterPNG(GameLocation);
            _monsterdata = ReadFromFile.GetMonsterData(GameLocation);
            _floorandwall = ReadFromFile.Get_Floor_And_Wall_Names_For_Dungeons();
            _monsterpng = ReadFromFile.GetMonsterPNG(GameLocation);
            itempng = ReadFromFile.ItemsPNG(GameLocation);
            itemdata = ReadFromFile.ItemData();
            alldngnpng = ReadFromFile.GetAllDungeonPNG(GameLocation);
        }

        public void GenerateImage(TerminalCharacter[,] chars)
        {
            if (chars != null) { CreatingFrame.DrawFrame(itempng, itemdata, moretiles, alldngnpng, _monsterdata, _monsterpng, _floorandwall, _characterdata, _characterpng, display, chars); }
            return;
        }

  

    }

   

    
}



