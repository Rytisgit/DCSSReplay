using System.Collections.Generic;

namespace FrameGenerator.wasm.Models
{
    public class NamedMonsterOverride
    {
        public NamedMonsterOverride(string name, string location, Dictionary<string, string> tileNameOverrides)
        {
            Name = name;
            Location = location;
            TileNameOverrides = tileNameOverrides;
        }
        public string Name { get; private set; }
        public string Location { get; private set; }
        public Dictionary<string, string> TileNameOverrides { get; private set; }

    }
}
