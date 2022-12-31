﻿using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XZ.NET;

namespace DCSSTV.Streams
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
        public static IEnumerable<Stream> TtyrecToStream(string[] files)
        {
            return files.Select(f =>
            {

                if (Path.GetExtension(f) == ".ttyrec") return File.OpenRead(f);
                Stream streamCompressed = File.OpenRead(f);
                Stream streamUncompressed = new MemoryStream();
                if (Path.GetExtension(f) == ".bz2")
                {
                    try
                    {
                        BZip2.Decompress(streamCompressed, streamUncompressed, false);
                    }
                    catch
                    {
                        //MessageBox.Show("The file is corrupted or not supported");
                    }
                    return streamUncompressed;
                }
                if (Path.GetExtension(f) == ".gz")
                {
                    try
                    {
                        GZip.Decompress(streamCompressed, streamUncompressed, false);
                    }
                    catch
                    {
                        //MessageBox.Show("The file is corrupted or not supported");
                    }
                    return streamUncompressed;
                }
                if (Path.GetExtension(f) == ".xz")
                {
                    try
                    {
                        using var xzStream = new XZOutputStream(streamCompressed);
                        xzStream.CopyTo(streamUncompressed);
                    }
                    catch
                    {
                        //MessageBox.Show("The file is corrupted or not supported");
                    }
                    return streamUncompressed;
                }
                return null;
            });
        }
        public static Stream CompressedTtyrecToStream(string name, Stream maybeCompressed)
        {
            Stream streamUncompressed = new MemoryStream();
            if (name.Contains(".bz2"))
            {
                try
                {
                    BZip2.Decompress(maybeCompressed, streamUncompressed, false);
                }
                catch
                {
                    throw;
                }
                return streamUncompressed;
            }
            if (name.Contains(".gz"))
            {
                try
                {
                    GZip.Decompress(maybeCompressed, streamUncompressed, false);
                }
                catch
                {
                throw;
                //MessageBox.Show("The file is corrupted or not supported");
            }
                return streamUncompressed;
            }
            if (name.Contains(".xz"))
            {
                try
                {
                    using var xzStream = new XZOutputStream(maybeCompressed);
                    xzStream.CopyTo(streamUncompressed);
                    streamUncompressed.Position = 0;
                }
                catch
                {
                    throw;
                //MessageBox.Show("The file is corrupted or not supported");
                }
                return streamUncompressed;
            }
            return maybeCompressed;
        }

    }
}
