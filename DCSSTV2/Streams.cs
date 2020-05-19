using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DCSSTV2.Streams
{
    public static class Streams
    {
        public static IEnumerable<Stream> TtyrecToStream(this List<string> files)
        {
            return files.Select(f =>
            {
                return File.OpenRead(f);
            });
        }
    }
}
