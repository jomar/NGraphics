﻿using System;

using Foundation;
using AppKit;
using System.Linq;
using NGraphics.Test;
using System.IO;
using System.Threading.Tasks;

namespace NGraphics.Mac.Test
{
	public partial class MainWindowController : NSWindowController
	{
		public MainWindowController (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
		}

		public MainWindowController () : base ("MainWindow")
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
			RunTestsAsync ();
		}

		async Task RunTestsAsync ()
		{
			var sdir = System.IO.Path.GetDirectoryName (Environment.GetCommandLineArgs () [0]);
			while (Directory.GetFiles (sdir, "NGraphics.sln").Length == 0)
				sdir = System.IO.Path.GetDirectoryName (sdir);
			PlatformTest.ResultsDirectory = System.IO.Path.Combine (sdir, "TestResults");
			PlatformTest.Platform = Platforms.Current;
			PlatformTest.OpenStream = n =>
				new System.IO.FileStream (n, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			Environment.CurrentDirectory = PlatformTest.ResultsDirectory;

			var tat = typeof(NUnit.Framework.TestAttribute);
			var tfat = typeof(NUnit.Framework.TestFixtureAttribute);

			var types = typeof (DrawingTest).Assembly.GetTypes ();
			var tfts = types.Where (t => t.GetCustomAttributes (tfat, false).Length > 0);

			foreach (var t in tfts) {
				var test = Activator.CreateInstance (t);
				var ms = t.GetMethods ().Where (m => m.GetCustomAttributes (tat, true).Length > 0);
				foreach (var m in ms) {
					try {
						var r = m.Invoke (test, null);
						var ta = r as Task;
						if (ta != null)
							await ta;
					}
					catch (Exception ex) {
						Console.WriteLine ("TEST {0} ERROR", m.Name);
						Console.WriteLine (ex);
					}
				}
			}

			NSApplication.SharedApplication.Terminate (this);
		}

		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}
	}
}
