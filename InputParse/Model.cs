
using System.Collections.Generic;
using Putty;

namespace InputParser
{
    public class Model : IParser
    {
        public virtual LayoutType Layout { get; set; } = LayoutType.Normal;
        public virtual string Location { get; set; } = "Dungeon";
        public virtual string[] TileNames { get; set; } = { " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", "!LIGHTBLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", "#BLUE", "#BLUE", "#BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "*BLUE", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ",BLUE", ",BLUE", ",BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", " LIGHTGREY", ",BLUE", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", "#BLUE", "#BLUE", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "+BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "rBLUE", "FBLUE", "OBLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", "+BLUE", "#BLUE", "#BLUE", "#BLUE", "#BLUE", ".BLUE", ".BLUE", ".BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".LIGHTGREY", "#BLUE", "#BLUE", "#BLUE", "#BLUE", "#BLUE", "#BLUE", "#BLUE", ".BLUE", "#BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ".BLUE", "*BLUE", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", "+LIGHTGREY", "#BROWN", "#BROWN", "#BROWN", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "<GREEN", ".LIGHTGREY", "#BROWN", "#BROWN", "#BROWN", "#BROWN", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "#BROWN", " LIGHTGREY", ",BLUE", " LIGHTGREY", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", "#BLUE", "#BLUE", "#BROWN", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "@BLACK", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "#BROWN", "*BLUE", " LIGHTGREY", ",BLUE", " LIGHTGREY", "*BLUE", ",BLUE", " LIGHTGREY", ",BLUE", " LIGHTGREY", " LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "+BLUE", ",BLUE", ",BLUE", "#BROWN", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "#BROWN", "#BROWN", ".LIGHTGREY", ".LIGHTGREY", "ÃLIGHTGREY", "·LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "#BROWN", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", ",BLUE", " LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", ".BLUE", " LIGHTGREY", ",BLUE", "#BROWN", ".LIGHTGREY", ".LIGHTGREY", ".BLUE", "#BLUE", "#BLUE", "<GREEN", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "#BROWN", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", ",BLUE", "*BLUE", "*BLUE", " LIGHTGREY", " LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", ".BLUE", "âBLUE", "(CYAN", "âBLUE", ".BLUE", "âBLUE", ".BLUE", " LIGHTGREY", "#BROWN", ".LIGHTGREY", ".BLUE", ".BLUE", "#BLUE", "#BLUE", "#BROWN", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", ".LIGHTGREY", "#BROWN", "*BLUE", ",BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ",BLUE", " LIGHTGREY", " LIGHTGREY", ".BLUE", ".BLUE", ".BLUE", "âBLUE", "#BLUE", "#BLUE", "+BLUE", "#BLUE", "#BLUE", "âBLUE", ".BLUE", "#BROWN", ".BLUE", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", "#BLUE", "OLIGHTRED", "@GREEN", ".LIGHTGREY", "(BROWN", "#BROWN", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", "*BLUE", " LIGHTGREY", "#BLUE", "#BLUE", "#BLUE", "#BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", "#BLUE", ".BLUE", "#BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", "#BLUE", ".LIGHTGREY", "<GREEN", ".LIGHTGREY", ".LIGHTGREY", "+LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ",BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", ".BLUE", "#BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", "#BLUE", ".BLUE", ".LIGHTGREY", "!LIGHTBLUE", ".LIGHTGREY", "#BROWN", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "#BLUE", ".BLUE", ".BLUE", ".BLUE", "#BLUE", " LIGHTGREY", "#BLUE", "#BLUE", "#BROWN", "#BROWN", "#BROWN", "#BROWN", "*BLUE", ",BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", "*BLUE", " LIGHTGREY", ",BLUE", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", "*BLUE", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY", " LIGHTGREY" };
        public virtual string[] HighlightColors { get; set; } = { "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "LIGHTGREY", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK", "BLACK" };
        public virtual int LineLength { get; set; } = 33;
        public virtual LogData[] LogData { get; set; } = new LogData[7] { new LogData(), new LogData(), new LogData(), new LogData(), new LogData(), new LogData(), new LogData() };
        public virtual MonsterData[] MonsterData { get; set; } = new MonsterData[4] { new MonsterData(), new MonsterData(), new MonsterData(), new MonsterData() };
        public virtual SideData SideData {get; set; } = new SideData();

        public virtual SideDataColored SideDataColored { get; set; } = new SideDataColored();
        public virtual ColorList ColorList { get; set; } = new ColorList();
        public Model ParseData(TerminalCharacter[,] chars)
        {
            return this;
        }
    }

    public class LogData
    {
        public bool Empty { get; set; } = true;
        public string[] LogText { get; set; }//{" LIGHTGRAY, jYELLOW" ...}
        public string[] LogBackground { get; set; }//{"YELLOW",....}
        public string LogTextRaw { get; set; }// jackal

    }
    public class MonsterData
    {
        public bool Empty { get; set; } = true;
        public string[] MonsterText { get; set; }//{" LIGHTGRAY, jYELLOW" ...}
        public string MonsterTextRaw { get; set; }// jackal
        public string[] MonsterDisplay { get; set; }//{"jYELLOW",....}
        public string[] MonsterBackground { get; set; }//{"YELLOW",....}
    }
    public class SideData
    {
        public string Name { get; set; } = "Sentinel The Covered";
        public string Race { get; set; } = "Ogre of Jiyva Jeggou ******";
        public int Health { get; set; } = -320;
        public int MaxHealth { get; set; } = 320;
        public string TrueHealth { get; set; } = "(321)";
        public int Magic { get; set; } = 50;
        public int MaxMagic { get; set; } = 50;
        public string TrueMagic { get; set; } = "(100)";
        public string ArmourClass { get; set; } = "100";
        public string Evasion { get; set; } = "100";
        public string Shield { get; set; } = "100";
        public string ExperienceLevel { get; set; } = "27";
        public string NextLevel { get; set; } = "-130%";
        public string Weapon { get; set; } = "a) +4 demon trident \"Embalmer\" {pierce, *Corrode Dex+3 SInv Int+10}";
        public string Quiver { get; set; } = "b) +4 demon trident \"Embalmer\" {pierce, *Corrode Dex+3 SInv Int+10}";
        public string Strength { get; set; } = "9999";
        public string Dexterity { get; set; } = "9999";
        public string Inteligence { get; set; } = "9999";
        public string Place { get; set; } = "Dungeon:1";
        public string NoisyGold { get; set; } = "===---";
        public string Time { get; set; } = "9999999 (25.0)";
        public string Statuses1 { get; set; } = "Slow Agi Might Haste Para";
        public string Statuses2 { get; set; } = "Tree -Tele ";

    };
    public class SideDataColored
    {
        public List<string> Statuses1 { get; set; } = new List<string>();
        public List<string> Statuses2 { get; set; } = new List<string>();

    };
}
