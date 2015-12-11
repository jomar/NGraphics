using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using System.Threading.Tasks;
using CoreGraphics;

namespace NGraphics.Editor
{
	public partial class Preview : AppKit.NSView
	{
		#region Constructors

		// Called when created from unmanaged code
		public Preview (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public Preview (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
			System.Diagnostics.Debug.WriteLine("Initializing platform: " + Platforms.Current.Name);
		}

		#endregion

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public Action ImageRenderedDelegate { get; set; }

		public IDrawable[] Drawables;

		LinearGradientBrush backBrush = new LinearGradientBrush (
            Point.Zero, Point.OneY,
			new Color (0.99),
			new Color (0.93));

		private IImage renderedContent;
		public TimeSpan RenderTime { get; set; }
		public string Error { get; set; }

		public override void SetNeedsDisplayInRect(CGRect rect)
		{
			Task.Run(() =>
			{
				Size canvasSize = Size.Zero;
				this.InvokeOnMainThread(() => {
					canvasSize = new Size(this.Bounds.Width, this.Bounds.Height);
				});

				RenderTime = TimeSpan.Zero;;
				var start = DateTime.Now;

				try
				{
					var canvas = Platforms.Current.CreateImageCanvas(canvasSize);
					canvas.FillRectangle (new Rect(canvas.Size), backBrush);

					var ds = Drawables;
					if (ds == null || ds.Length == 0)
						return;

					foreach (var d in ds) {
						try {
							d.Draw (canvas);
						} catch (Exception ex) {
							Console.WriteLine (ex);
						}
					}
					renderedContent = canvas.GetImage();
				}
				catch (Exception ex)
				{
					Error = "Error while rendering content: " + ex.Message;
					System.Diagnostics.Debug.WriteLine(Error);
				}
				finally
				{
					this.InvokeOnMainThread(() => base.SetNeedsDisplayInRect(rect));

					RenderTime = DateTime.Now - start;

					if (ImageRenderedDelegate != null)
						ImageRenderedDelegate.Invoke();
				}
			});
		}

		public override void DrawRect (CoreGraphics.CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);

			if (renderedContent != null)
			{
				var previewCanvas = new NGraphics.CGContextCanvas (NSGraphicsContext.CurrentContext.CGContext);
				previewCanvas.DrawImage(renderedContent);
			}
		}
	}
}
