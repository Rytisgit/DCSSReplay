// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using SlimDX;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShinyConsole
{
    using Blend = SlimDX.Direct3D9.Blend;
    using Matrix = SlimDX.Matrix;
    using Vertex = BasicShinyConsoleFormVertex;

    struct BasicShinyConsoleFormVertex
    {
        public Vector3 Position;
        public uint Specular, Diffuse;
        public Vector2 Texture;

        public static readonly int Size = Marshal.SizeOf(typeof(BasicShinyConsoleFormVertex));
        public static readonly VertexFormat FVF = VertexFormat.Position | VertexFormat.Specular | VertexFormat.Diffuse | VertexFormat.Texture1;
    }

    [System.ComponentModel.DesignerCategory("")]
    public class BasicShinyConsoleForm<CC> : Form2 where CC : struct, IConsoleCharacter
    {
        static readonly Direct3D D3D = new Direct3D();
       // Bitmap bitmap;
        Rectangle cropRect = new Rectangle(0,0,1360,400);
        protected CC[,] Buffer;
        public new int Width { get { return Buffer.GetLength(0); } }
        public new int Height { get { return Buffer.GetLength(1); } }
        public Size GlyphSize = new Size(12, 12);
        public Size GlyphOverlap = new Size(0, 0);

        public int Zoom = 1;

        public Size ActiveSize
        {
            get
            {
                return new Size
                    (pictureBox1.Width * Zoom
                    , pictureBox1.Height * Zoom
                    );
            }
        }

        public void FitWindowToMetrics()
        {
            ClientSize = ActiveSize;
        }

        Device Device;

        readonly Dictionary<Font, PerFontData> FontData = new Dictionary<Font, PerFontData>();
        class PerFontData : IDisposable
        {
            public Texture Texture;
            public Size TextureSize;
            public int GlyphCount;
            public VertexBuffer VB;
            public IndexBuffer IB;
            public DataStream VBWriter;
            public DataStream IBWriter;

            public void Dispose()
            {
                using (Texture) Texture = null;
                using (VB) VB = null;
                using (IB) IB = null;
            }
        }

        public BasicShinyConsoleForm(int w, int h)
        {
            Font = new System.Drawing.Font("Courier New", 7f);

            SetupDevice();
            Buffer = new CC[w, h];
            ClientSize = ActiveSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TeardownDevice();
                Device = null;
                using (D3D) { }
            }
            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs e)
        {
            TeardownDevice();
            Device = null;
            base.OnResize(e);
        }

        void TeardownDevice()
        {
            foreach (var fd in FontData.Values) using (fd) { }
            FontData.Clear();

            using (Device) Device = null;
        }

        void SetupDevice()
        {
            TeardownDevice();
            Device = new Device(D3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, new PresentParameters()
            {
                BackBufferCount = 1
                ,
                BackBufferFormat = Format.X8R8G8B8
                ,
                BackBufferHeight = ClientSize.Height
                ,
                BackBufferWidth = ClientSize.Width
                ,
                DeviceWindowHandle = Handle
                ,
                Windowed = true
            });
        }

        public virtual void Redraw()
        {
           if (Device == null) SetupDevice();

            var w = Width;
            var h = Height;
            foreach (var fd in FontData.Values) fd.GlyphCount = 0;

            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                    if (Buffer[x, y].Font != null)
                    {
                        if (!FontData.ContainsKey(Buffer[x, y].Font)) FontData.Add(Buffer[x, y].Font, new PerFontData());
                        ++FontData[Buffer[x, y].Font].GlyphCount;
                    }

            foreach (var fd in FontData)
            {
                if (fd.Value.GlyphCount == 0) continue;
                if (fd.Value.Texture == null)
                {
                    var bw = fd.Key.Bitmap.Width;
                    var bh = fd.Key.Bitmap.Height;
                    var src = fd.Key.Bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bw, bh), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    fd.Value.Texture = new Texture(Device, bw, bh, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                    var dst = fd.Value.Texture.LockRectangle(0, LockFlags.None);
                    for (int y = 0; y < bh; ++y)
                    {
                        dst.Data.Seek(y * dst.Pitch, System.IO.SeekOrigin.Begin);
                        dst.Data.WriteRange(new IntPtr(src.Scan0.ToInt64() + src.Stride * y), src.Width * 4);
                    }
                    fd.Key.Bitmap.UnlockBits(src);
                    fd.Value.Texture.UnlockRectangle(0);
                    fd.Value.TextureSize = new Size(bw, bh);
                }

                var req_ib_size = sizeof(uint) * 6 * fd.Value.GlyphCount;
                var req_vb_size = Vertex.Size * 4 * fd.Value.GlyphCount;

                if (fd.Value.IB == null || fd.Value.IB.Description.SizeInBytes < req_ib_size) using (fd.Value.IB) fd.Value.IB = new IndexBuffer(Device, Math.Max(req_ib_size, fd.Value.IB == null ? 0 : 2 * fd.Value.IB.Description.SizeInBytes), Usage.None, Pool.Managed, false);
                if (fd.Value.VB == null || fd.Value.VB.Description.SizeInBytes < req_vb_size) using (fd.Value.VB) fd.Value.VB = new VertexBuffer(Device, Math.Max(req_vb_size, fd.Value.VB == null ? 0 : 2 * fd.Value.VB.Description.SizeInBytes), Usage.None, Vertex.FVF, Pool.Managed);

                fd.Value.IBWriter = fd.Value.IB.Lock(0, 0, LockFlags.None);
                fd.Value.VBWriter = fd.Value.VB.Lock(0, 0, LockFlags.None);

                fd.Value.GlyphCount = 0;
            }

            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                    if (Buffer[x, y].Font != null)
                    {
                        var fd = FontData[Buffer[x, y].Font];
                        var i = 4 * fd.GlyphCount;

                        var overlap_l = (GlyphOverlap.Width + 0) / 2;
                        var overlap_r = (GlyphOverlap.Width + 1) / 2;
                        var overlap_t = (GlyphOverlap.Height + 0) / 2;
                        var overlap_b = (GlyphOverlap.Height + 1) / 2;

                        var stridex = GlyphSize.Width - GlyphOverlap.Width;
                        var stridey = GlyphSize.Height - GlyphOverlap.Height;

                        var pl = ((x + 0) * stridex) * Zoom;
                        var pr = ((x + 1) * stridex) * Zoom;
                        var pt = ((y + 0) * stridey) * Zoom;
                        var pb = ((y + 1) * stridey) * Zoom;

                        var tl = (((Buffer[x, y].Glyph % 16) + 0f) * GlyphSize.Width + overlap_l) / fd.TextureSize.Width;
                        var tr = (((Buffer[x, y].Glyph % 16) + 1f) * GlyphSize.Width - overlap_r) / fd.TextureSize.Width;
                        var tt = (((Buffer[x, y].Glyph / 16) + 0f) * GlyphSize.Height + overlap_t) / fd.TextureSize.Height;
                        var tb = (((Buffer[x, y].Glyph / 16) + 1f) * GlyphSize.Height - overlap_b) / fd.TextureSize.Height;

                        fd.IBWriter.WriteRange(new[] { i + 0, i + 1, i + 2, i + 0, i + 2, i + 3 });
                        fd.VBWriter.WriteRange(new[]
                            { new Vertex() { Position = new Vector3(pl,pt,0), Diffuse=Buffer[x,y].Foreground, Specular=Buffer[x,y].Background, Texture = new Vector2(tl,tt) } // TODO:  Correct
					, new Vertex() { Position = new Vector3(pr,pt,0), Diffuse=Buffer[x,y].Foreground, Specular=Buffer[x,y].Background, Texture = new Vector2(tr,tt) }
                    , new Vertex() { Position = new Vector3(pr,pb,0), Diffuse=Buffer[x,y].Foreground, Specular=Buffer[x,y].Background, Texture = new Vector2(tr,tb) }
                    , new Vertex() { Position = new Vector3(pl,pb,0), Diffuse=Buffer[x,y].Foreground, Specular=Buffer[x,y].Background, Texture = new Vector2(tl,tb) }
                    });
                        ++fd.GlyphCount;
                    }

            foreach (var fd in FontData)
            {
                fd.Value.VB.Unlock();
                fd.Value.IB.Unlock();
            }

            Device.BeginScene();

            Device.Clear(ClearFlags.Target, unchecked((int)0xFF112233u), 0f, 0);
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            Device.SetRenderState(RenderState.Lighting, false);
            Device.SetRenderState(RenderState.ZEnable, false);
            Device.SetRenderState(RenderState.CullMode, Cull.None);
            Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            Device.SetTextureStageState(0, TextureStage.ColorArg0, TextureArgument.Texture);
            Device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Specular);
            Device.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
            Device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Lerp);
            Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
            Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.GaussianQuad);
            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.GaussianQuad);
            Device.SetTransform(TransformState.Projection, Matrix.OrthoOffCenterLH(0, 1360, 400, 0, -1, +1));
            Device.SetTransform(TransformState.View, Matrix.Translation(-0.5f, -0.5f, 0f));
            Device.SetTransform(TransformState.World, Matrix.Translation((int)-(0)/2 , (int)-(0)/2 , 0));
            Device.VertexFormat = Vertex.FVF;

            foreach (var fd in FontData)
                if (fd.Value.GlyphCount > 0)
                {
                    Device.Indices = fd.Value.IB;
                    Device.SetStreamSource(0, fd.Value.VB, 0, Vertex.Size);
                    Device.SetTexture(0, fd.Value.Texture);
                    Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, fd.Value.GlyphCount * 4, 0, fd.Value.GlyphCount * 2);
                }

            Device.EndScene();
            //Device.Present();
                Surface s = null;
                s = Device.GetBackBuffer(0, 0);
                Bitmap bitmap = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(s, SlimDX.Direct3D9.ImageFileFormat.Bmp));
                update(bitmap);
                s.Dispose();
            
            GC.Collect();
        }

      
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }



    }

}
