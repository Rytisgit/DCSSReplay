// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Putty
{
    public class Terminal : IDisposable
    {
        IntPtr Handle;

        [DllImport(@"libPuttyDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "CreatePuttyTerminal")] 
        static extern IntPtr CreatePuttyTerminal(int width, int height);
        [DllImport(@"libPuttyDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "DestroyPuttyTerminal")] 
        static extern void DestroyPuttyTerminal(IntPtr terminal);
        [DllImport(@"libPuttyDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "SendPuttyTerminal")] 
        unsafe static extern void SendPuttyTerminal(IntPtr terminal, int stderr, byte* data, int length);
        [DllImport(@"libPuttyDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "GetPuttyTerminalLine")] 
        unsafe static extern TerminalCharacter* GetPuttyTerminalLine(IntPtr terminal, int y);
        [DllImport(@"libPuttyDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "ClonePuttyTerminal")] 
        static extern IntPtr ClonePuttyTerminal(IntPtr terminal);
        [DllImport(@"libPuttyDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "GetPuttyTerminalWidth")] 
        static extern int GetPuttyTerminalWidth(IntPtr terminal);
        [DllImport(@"libPuttyDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "GetPuttyTerminalHeight")] 
        static extern int GetPuttyTerminalHeight(IntPtr terminal);

        /// <summary>
        /// Create a new PuTTY terminal
        /// </summary>
        /// <param name="width">Width of the newly created terminal</param>
        /// <param name="height">Height of the newly created terminal</param>
        public Terminal(int width, int height)
        {
            Handle = CreatePuttyTerminal(width, height);
            Width = width;
            Height = height;
            var env = Environment.GetEnvironmentVariable("ISDOCKER");
            DoubleSpace = env == null ? false : env.Equals("DoubleSpace", StringComparison.InvariantCultureIgnoreCase) ;
            //on docker linux the puttydll includes a space without a character in each row, so we skip them
        }

        public bool DoubleSpace { get; set; }

        /// <summary>
        /// Create a copy of an existing PuTTY terminal, including PuTTY's parse state
        /// </summary>
        /// <param name="toclone">The terminal to make a copy of</param>
        public Terminal(Terminal toclone)
        {
            Handle = ClonePuttyTerminal(toclone.Handle);
            Width = toclone.Width;
            Height = toclone.Height;
            DoubleSpace = toclone.DoubleSpace;

        }

        /// <summary>
        /// Deallocate the memory associated with this PuTTY terminal
        /// </summary>
        public void Dispose()
        {
            if(!Disposed) DestroyPuttyTerminal(Handle);   
            Handle = IntPtr.Zero;
        }

        /// <summary>
        /// Has the terminal already been disposed?
        /// </summary>
        public bool Disposed
        {
            get
            {
                return Handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// The width or number of columns in this Terminal
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height or number of rows in this Terminal
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Send data (text, escape codes, etc) to be processed by PuTTY
        /// </summary>
        /// <param name="payload">Data (text, escape codes, etc) to be processed by PuTTY</param>
        public unsafe void Send(byte[] payload)
        {
            if (Disposed) throw new ObjectDisposedException("This terminal has already been disposed");
            fixed (byte* pinned = payload) SendPuttyTerminal(Handle, 0, pinned, payload.Length);
            Debug.Assert(Width == GetPuttyTerminalWidth(Handle));
            Debug.Assert(Height == GetPuttyTerminalHeight(Handle));
        }

        /// <summary>
        /// Get a line or row of text from the Terminal
        /// </summary>
        /// <param name="row">Row index or Y-coordinate of the row of text to get</param>
        /// <returns>A line of terminal text</returns>
        public unsafe IList<TerminalCharacter> GetLine(int row)
        {
            if (Disposed) throw new ObjectDisposedException("This terminal has already been disposed");
            if (row < 0 || Height <= row) throw new ArgumentException("Row outside of terminal boundary", "row");

            var w = Width;
            var buffer = new List<TerminalCharacter>(w);
            var src = GetPuttyTerminalLine(Handle, row);
            if(DoubleSpace) for (int x = 0; x < w * 2; x += 2) buffer.Add(src[x]);
            else for(int x = 0; x < w; ++x) buffer.Add(src[x]);
            Debug.Assert(buffer.Count == w);
            return buffer.AsReadOnly();
        }
    }
}
