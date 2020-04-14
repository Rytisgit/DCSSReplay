using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FrameGenerator.OutOfSightCache
{
    public class Cacher
    {
        public Dictionary<char, Bitmap> OutofSightCache { get; set; }

        public Cacher()
        {
            OutofSightCache = new Dictionary<char, Bitmap>();
        }
        //TODO DUMP Cahce on new level
        public void UpdateCache(List<Tuple<string, Bitmap>> lastDrawnFrameKeyValues)
        {
            var orderedList = lastDrawnFrameKeyValues
                .OrderBy((tile) => tile.Item1[0])
                .ThenBy((tile) => tile.Item1)
                .GroupBy((tile) => tile.Item1[0])
                .Select((mostCommonKeyvalueForChar) => {
                    var firstFromGroup = mostCommonKeyvalueForChar.First();
                    return new Tuple<char, Bitmap>(firstFromGroup.Item1[0], firstFromGroup.Item2);
                    }
                );
            foreach (var lastSeenImage in orderedList)
            {
                if (lastSeenImage.Item2 != null)
                {
                    OutofSightCache[lastSeenImage.Item1] = lastSeenImage.Item2;
                }
            }
        }
        public bool TryGetLastSeenBitmapByChar(char key, out Bitmap lastSeen)
        {
            return OutofSightCache.TryGetValue(key, out lastSeen);
        }
    }
}
