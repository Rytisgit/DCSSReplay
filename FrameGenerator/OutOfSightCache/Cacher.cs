using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FrameGenerator.OutOfSightCache
{
    public class Cacher
    {
        private Dictionary<char, Bitmap> OutofSightCache { get; set; }

        public void UpdateCache(List<Tuple<string, Bitmap>> lastDrawnFrameKeyValues)
        {
            lastDrawnFrameKeyValues
                .OrderBy((tile) => tile.Item1[0])
                .ThenBy((tile) => tile.Item1)
                .GroupBy((tile) => tile.Item1[0])
                .First()
                .Select((mostCommonKeyvalueForChar) => OutofSightCache[mostCommonKeyvalueForChar.Item1[0]] = mostCommonKeyvalueForChar.Item2);               
        }
        public bool TryGetLastSeenBitmapByChar(char key, out Bitmap lastSeen)
        {
            return OutofSightCache.TryGetValue(key, out lastSeen);
        }
    }
}
