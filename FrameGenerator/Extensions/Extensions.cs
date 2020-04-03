using InputParser;

namespace FrameGenerator.Extensions
{
    public static class Extensions
    {
        public static bool MonsterIsVisible(this MonsterData[] monsterData, string MonsterName) =>
            !monsterData[0].Empty && monsterData[0].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[1].Empty && monsterData[1].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[2].Empty && monsterData[2].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[3].Empty && monsterData[3].MonsterTextRaw.Contains(MonsterName);
    }
}