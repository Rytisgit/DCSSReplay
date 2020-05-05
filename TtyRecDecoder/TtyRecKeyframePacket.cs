// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using Putty;
using System;
using System.Collections.Generic;

namespace TtyRecDecoder
{
    class AnnotatedPacket
    {
        public TimeSpan SinceStart;
        public byte[] Payload;
        public Terminal RestartPosition;
        public TerminalCharacter[,] DecodedCache;
        public WeakReference DecodedCacheWeak;

        public bool IsKeyframe { get { return RestartPosition != null; } }

        public static IEnumerable<AnnotatedPacket> AnnotatePackets(int w, int h, IEnumerable<TtyRecPacket> packets, Func<bool> checkinterrupt)
        {
            var term = new Terminal(w, h);
            var memory_budget3 = 100 * 1000 * 1000;
            var time_budget = TimeSpan.FromMilliseconds(10);

            var last_restart_position_time = DateTime.MinValue;
            var last_restart_memory_avail = memory_budget3 / 3;

            foreach (var packet in packets)
            {
                if (checkinterrupt()) break;

                var now = DateTime.Now;

                bool need_restart = (last_restart_position_time + time_budget < now) || (last_restart_memory_avail <= 1000);

                if (packet.Payload == null)
                {
                    using (term) { }
                    term = new Terminal(w, h);
                    need_restart = true;
                }

                yield return new AnnotatedPacket()
                {
                    SinceStart = packet.SinceStart,
                    Payload = packet.Payload,
                    RestartPosition = need_restart ? new Terminal(term) : null
                };

                if (need_restart)
                {
                    last_restart_position_time = now;
                    last_restart_memory_avail = memory_budget3 / 3;
                }
                else
                {
                    last_restart_memory_avail -= w * h * 12;
                }

                if (packet.Payload != null) term.Send(packet.Payload);
            }
            using (term) { }
        }
    }
}
