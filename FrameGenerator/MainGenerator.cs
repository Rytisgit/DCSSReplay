using FrameGenerator.FileReading;
using FrameGenerator.FrameCreation;
using Putty;
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
        private Dictionary<string, string> moretiles;
        private Dictionary<string, Bitmap> _monsterpng;
        private Dictionary<string, Bitmap> _characterpng;
        private Dictionary<string, string> itemdata;
        private Dictionary<string, Bitmap> itempng;
        private Dictionary<string, Bitmap> alldngnpng;
        private Bitmap lastframe= new Bitmap(1602, 1050, PixelFormat.Format32bppArgb);


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
            if (chars != null) { CreatingFrame.DrawFrame(ref lastframe, itempng, itemdata, moretiles, alldngnpng, _monsterdata, _monsterpng, _floorandwall, _characterdata, _characterpng, display, chars); }

            return;
        }



    }




}



