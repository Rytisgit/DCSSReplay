using InputParser;

namespace FrameGenerator.Extensions
{
    public static class Extensions
    {
        public static bool MonsterIsVisible(this MonsterData[] monsterData, string MonsterName) =>
            !monsterData[0].empty && monsterData[0].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[1].empty && monsterData[1].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[2].empty && monsterData[2].MonsterTextRaw.Contains(MonsterName) ||
            !monsterData[3].empty && monsterData[3].MonsterTextRaw.Contains(MonsterName);
    }
}