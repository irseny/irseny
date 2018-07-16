using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;

namespace Irseny.Tracap {
	public class Basic3PointCapTracker : SingleImageCapTracker {
		Basic3PointOptions options;
		Point2i[] visiblePoints;
		int visiblePointNo;
		Point2i[] clusterCenters;
		int clusterCenterNo;
		bool[,] suppressMap;
		bool[,] visibleMap;
		Point2i[] clusterPoints;
		Util.SharedRef<Emgu.CV.Mat> visibleOut = Util.SharedRef.Create(new Emgu.CV.Mat());

		Util.SharedRefCleaner imageCleaner = new Util.SharedRefCleaner(32);
		public Basic3PointCapTracker(Basic3PointOptions options) : base(options) {
			this.options = new Basic3PointOptions(options);
		}

		public override bool Start() {
			Running = true;
			visiblePoints = new Point2i[options.MaxPointNo];
			clusterCenters = new Point2i[options.MaxClusterNo];
			suppressMap = new bool[options.ApproximateImageSize.Height, options.ApproximateImageSize.Width];
			visibleMap = new bool[options.ApproximateImageSize.Height, options.ApproximateImageSize.Width];
			clusterPoints = new Point2i[options.MaxClusterPoints];
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
			ThresholdSource(image.Reference);
			FindClusters();
			MarkClusters();
			OnInputProcessed(new ImageProcessedEventArgs(visibleOut));
			var position = new CapPosition();
			OnPositionDetected(new PositionDetectedEventArgs(position));



			return true;
		}
		private void SetupStep(Emgu.CV.Mat image) {
			Emgu.CV.Mat result = visibleOut.Reference;
			int width = image.Width;
			int height = image.Height;
			int currentWidth = result.Width;
			int currentHeight = result.Height;
			if (result.Width != height || result.Height != width) {
				imageCleaner.AddReference(visibleOut);
				imageCleaner.CleanUpAll(); // executed very rarely
				visibleOut = Util.SharedRef.Create(new Emgu.CV.Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 1));
			}
			if (height > suppressMap.GetLength(0) || width > suppressMap.GetLength(1)) {
				suppressMap = new bool[height, width];
				visibleMap = new bool[height, width];
			}
			Array.Clear(suppressMap, 0, suppressMap.Length);
			// Array.Clear(visibleMap, 0, visibleMap.Length); // set below
			visiblePointNo = 0;
			clusterCenterNo = 0;
		}
		private void ThresholdSource(Emgu.CV.Mat image) {
			int width = image.Width;
			int height = image.Height;
			int stride = width;
			int length = width * height;
			int threshold = options.Threshold;
			IntPtr dataIn = image.DataPointer;
			IntPtr dataOut = visibleOut.Reference.DataPointer; // same size as source image
			for (int r = 0; r < height; r++) {
				for (int c = 0; c < width; c++) {
					byte brightness = Marshal.ReadByte(dataIn, r * stride + c);
					if (brightness > threshold) {
						if (visiblePointNo < visiblePoints.Length) {
							visiblePoints[visiblePointNo] = new Point2i(c, r);
							visiblePointNo += 1;
						}
						visibleMap[r, c] = true;
						Marshal.WriteByte(dataOut, r * stride + c, brightness);
					} else {
						visibleMap[r, c] = false;
						Marshal.WriteByte(dataOut, r * stride + c, 0);
					}

				}
			}
		}
		private void FindClusters() {
			for (int p = 0; p < visiblePointNo; p++) {
				var point = visiblePoints[p];
				if (!suppressMap[point.Y, point.X]) {
					Point2i clusterMid;
					Size2i clusterSize;
					int clusterEnergy;
					bool isCluster = DetectCluster(point, out clusterMid, out clusterSize, out clusterEnergy);
					if (isCluster) {
						if (clusterCenterNo < clusterCenters.Length) {
							clusterCenters[clusterCenterNo] = clusterMid;
							clusterCenterNo += 1;
						}
					}
				}
			}

		}
		private bool DetectCluster(Point2i start, out Point2i mid, out Size2i size, out int energy) {
			Emgu.CV.Mat source = visibleOut.Reference;
			int width = source.Width;
			int height = source.Height;
			int stride = width; // image stride
			int gapLimit = options.MaxClusterGap;
			int minStrideEnergy = options.MinStrideEnergy;
			int rBoundLow = height;
			int rBoundHigh = 0;
			int cBoundLow = width;
			int cBoundHigh = 0;
			int clusterPointNo = 0;
			IntPtr dataIn = visibleOut.Reference.DataPointer;

			int clusterEnergy = 0; // stride energy accumulator in negative and positive direction
			for (int mr = -1; mr <= 1; mr += 2) { // row multiplier -1 or 1
				int rGap = 0; // number of rows with low energy
				for (int r = start.Y; rGap < gapLimit && r > -1 && r < height; r += mr) { // current row
					int strideEnergy = 0; // energy of stride in negative and positive direction
					for (int mc = -1; mc <= 1; mc += 2) { // column multiplier
						int cGap = 0; // number of void points
						for (int c = start.X; cGap < gapLimit && c > -1 && c < width; c += mc) { // current column
							if (visibleMap[r, c] && !suppressMap[r, c]) {
								cGap = 0;
								// update stride information
								strideEnergy += 1;
								if (c < cBoundLow) {
									cBoundLow = c;
								} else if (c > cBoundHigh) {
									cBoundHigh = c;
								}
								// include in cluster
								if (clusterPointNo < clusterPoints.Length) {
									clusterPoints[clusterPointNo] = new Point2i(c, r);
									clusterPointNo += 1;
								}

							} else {
								cGap += 1;
							}
						}
					}
					if (strideEnergy < minStrideEnergy) {
						rGap += 1;
					} else {
						rGap = 0;
						clusterEnergy += strideEnergy; // only count stride if energy test is passed
						if (r < rBoundLow) { // update bounds
							rBoundLow = r;
						} else if (r > rBoundHigh) {
							rBoundHigh = r;
						}
					}

				}
			}
			mid = new Point2i((cBoundLow + cBoundHigh) / 2, (rBoundLow + rBoundHigh) / 2);
			size = new Size2i(cBoundHigh - cBoundLow, rBoundHigh - rBoundLow);
			energy = clusterEnergy;
			if (clusterEnergy >= options.MinClusterEnergy) {
				// suppress cluster points for further cluster detection
				for (int p = 0; p < clusterPointNo; p++) {
					suppressMap[clusterPoints[p].Y, clusterPoints[p].X] = true;
				}
				return true;
			} else {
				return false;
			}
		}
		private void MarkClusters() {
			Emgu.CV.Mat target = visibleOut.Reference;
			int width = target.Width;
			int height = target.Height;
			int stride = width;
			int crossRadius = 20;
			IntPtr dataOut = target.DataPointer;
			for (int m = 0; m < clusterCenterNo; m++) {
				Point2i center = clusterCenters[m];
				for (int r = center.Y - crossRadius; r < center.Y + crossRadius && r > -1 && r < height; r++) {
					Marshal.WriteByte(dataOut, r * stride + center.X, 255);
				}
				for (int c = center.X - crossRadius; c < center.X + crossRadius && c > -1 && c < width; c++) {
					Marshal.WriteByte(dataOut, center.Y * stride + c, 255);
				}
			}
		}
	}
	public static class CapTrackerOptionExtension {
		public static int MaxLines(this CapTrackerOptions options) {
			return (int)Math.Sqrt(options.MaxPointNo) + 1;
		}
		/*public static byte GetAt(this Emgu.CV.Mat matrix, int row, int column) {
			return Marshal.ReadByte(matrix.DataPointer, row * matrix.Width + column);
		}
		public static void SetAt(this Emgu.CV.Mat matrix, int row, int column, byte bright) {
			Marshal.WriteByte(matrix.DataPointer, row * matrix.Width + column, bright);
		}*/
	}
}
