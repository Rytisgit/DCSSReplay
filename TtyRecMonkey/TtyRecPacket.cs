// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System;
using System.Collections.Generic;
using System.IO;

namespace TtyRecMonkey
{
    struct TtyRecPacket
    {
        public TimeSpan SinceStart;
        public byte[] Payload;

        public static IEnumerable<TtyRecPacket> DecodePackets(IEnumerable<Stream> streams, TimeSpan delay_between_streams, Func<bool> checkinterrupt)
        {
            TimeSpan BaseDelay = TimeSpan.Zero;
            TimeSpan LastPacketSS = TimeSpan.Zero;

            bool first_stream = true;

            foreach (var stream in streams)
            {
                if (checkinterrupt()) break;

                stream.Position = 0;

                using (var reader = new BinaryReader(stream))
                {
                    int first_sec = int.MinValue;
                    int first_usec = int.MinValue;

                    while (stream.Position < stream.Length)
                    {
                        bool first_packet_of_stream = stream.Position == 0;
                        int sec = reader.ReadInt32();
                        int usec = reader.ReadInt32();
                        int len = reader.ReadInt32();

                        if (first_packet_of_stream)
                        {
                            first_sec = sec;
                            first_usec = usec;

                            if (!first_stream) yield return new TtyRecPacket() { SinceStart = BaseDelay, Payload = null }; // force a restart
                            first_stream = false;
                        }

                        var since_start = TimeSpan.FromSeconds(sec - first_sec) + TimeSpan.FromMilliseconds((usec - first_usec) / 1000);

                        yield return new TtyRecPacket()
                        {
                            SinceStart = LastPacketSS = BaseDelay + since_start
                            ,
                            Payload = reader.ReadBytes(len)
                        };
                    }
                }

                BaseDelay = LastPacketSS + delay_between_streams;
            }
        }
    }
}
