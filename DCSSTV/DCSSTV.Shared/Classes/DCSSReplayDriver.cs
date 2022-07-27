﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FrameGenerator;
using SkiaSharp;
using TtyRecDecoder;

namespace DCSSTV
{
    class DCSSReplayDriver
    {
        public int ConsoleSwitchLevel = 1;
        private readonly MainGenerator frameGenerator;
        private readonly Action _refreshCanvas;
        private readonly Func<bool> _readyForRefresh;
        public SKBitmap currentFrame { get; private set; }
        private const int TimeStepLengthMS = 5000;
        private readonly List<DateTime> PreviousFrames = new List<DateTime>();
        private DateTime PreviousFrame = DateTime.Now;
        public TimeSpan MaxDelayBetweenPackets = new TimeSpan(0, 0, 0, 0, 500);//milliseconds
        private int FrameStepCount;
        public int framerateControlTimeout = 1000;
        int prevHash = 0;
        public TtyRecKeyframeDecoder ttyrecDecoder = null;
        public double PlaybackSpeed = 0, PausedSpeed = 2;
        public TimeSpan Seek;
        public List<string> files = new List<string>();
        public IEnumerable<Stream> streams;
        TimeSpan delay = TimeSpan.Zero;
        public bool CancellationToken = false;
        List<string> hostsites = new List<string>() { "https://underhound.eu/crawl/ttyrec/", "http://crawl.akrasiac.org/rawdata/", "http://crawl.berotato.org/crawl/ttyrec/",  "https://webzook.net/soup/ttyrecs/", "https://termcast.shalott.org/ttyrecs/dobrazupa.org/ttyrec/",
            "http://crawl.develz.org/ttyrecs/", "https://crawl.xtahua.com/crawl/ttyrec/","https://crawl.kelbi.org/crawl/ttyrec/","http://lazy-life.ddo.jp/mirror/ttyrecs/" };
        string selectedString = "https://underhound.eu/crawl/ttyrec/";

        List<string> linkList = new List<string>() { };
        List<string> ttyrecList = new List<string>() { };
        string selectedttyrec = "";
        string selectedLink = "";
        string Name;

        public DCSSReplayDriver(MainGenerator imageGenerator, Action RefreshCanvas, Func<bool> readyForRefresh)
        {
            frameGenerator = imageGenerator;
            _refreshCanvas = RefreshCanvas;
            _readyForRefresh = readyForRefresh;
        }

        public async Task CancelImageGeneration()
        {
            await Task.Run(() => CancellationToken = false);
        }
        public async Task StartImageGeneration()
        {
            //if (PlaybackSpeed != 0) { PausedSpeed = PlaybackSpeed; PlaybackSpeed = 0; text = "Play"; }
            // else { PlaybackSpeed = PausedSpeed; text = "Pause"; }
            CancellationToken = true;
            while (CancellationToken)
            {
                await Task.Delay(framerateControlTimeout);
                var now = DateTime.Now;

                var dt = Math.Max(0, Math.Min(0.1, (now - PreviousFrame).TotalSeconds));
                PreviousFrame = now;

                if (ttyrecDecoder != null)
                {

                    Seek += TimeSpan.FromSeconds(dt * PlaybackSpeed);

                    if (Seek > ttyrecDecoder.Length)
                    {
                        Seek = ttyrecDecoder.Length;
                    }
                    if (Seek < TimeSpan.Zero)
                    {
                        Seek = TimeSpan.Zero;
                    }

                    if (FrameStepCount != 0)
                    {
                        ttyrecDecoder.FrameStep(FrameStepCount); //step frame index by count
                        Seek = ttyrecDecoder.CurrentFrame.SinceStart;
                        FrameStepCount = 0;
                    }
                    else
                    {

                        ttyrecDecoder.Seek(Seek);

                    }

                    var frame = ttyrecDecoder.CurrentFrame.Data;

                    if (frame != null)
                    {

                        if (!frameGenerator.isGeneratingFrame && prevHash != frame.GetHashCode() && _readyForRefresh.Invoke())
                        {
                            frameGenerator.isGeneratingFrame = true;
#if faklse
                            ThreadPool.UnsafeQueueUserWorkItem(o =>
                            {
                                try
                                {
                                    currentFrame = frameGenerator.GenerateImage(frame, ConsoleSwitchLevel);
                                    frameGenerator.isGeneratingFrame = false;
                                    frame = null;
                                    _refreshCanvas();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    //generator.GenerateImage(savedFrame);
                                    frameGenerator.isGeneratingFrame = false;
                                }
                            }, null);
#else //non threaded image generation (slow)
                            currentFrame = frameGenerator.GenerateImage(frame);
#if DEBUG
                            Console.WriteLine("driver " + currentFrame.ByteCount);
#endif
                            frameGenerator.isGeneratingFrame = false;
                            prevHash = frame.GetHashCode();
                            _refreshCanvas();
#endif
                        }
                    }

                }
                else
                {
                    currentFrame = frameGenerator.GenerateImage(null);

                }

            }

        }
        public void GetImage()
        {
            //if (PlaybackSpeed != 0) { PausedSpeed = PlaybackSpeed; PlaybackSpeed = 0; text = "Play"; }
            // else { PlaybackSpeed = PausedSpeed; text = "Pause"; }
            var now = DateTime.Now;

            var dt = Math.Max(0, Math.Min(0.1, (now - PreviousFrame).TotalSeconds));
            PreviousFrame = now;

            if (ttyrecDecoder != null)
            {

                Seek += TimeSpan.FromSeconds(dt * PlaybackSpeed);

                if (Seek > ttyrecDecoder.Length)
                {
                    Seek = ttyrecDecoder.Length;
                }
                if (Seek < TimeSpan.Zero)
                {
                    Seek = TimeSpan.Zero;
                }

                if (FrameStepCount != 0)
                {
                    ttyrecDecoder.FrameStep(FrameStepCount); //step frame index by count
                    Seek = ttyrecDecoder.CurrentFrame.SinceStart;
                    FrameStepCount = 0;
                }
                else
                {

                    ttyrecDecoder.Seek(Seek);

                }

                var frame = ttyrecDecoder.CurrentFrame.Data;

                if (frame != null)
                {

#if faklse
                    ThreadPool.UnsafeQueueUserWorkItem(o =>
                    {
                        try
                        {
                            currentFrame = frameGenerator.GenerateImage(frame, ConsoleSwitchLevel);
                            frameGenerator.isGeneratingFrame = false;
                            frame = null;
                            _refreshCanvas();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            //generator.GenerateImage(savedFrame);
                            frameGenerator.isGeneratingFrame = false;
                        }
                    }, null);
#else //non threaded image generation (slow)
                currentFrame = frameGenerator.GenerateImage(frame);
                Console.WriteLine("driver " + currentFrame.ByteCount);
#endif
                }

            }
            else
            {
                currentFrame = frameGenerator.GenerateImage(null);

            }
        }
    }
}
