using System;
using System.Collections.Generic;
using System.Text;
using Putty;
using static InputParser.Helpers;

namespace InputParser.Decorators
{
    class LogDataDecorator : AbstractDecorator
    {
        public LogDataDecorator(IParser model) : base(model) { }

        public override Model ParseData(TerminalCharacter[,] characters)
        {
            base.model.ParseData(characters);
            if (!(base.model is Model)) return new Model();
            var model = (Model)base.model;
            model.LogData = ParseLogLines(characters);
            return model;
        }

        private static LogData[] ParseLogLines(TerminalCharacter[,] characters)
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
                    logLine.Append(GetCharacter(characters[i, loglineRow]));
                }
                line.LogTextRaw = logLine.ToString();
                if (line.LogTextRaw.Length > 0)
                {
                    line.Empty = false;
                    for (int i = 0; i < line.LogTextRaw.Length; i++)
                    {
                        logText.Add(GetColoredCharacter(characters[i, loglineRow]));
                        logBackground.Add(GetBackgroundColor(characters[i, loglineRow]));
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
