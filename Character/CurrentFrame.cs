namespace TtyRecMonkey
{
    public static class CurrentFrame
    {
        public static Putty.TerminalCharacter[,] frame { get; set; } = new Putty.TerminalCharacter[80, 30];

    }
}
