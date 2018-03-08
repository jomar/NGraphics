﻿using System;
using System.Linq;
using System.Collections.Generic;

using Foundation;
using UIKit;
using NGraphics.Test;

namespace NGraphics.iOS.Test
{
	[Register ("UnitTestAppDelegate")]
	public partial class UnitTestAppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			System.Threading.ThreadPool.QueueUserWorkItem (_ => {
				var tat = typeof (NUnit.Framework.TestAttribute);
				var tfat = typeof (NUnit.Framework.TestFixtureAttribute);

				var types = typeof (DrawingTest).Assembly.GetTypes ();
				var tfts = types.Where (t => t.GetCustomAttributes (tfat, false).Length > 0);

				PlatformTest.ResultsDirectory = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("NGraphicsDir"), "TestResults");
				PlatformTest.Platform = Platforms.Current;
				Environment.CurrentDirectory = PlatformTest.ResultsDirectory;

				foreach (var t in tfts) {
					var test = Activator.CreateInstance (t);
					var ms = t.GetMethods ().Where (m => m.GetCustomAttributes (tat, true).Length > 0);
					foreach (var m in ms) {
						m.Invoke (test, null);
					}
				}
			});

			Window = new UIWindow (UIScreen.MainScreen.Bounds);
			Window.RootViewController = new UIViewController ();
			Window.MakeKeyAndVisible ();

			return true;
		}
	}
}

