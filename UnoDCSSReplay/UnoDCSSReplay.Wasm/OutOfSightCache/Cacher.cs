using System;
using System.Collections.Generic;
using SkiaSharp;
using System.Linq;

namespace FrameGenerator.wasm.OutOfSightCache
{
    public class Cacher
    {
        private string LocationOfCache { get; set; }
        public Dictionary<char, SKBitmap> OutofSightCache { get; set; }

        public Cacher()
        {
            OutofSightCache = new Dictionary<char, SKBitmap>();
            LocationOfCache = "";
        }


        public void UpdateCache(List<Tuple<string, SKBitmap>> lastDrawnFrameKeyValues)
        {
            var orderedList = lastDrawnFrameKeyValues
                .OrderBy((tile) => tile.Item1[0])
                .ThenBy((tile) => tile.Item1)
                .GroupBy((tile) => tile.Item1[0])
                .Select((mostCommonKeyvalueForChar) => {
                    var firstFromGroup = mostCommonKeyvalueForChar.First();
                    return new Tuple<char, SKBitmap>(firstFromGroup.Item1[0], firstFromGroup.Item2);
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
        public bool TryGetLastSeenBitmapByChar(char key, out SKBitmap lastSeen)
        {
            //TODO Track Cache Hits
            return OutofSightCache.TryGetValue(key, out lastSeen);
        }

        public void DumpDataOnLocationChange(string currentLocation)
        {
            if (!LocationOfCache.Equals(currentLocation))
            {
                LocationOfCache = currentLocation;
                OutofSightCache.Clear();
            }
        }
    }
}
