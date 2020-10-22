using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InputParser.Abstract;
using Putty;
using static InputParser.Constant.Helpers;

namespace InputParser.Decorators
{
    class MonsterDataDecorator : AbstractDecorator
    {
        public MonsterDataDecorator(Model model) : base(model) { }

        public override Model ParseData(TerminalCharacter[,] characters)
        {
            var parsedModel = base.model.ParseData(characters);
            if (!(base.model is Model)) return new Model();
            parsedModel.MonsterData = ParseMonsterDisplay(characters);
            return parsedModel;
        }

        private static MonsterData[] ParseMonsterDisplay(TerminalCharacter[,] characters)
        {
            List<MonsterData> monsterDataList = new List<MonsterData>(4);
            StringBuilder monsterLine = new StringBuilder();
            List<string> monsterLineColored = new List<string>(AlmostFullWidth - GameViewWidth - 4);
            List<string> monsterLineBackground = new List<string>(AlmostFullWidth - GameViewWidth - 4);

            var lineOffset = 13;
            int currentChar = 0;
            for (int lineIndex = 0; lineIndex < 4; lineIndex++)
            {
                for (int i = GameViewWidth + 4; i < AlmostFullWidth; i++, currentChar++)
                {
                    monsterLine.Append(GetCharacter(characters[i, lineIndex + lineOffset]));
                    monsterLineColored.Add(GetColoredCharacter(characters[i, lineIndex + lineOffset]));
                    monsterLineBackground.Add(GetBackgroundColor(characters[i, lineIndex + lineOffset]));
                }
                monsterDataList.Add(FormatMonsterData(monsterLine.ToString(), monsterLineColored.ToArray(), monsterLineBackground.ToArray()));
                monsterLine.Clear();
                monsterLineColored.Clear();
                monsterLineBackground.Clear();
            }

            return monsterDataList.ToArray();
        }

        private static MonsterData FormatMonsterData(string monsterLine, string[] monsterLineColored, string[] monsterBackgroundColors)
        {
            if (monsterLine[0].Equals(' '))
            {
                return new MonsterData();
            }
            var chars = new char[] { ' ' };
            var split = monsterLine.ToString().Split(chars, count: 2);
            return new MonsterData()
            {
                Empty = false,
                MonsterTextRaw = split[1],
                MonsterDisplay = monsterLineColored.Take(split[0].Length).ToArray(),
                MonsterText = monsterLineColored.Skip(split[0].Length).ToArray(),
                MonsterBackground = monsterBackgroundColors
            };
        }
    }
}
