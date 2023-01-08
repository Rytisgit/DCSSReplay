using FrameGenerator.Models;
using System;
using System.Collections.Generic;
using SkiaSharp;
using System.IO;
using System.Threading.Tasks;

namespace FrameGenerator.FileReading
{
    public class VersionSelectableDataHolder
    {
        private Dictionary<string, string> _monsterdata = new();
        private List<NamedMonsterOverride> _namedMonsterOverrideData = new();
        private Dictionary<string, string> _characterdata = new();
        private Dictionary<string, string[]> _floorandwall = new();
        private Dictionary<string, Tuple<List<string>, List<string>>> _floorandwallColor = new();
        private Dictionary<string, string> _features = new();
        private Dictionary<string, string> _cloudtiles = new();
        private Dictionary<string, string> _itemdata = new();
        private Dictionary<string, string> _weapondata = new();
        private Dictionary<string, SKBitmap> _monsterpng = new();
        private Dictionary<string, SKBitmap> _characterpng = new();
        private Dictionary<string, SKBitmap> _weaponpng = new();
        private Dictionary<string, SKBitmap> _itempng = new();
        private Dictionary<string, SKBitmap> _alldngnpng = new();
        private Dictionary<string, SKBitmap> _alleffects = new();
        private Dictionary<string, SKBitmap> _miscallaneous = new();
        private Dictionary<string, SKBitmap> _floorpng = new();
        private Dictionary<string, SKBitmap> _wallpng = new();

        private Dictionary<string, string> _monsterdata2023 = new();
        private List<NamedMonsterOverride> _namedMonsterOverrideData2023 = new();
        private Dictionary<string, string> _characterdata2023 = new();
        private Dictionary<string, string[]> _floorandwall2023 = new();
        private Dictionary<string, Tuple<List<string>, List<string>>> _floorandwallColor2023 = new();
        private Dictionary<string, string> _features2023 = new();
        private Dictionary<string, string> _cloudtiles2023 = new();
        private Dictionary<string, string> _itemdata2023 = new();
        private Dictionary<string, string> _weapondata2023 = new();
        private Dictionary<string, SKBitmap> _monsterpng2023 = new();
        private Dictionary<string, SKBitmap> _characterpng2023 = new();
        private Dictionary<string, SKBitmap> _weaponpng2023 = new();
        private Dictionary<string, SKBitmap> _itempng2023 = new();
        private Dictionary<string, SKBitmap> _alldngnpng2023 = new();
        private Dictionary<string, SKBitmap> _alleffects2023 = new();
        private Dictionary<string, SKBitmap> _miscallaneous2023 = new();
        private Dictionary<string, SKBitmap> _floorpng2023 = new();
        private Dictionary<string, SKBitmap> _wallpng2023 = new();

        public Dictionary<string, string> Monsterdata { get => version == "Classic" ? _monsterdata : _monsterdata2023; }
        public List<NamedMonsterOverride> NamedMonsterOverrideData { get => version == "Classic" ? _namedMonsterOverrideData : _namedMonsterOverrideData2023; }
        public Dictionary<string, string> Characterdata { get => version == "Classic" ? _characterdata : _characterdata2023; }
        public Dictionary<string, string[]> Floorandwall { get => version == "Classic" ? _floorandwall : _floorandwall2023; }
        public Dictionary<string, Tuple<List<string>, List<string>>> FloorandwallColor { get => version == "Classic" ? _floorandwallColor : _floorandwallColor2023; }
        public Dictionary<string, string> Features { get => version == "Classic" ? _features : _features2023; }
        public Dictionary<string, string> Cloudtiles { get => version == "Classic" ? _cloudtiles : _cloudtiles2023; }
        public Dictionary<string, string> Itemdata { get => version == "Classic" ? _itemdata : _itemdata2023; }
        public Dictionary<string, string> Weapondata { get => version == "Classic" ? _weapondata : _weapondata2023; }
        public Dictionary<string, SKBitmap> Monsterpng { get => version == "Classic" ? _monsterpng : _monsterpng2023; }
        public Dictionary<string, SKBitmap> Characterpng { get => version == "Classic" ? _characterpng : _characterpng2023; }
        public Dictionary<string, SKBitmap> Weaponpng { get => version == "Classic" ? _weaponpng : _weaponpng2023; }
        public Dictionary<string, SKBitmap> Itempng { get => version == "Classic" ? _itempng : _itempng2023; }
        public Dictionary<string, SKBitmap> Alldngnpng { get => version == "Classic" ? _alldngnpng : _alldngnpng2023; }
        public Dictionary<string, SKBitmap> Alleffects { get => version == "Classic" ? _alleffects : _alleffects2023; }
        public Dictionary<string, SKBitmap> Miscallaneous { get => version == "Classic" ? _miscallaneous : _miscallaneous2023; }
        public Dictionary<string, SKBitmap> Floorpng { get => version == "Classic" ? _floorpng : _floorpng2023; }
        public Dictionary<string, SKBitmap> Wallpng { get => version == "Classic" ? _wallpng : _wallpng2023; }
        public IReadFromFileAsync ReadFromFile { get; }
        public bool NeedRefreshDictionaries = true;
        public string version = "Classic";

        public VersionSelectableDataHolder(IReadFromFile fileReader, string gameLocation = @"../../../Extra", string gameLocation2 = @"../../../Extra2023")
        {
            var ReadFromFile = fileReader;
            _characterdata = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/racepng.txt");
            _features = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/features.txt");
            _cloudtiles = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/clouds.txt");
            _itemdata = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/items.txt");
            _weapondata = ReadFromFile.GetDictionaryFromFile(gameLocation + @"/weapons.txt");

            _floorandwall = ReadFromFile.GetFloorAndWallNamesForDungeons(gameLocation + @"/tilefloor.txt");
            _floorandwallColor = ReadFromFile.GetFloorAndWallColours(gameLocation + @"/tilefloorColors.txt");
            _monsterdata = ReadFromFile.GetMonsterData(gameLocation + @"/mon-data.h", gameLocation + @"/monsteroverrides.txt");
            _namedMonsterOverrideData = ReadFromFile.GetNamedMonsterOverrideData(gameLocation + @"/namedmonsteroverrides.txt");

            var extraPngs = ReadFromFile.GetExtraPngFiles(gameLocation);
            _floorpng = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn", "floor"), extraPngs);
            _wallpng = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn", "wall"), extraPngs);
            _alldngnpng = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn"), extraPngs);
            _alleffects = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "effect"), extraPngs);
            _miscallaneous = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "misc"), extraPngs);
            _itempng = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "item"), extraPngs);

            _characterpng = ReadFromFile.GetCharacterPNG(gameLocation);
            _monsterpng = ReadFromFile.GetMonsterPNG(gameLocation);

            _weaponpng = ReadFromFile.GetWeaponPNG(gameLocation);

            _characterdata2023 = ReadFromFile.GetDictionaryFromFile(gameLocation2 + @"/racepng.txt");
            _features2023 = ReadFromFile.GetDictionaryFromFile(gameLocation2 + @"/features.txt");
            _cloudtiles2023 = ReadFromFile.GetDictionaryFromFile(gameLocation2 + @"/clouds.txt");
            _itemdata2023 = ReadFromFile.GetDictionaryFromFile(gameLocation2 + @"/items.txt");
            _weapondata2023 = ReadFromFile.GetDictionaryFromFile(gameLocation2 + @"/weapons.txt");

            _floorandwall2023 = ReadFromFile.GetFloorAndWallNamesForDungeons(gameLocation2 + @"/tilefloor.txt");
            _floorandwallColor2023 = ReadFromFile.GetFloorAndWallColours(gameLocation2 + @"/tilefloorColors.txt");
            _monsterdata2023 = ReadFromFile.GetMonsterData(gameLocation2 + @"/mon-data.h", gameLocation2 + @"/monsteroverrides.txt");
            _namedMonsterOverrideData2023 = ReadFromFile.GetNamedMonsterOverrideData(gameLocation2 + @"/namedmonsteroverrides.txt");

            var extraPngs2 = ReadFromFile.GetExtraPngFiles(gameLocation2);
            _floorpng2023 = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation2, "rltiles", "dngn", "floor"), extraPngs2);
            _wallpng2023 = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation2, "rltiles", "dngn", "wall"), extraPngs2);
            _alldngnpng2023 = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation2, "rltiles", "dngn"), extraPngs2);
            _alleffects2023 = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation2, "rltiles", "effect"), extraPngs2);
            _miscallaneous2023 = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation2, "rltiles", "misc"), extraPngs2);
            _itempng2023 = ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation2, "rltiles", "item"), extraPngs2);

            _characterpng2023 = ReadFromFile.GetCharacterPNG(gameLocation2);
            _monsterpng2023 = ReadFromFile.GetMonsterPNG(gameLocation2);

            _weaponpng2023 = ReadFromFile.GetWeaponPNG(gameLocation2);
        }

        public VersionSelectableDataHolder(IReadFromFileAsync fileReader)
        {
            ReadFromFile = fileReader;
        }

        public async Task InitialiseData()
        {
            if (NeedRefreshDictionaries || _characterdata == null) { NeedRefreshDictionaries = false; }
            else return;

            if (version == "Classic")
            {
                await LoadVersionDataClassic();
                await LoadVersionData2023();
            }
            else
            {
                await LoadVersionData2023();
                await LoadVersionDataClassic();
            }
        }

        private async Task LoadVersionDataClassic()
        {
            var gameLocation = "Extra";
            _characterdata = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/racepng.txt");
            _characterpng = await ReadFromFile.GetCharacterPNG(gameLocation);

            _floorandwall = await ReadFromFile.GetFloorAndWallNamesForDungeons(gameLocation + @"/tilefloor.txt");
            _floorandwallColor = await ReadFromFile.GetFloorAndWallColours(gameLocation + @"/tilefloorColors.txt");
            _floorpng = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn", "floor"), gameLocation);
            _wallpng = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn", "wall"), gameLocation);

            _monsterdata = await ReadFromFile.GetMonsterData(gameLocation + @"/mon-data.h", gameLocation + @"/monsteroverrides.txt");
            _namedMonsterOverrideData = await ReadFromFile.GetNamedMonsterOverrideData(gameLocation + @"/namedmonsteroverrides.txt");
            _monsterpng = await ReadFromFile.GetMonsterPNG(gameLocation);


            _features = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/features.txt");
            _alldngnpng = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn"), gameLocation);

            _cloudtiles = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/clouds.txt");
            _alleffects = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "effect"), gameLocation);
            _miscallaneous = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "misc"), gameLocation);

            _itemdata = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/items.txt");
            _itempng = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "item"), gameLocation);

            _weapondata = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/weapons.txt");
            _weaponpng = await ReadFromFile.GetWeaponPNG(gameLocation);
        }
        private async Task LoadVersionData2023()
        {
            var gameLocation = "Extra2023";
            _characterdata2023 = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/racepng.txt");
            _characterpng2023 = await ReadFromFile.GetCharacterPNG(gameLocation);

            _floorandwall2023 = await ReadFromFile.GetFloorAndWallNamesForDungeons(gameLocation + @"/tilefloor.txt");
            _floorandwallColor2023 = await ReadFromFile.GetFloorAndWallColours(gameLocation + @"/tilefloorColors.txt");
            _floorpng2023 = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn", "floor"), gameLocation);
            _wallpng2023 = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn", "wall"), gameLocation);

            _monsterdata2023 = await ReadFromFile.GetMonsterData(gameLocation + @"/mon-data.h", gameLocation + @"/monsteroverrides.txt");
            _namedMonsterOverrideData2023 = await ReadFromFile.GetNamedMonsterOverrideData(gameLocation + @"/namedmonsteroverrides.txt");
            _monsterpng2023 = await ReadFromFile.GetMonsterPNG(gameLocation);


            _features2023 = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/features.txt");
            _alldngnpng2023 = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "dngn"), gameLocation);

            _cloudtiles2023 = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/clouds.txt");
            _alleffects2023 = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "effect"), gameLocation);
            _miscallaneous2023 = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "misc"), gameLocation);

            _itemdata2023 = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/items.txt");
            _itempng2023 = await ReadFromFile.GetSKBitmapDictionaryFromFolder(Path.Combine(gameLocation, "rltiles", "item"), gameLocation);

            _weapondata2023 = await ReadFromFile.GetDictionaryFromFile(gameLocation + @"/weapons.txt");
            _weaponpng2023 = await ReadFromFile.GetWeaponPNG(gameLocation);
        }
    }
}
