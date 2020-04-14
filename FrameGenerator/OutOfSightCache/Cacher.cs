using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FrameGenerator.OutOfSightCache
{
    public class DrawnBMP
    {
        public bool DrawFloor { get; set; }
        public Bitmap Image { get; set; }
    }
    public class Cacher
    {
        private Dictionary<char, DrawnBMP> OutofSightCache { get; set; }

        public void UpdateCache(List<Tuple<string, DrawnBMP>> lastDrawnFrameKeyValues)
        {
            lastDrawnFrameKeyValues
                .OrderBy((tile) => tile.Item1[0])
                .ThenBy((tile) => tile.Item1)
                .GroupBy((tile) => tile.Item1[0])
                .First()
                .Select((mostCommonKeyvalueForChar) => OutofSightCache[mostCommonKeyvalueForChar.Item1[0]] = mostCommonKeyvalueForChar.Item2);               
        }
        public bool TryGetLastSeenBitmapByChar(char key, out DrawnBMP lastSeen)
        {
            return OutofSightCache.TryGetValue(key, out lastSeen);
        }
    }
}
