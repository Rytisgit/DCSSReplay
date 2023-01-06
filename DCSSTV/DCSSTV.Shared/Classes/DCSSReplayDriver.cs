using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
        private readonly Action<int, int, string> _updateSeekbar;

        public SKBitmap currentFrame { get; private set; }
        private DateTime PreviousFrame = DateTime.Now;
        public TimeSpan MaxDelayBetweenPackets = new TimeSpan(0, 0, 0, 0, 500);//milliseconds
        private int FrameStepCount;
        public int framerateControlTimeout = 1000;
        int prevHash = 0;
        public TtyRecKeyframeDecoder ttyrecDecoder = null;
        public TimeSpan Seek;
        public List<string> files = new List<string>();
        public IEnumerable<Stream> streams;
        public bool CancellationToken = false;

        public DCSSReplayDriver(MainGenerator imageGenerator, Action RefreshCanvas, Func<bool> readyForRefresh, Action<int, int, string> updateSeekbar)
        {
            frameGenerator = imageGenerator;
            _refreshCanvas = RefreshCanvas;
            _readyForRefresh = readyForRefresh;
            _updateSeekbar = updateSeekbar;
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

                    Seek += TimeSpan.FromSeconds(dt * ttyrecDecoder.PlaybackSpeed);

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
                var start = ttyrecDecoder == null ? new System.TimeSpan(0) : ttyrecDecoder.CurrentFrame.SinceStart;
                var end = ttyrecDecoder == null ? new System.TimeSpan(0) : ttyrecDecoder.Length;
                var progress = start.TotalMilliseconds / end.TotalMilliseconds;

                // Return the formatted strings in the desired format
                var remainingTime = $"{start.ToString(@"hh\:mm\:ss")} / {end.ToString(@"hh\:mm\:ss")}";
                _updateSeekbar((int)start.TotalMilliseconds, (int)end.TotalMilliseconds, remainingTime);

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

                Seek += TimeSpan.FromSeconds(dt * ttyrecDecoder.PlaybackSpeed);

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
