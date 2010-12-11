﻿// Copyright (c) 2010 Michael B. Edwin Rickert
//
// See the file LICENSE.txt for copying permission.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO; // Disambiguate from System.Drawing.Font
using System.Windows.Forms;
using Putty;
using ShinyConsole;
using SlimDX.Windows;
using Font = ShinyConsole.Font;

namespace TtyRecMonkey {
	[System.ComponentModel.DesignerCategory("")]
	class PlayerForm : BasicShinyConsoleForm<PlayerForm.Character> {
		public struct Character : IConsoleCharacter {
			public new uint Foreground, Background;
			public uint ActualForeground;// { get { return base.Foreground; } set { base.Foreground = value; }}
			public uint ActualBackground;// { get { return base.Background; } set { base.Background = value; }}
			public char Glyph;
			public Font Font;

			uint IConsoleCharacter.Foreground { get { return ActualForeground; }}
			uint IConsoleCharacter.Background { get { return ActualBackground; }}
			Font IConsoleCharacter.Font       { get { return Font; }}
			char IConsoleCharacter.Glyph      { get { return Glyph; }}
		}

		Point CursorPosition      = new Point(0,0);
		Point SavedCursorPosition = new Point(0,0);

		Character Prototype = new Character()
			{ Foreground = 0xFFFFFFFFu
			, Background = 0xFF000000u
			, Glyph      = ' '
			};

		public PlayerForm(): base(80,50) {
			Text = "TtyRecMonkey";

			GlyphSize = new Size(6,8);
			GlyphOverlap = new Size(1,1);
			FitWindowToMetrics();

			for ( int y=0 ; y<Height ; ++y )
			for ( int x=0 ; x<Width  ; ++x )
			{
				Buffer[x,y] = Prototype;
			}

			Visible = true;
			Configuration.Load(this);
			AfterConfiguration();
		}

		protected override void Dispose( bool disposing ) {
			if ( disposing ) {
				using ( Decoder ) {} Decoder = null;
			}
			base.Dispose(disposing);
		}

		public override void Redraw() {
			var now = DateTime.Now;
			int w=Width, h=Height;

			for ( int y=0 ; y<h ; ++y )
			for ( int x=0 ; x<w ; ++x )
			{
				//bool flipbg = (Mode.ScreenReverse!=Buffer[x,y].Reverse);
				//bool flipfg = (Buffer[x,y].Invisible || (Buffer[x,y].Blink && now.Millisecond<500) ) ? !flipbg : flipbg;
				bool flipbg=false, flipfg=false;

				Buffer[x,y].ActualForeground = flipfg ? Buffer[x,y].Background : Buffer[x,y].Foreground;
				Buffer[x,y].ActualBackground = flipbg ? Buffer[x,y].Foreground : Buffer[x,y].Background;
				Buffer[x,y].Font             = Prototype.Font;
			}

			base.Redraw();
		}

		void ResizeConsole( int w, int h ) {
			var newbuffer = new Character[w,h];

			var ow = Width;
			var oh = Height;

			for ( int y=0 ; y<h ; ++y )
			for ( int x=0 ; x<w ; ++x )
			{
				newbuffer[x,y] = (x<ow&&y<oh) ? Buffer[x,y] : Prototype;
			}

			Buffer = newbuffer;
		}

		void Reconfigure() {
			var cfg = new ConfigurationForm();
			if ( cfg.ShowDialog(this) == DialogResult.OK ) AfterConfiguration();
		}

		void AfterConfiguration() {
			bool resize = (WindowState==FormWindowState.Normal) && (ClientSize==ActiveSize);
			Prototype.Font = ShinyConsole.Font.FromBitmap( Configuration.Main.Font, Configuration.Main.Font.Width/16, Configuration.Main.Font.Height/16 );
			GlyphSize      = new Size( Configuration.Main.Font.Width/16, Configuration.Main.Font.Height/16 );
			GlyphOverlap   = new Size( Configuration.Main.FontOverlapX, Configuration.Main.FontOverlapY );
			ResizeConsole( Configuration.Main.DisplayConsoleSizeW, Configuration.Main.DisplayConsoleSizeH );

			if ( Decoder != null &&
				(  Configuration.Main.LogicalConsoleSizeW != Decoder.CurrentFrame.Data.GetLength(0)
				|| Configuration.Main.LogicalConsoleSizeH != Decoder.CurrentFrame.Data.GetLength(1)
				)
			){
				using ( Decoder ) {}
				Decoder = null;
				DecoderData.Position = 0;
				Decoder = new TtyRecKeyframeDecoder( Configuration.Main.LogicalConsoleSizeW, Configuration.Main.LogicalConsoleSizeH, DecoderData);
			}

			if ( resize ) ClientSize=ActiveSize;
		}

		Stream                DecoderData = null;
		TtyRecKeyframeDecoder Decoder = null;
		int PlaybackSpeed;
		TimeSpan Seek;
		readonly List<DateTime> PreviousFrames = new List<DateTime>();

		void OpenFile() {
			var open = new OpenFileDialog()
				{ CheckFileExists = true
				, DefaultExt = "ttyrec"
				, Filter = "TtyRec Files|*.ttyrec|All Files|*"
				, InitialDirectory = @"I:\home\media\ttyrecs\"
				, Multiselect = false
				, RestoreDirectory = true
				, Title = "Select a TtyRec to play"
				};
			if ( open.ShowDialog(this) != DialogResult.OK ) return;
			using ( DecoderData ) {}
			DecoderData = null;
			DecoderData = open.OpenFile();
			using ( open ) {} open = null;
			using ( Decoder ) {}
			Decoder = null;
			Decoder = new TtyRecKeyframeDecoder( Configuration.Main.LogicalConsoleSizeW, Configuration.Main.LogicalConsoleSizeH, DecoderData );
			PlaybackSpeed = +1;
			Seek = TimeSpan.Zero;
		}

		DateTime PreviousFrame = DateTime.Now;
		void MainLoop() {
			var now = DateTime.Now;

			PreviousFrames.Add(now);
			PreviousFrames.RemoveAll(f=>f.AddSeconds(1)<now);

			var dt = Math.Max( 0, Math.Min( 0.1, (now-PreviousFrame).TotalSeconds ) );
			PreviousFrame = now;

			Seek += TimeSpan.FromSeconds(dt*PlaybackSpeed);

			var BufferW = Buffer.GetLength(0);
			var BufferH = Buffer.GetLength(1);

			if ( Decoder != null ) {
				Decoder.Seek( Seek );

				var frame = Decoder.CurrentFrame.Data;
				if ( frame != null )
				for ( int y=0 ; y<BufferH ; ++y )
				for ( int x=0 ; x<BufferW ; ++x )
				{
					var ch = (x<frame.GetLength(0) && y<frame.GetLength(1)) ? frame[x,y] : default(TerminalCharacter);

					Buffer[x,y].Glyph      = ch.Character;
					Buffer[x,y].Foreground = Palette.Default[ ch.ForegroundPaletteIndex ];
					Buffer[x,y].Background = Palette.Default[ ch.BackgroundPaletteIndex ];
				}
			} else {
				var text = new[]
					{ "           PLACEHOLDER CONTROLS"
					, ""
					, "Ctrl+C     Reconfigure TtyRecMonkey"
					, "Ctrl+O     Open a ttyrec"
					, "Escape     Close ttyrec and return here"
					, "Alt+Enter  Toggle fullscreen"
					, ""
					, "ZXC        Play backwards at x100, x10, or x1 speed"
					, "   V       Pause (press a speed to unpause)"
					, "    BNM    Play forwards at 1x, x10, or x100 speed"
					, ""
					, " A / S     Zoom In/Out"
					};

				for ( int y=0 ; y<BufferH ; ++y )
				for ( int x=0 ; x<BufferW ; ++x )
				{
					var ch = (y<text.Length && x<text[y].Length) ? text[y][x] : ' ';

					Buffer[x,y].Glyph      = ch;
					Buffer[x,y].Foreground = 0xFFFFFFFFu;
					Buffer[x,y].Background = 0xFF000000u;
					Buffer[x,y].Font       = Prototype.Font;
				}
			}
			

			Text = string.Format
				( "TtyRecMonkey -- {0} FPS -- {1} @ {2} of {3} ({4} keyframes) -- Speed {5} -- GC recognized memory: {6}"
				, PreviousFrames.Count
				, PrettyTimeSpan( Seek )
				, Decoder==null ? "N/A" : PrettyTimeSpan( Decoder.CurrentFrame.SinceStart )
				, Decoder==null ? "N/A" : PrettyTimeSpan( Decoder.Length )
				, Decoder==null ? "N/A" : Decoder.Keyframes.ToString()
				, PlaybackSpeed
				, PrettyByteCount( GC.GetTotalMemory(false) )
				);
			Redraw();
		}

		protected override void OnKeyDown( KeyEventArgs e ) {
			bool resize = (WindowState==FormWindowState.Normal) && (ClientSize==ActiveSize);

			switch ( e.KeyData ) {

			case Keys.Escape: using ( Decoder ) {} Decoder = null; break;
			case Keys.Control|Keys.C: Reconfigure(); break;
			case Keys.Control|Keys.O: OpenFile(); break;
			case Keys.Alt|Keys.Enter:
				if ( FormBorderStyle == FormBorderStyle.None ) {
					FormBorderStyle = FormBorderStyle.Sizable;
					WindowState = FormWindowState.Normal;
				} else {
					FormBorderStyle = FormBorderStyle.None;
					WindowState = FormWindowState.Maximized;
				}
				e.SuppressKeyPress = true; // also supresses annoying dings
				break;

			case Keys.Z: PlaybackSpeed=-100; break;
			case Keys.X: PlaybackSpeed= -10; break;
			case Keys.C: PlaybackSpeed=  -1; break;
			case Keys.V: PlaybackSpeed=   0; break;
			case Keys.B: PlaybackSpeed=  +1; break;
			case Keys.N: PlaybackSpeed= +10; break;
			case Keys.M: PlaybackSpeed=+100; break;

			case Keys.A: ++Zoom; if (resize) ClientSize=ActiveSize; break;
			case Keys.S: if ( Zoom>1 ) --Zoom; if (resize) ClientSize=ActiveSize; break;

			}
			base.OnKeyDown(e);
		}

		static string PrettyTimeSpan( TimeSpan ts ) {
			return string.Format( "{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds );
		}

		static string PrettyByteCount( long bytes ) {
			if ( bytes<10000L ) return string.Format( "{0:0,0}B", bytes );
			bytes /= 1000;
			if ( bytes<10000L ) return string.Format( "{0:0,0}KB", bytes );
			bytes /= 1000;
			if ( bytes<10000L ) return string.Format( "{0:0,0}MB", bytes );
			bytes /= 1000;
			if ( bytes<10000L ) return string.Format( "{0:0,0}GB", bytes );
			bytes /= 1000;
			if ( bytes<10000L ) return string.Format( "{0:0,0}TB", bytes );
			bytes /= 1000;
			return string.Format( "{0:0,0}PB", bytes );
		}

		[STAThread] static void Main() {
			using ( var form = new PlayerForm() ) {
				MessagePump.Run( form, form.MainLoop );
			}
		}
	}
}