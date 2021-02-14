using System;
using System.Collections.Generic;
using System.Text;
using InputParser.Abstract;
using Putty;
using static InputParser.Constant.Helpers;

namespace InputParser.Decorators
{
    public class LogDataDecorator : AbstractDecorator
    {
        public LogDataDecorator(IParser model) : base(model) { }

        public override Model ParseData(TerminalCharacter[,] characters)
        {
            var parsedModel = base.ParseData(characters);
            parsedModel.LogData = ParseLogLines(characters);
            return parsedModel;
        }

        private static LogData[] ParseLogLines(TerminalCharacter[,] chars)
        {
            var loglines = new LogData[7] { new LogData(), new LogData(), new LogData(), new LogData(), new LogData(), new LogData(), new LogData() };
            StringBuilder logLine = new StringBuilder();
            var logText = new List<string>();
            var logBackground = new List<string>();
            var loglineRow = 17;
            foreach (var line in loglines)
            {
                for (int i = 0; i < FullWidth; i++)
                {
                    logLine.Append(GetCharacter(chars[i, loglineRow]));
                }
                line.LogTextRaw = logLine.ToString();
                if (line.LogTextRaw.Length > 0)
                {
                    line.Empty = false;
                    for (int i = 0; i < line.LogTextRaw.Length; i++)
                    {
                        logText.Add(GetColoredCharacter(chars[i, loglineRow]));
                        logBackground.Add(GetBackgroundColor(chars[i, loglineRow]));
                    }
                    line.LogText = logText.ToArray();
                    line.LogBackground = logBackground.ToArray();
                    logText.Clear();
                    logBackground.Clear();
                }
                logLine.Clear();
                loglineRow++;
            }
            return loglines;
        }
    }
}
