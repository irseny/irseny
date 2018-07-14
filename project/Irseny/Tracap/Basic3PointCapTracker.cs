using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;

namespace Irseny.Tracap {
	public class Basic3PointCapTracker : SingleImageCapTracker {
		CapTrackerOptions options;
		List<int> hLines;
		List<int> vLines;
		List<Point2i> visiblePoints;
		List<Point2i> midPoints;
		Util.SharedRef<Emgu.CV.Mat> visibleOut = Util.SharedRef.Create(new Emgu.CV.Mat());

		Util.SharedRefCleaner imageCleaner = new Util.SharedRefCleaner(32);
		public Basic3PointCapTracker(CapTrackerOptions options) : base() {
			this.options = new CapTrackerOptions(options);
		}

		public override bool Start() {
			Running = true;
			hLines = new List<int>(options.MaxLines());
			vLines = new List<int>(options.MaxLines());
			visiblePoints = new List<Point2i>(options.PointBufferLength);
			midPoints = new List<Point2i>(options.MaxLights);
			return true;
		}

		public override bool Stop() {
			Running = false;
			imageCleaner.CleanUpAll(); // might leave some images left
			return true;
		}
		public override void Dispose() {
			imageCleaner.DisposeAll(); // should not matter if some images are disposed on non detection threads
			base.Dispose();
		}
		protected override bool Step(Util.SharedRef<Emgu.CV.Mat> image) {
			SetupStep(image.Reference);
			Threshold(image.Reference);
			OnInputProcessed(new ImageProcessedEventArgs(visibleOut));
			var position = new CapPosition();
			OnPositionDetected(new PositionDetectedEventArgs(position));


			//imageCleaner.AddReference(result);
			return true;
		}
		private void SetupStep(Emgu.CV.Mat image) {
			Emgu.CV.Mat result = visibleOut.Reference;
			int width = image.Width;
			int height = image.Height;
			if (result.Width != height || result.Height != width) {
				imageCleaner.AddReference(visibleOut);
				imageCleaner.CleanUpAll(); // executed very rarely
				visibleOut = Util.SharedRef.Create(new Emgu.CV.Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 1));
			}
			hLines.Clear();
			vLines.Clear();
			visiblePoints.Clear();
			midPoints.Clear();
		}
		private void Threshold(Emgu.CV.Mat image) {
			Emgu.CV.Mat result = visibleOut.Reference; // has same size
			int width = image.Width;
			int height = image.Height;
			int stride = width;
			int length = width * height;
			int threshold = options.Threshold;
			for (int c = 0; c < width; c++) {
				for (int r = 0; r < height; r++) {
					byte pixel = image.GetAt(r, c);
					if (pixel > threshold) {
						result.SetAt(r, c, pixel);
					} else {
						result.SetAt(r, c, 0);
					}

				}
			}
		}
		private int GetMatAt(int row, int column, int stride) {
			return row * stride + column;
		}

	}
	public static class CapTrackerOptionExtension {
		public static int MaxLines(this CapTrackerOptions options) {
			return (int)Math.Sqrt(options.PointBufferLength) + 1;
		}
		public static byte GetAt(this Emgu.CV.Mat matrix, int row, int column) {
			return Marshal.ReadByte(matrix.DataPointer, row * matrix.Width + column);
		}
		public static void SetAt(this Emgu.CV.Mat matrix, int row, int column, byte bright) {
			Marshal.WriteByte(matrix.DataPointer, row * matrix.Width + column, bright);
		}
	}
}
