using System;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;

namespace Irseny.Tracap {
	public class KeypointDetector {
		IKeypointDetectorOptions options;
		Point2i[] imagePoints = new Point2i[0];
		int imagePointNo;
		Point2i[] clusterPoints = new Point2i[0];
		int clusterPointNo;
		Point2i[] clusterCenters = new Point2i[0];
		int clusterCenterNo;
		bool[] suppressionMap = new bool[0];
		bool[] visibilityMap = new bool[0];
		int imageWidth;
		int imageHeight;
		int imageStride;

		public KeypointDetector(IKeypointDetectorOptions options) {
			if (options == null) throw new ArgumentNullException("options");
			this.options = options;
		}

		public void Process(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			Setup(imageIn, imageOut);
			Threshold(imageIn, imageOut);
			FindClusters(imageIn);
			if (options.MarkClusters) {
				MarkClusters(imageOut);
			}
		}
		private void Setup(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			if (imageIn.ElementSize != sizeof(byte)) throw new ArgumentException("imageIn: Wrong pixel format");
			if (imageOut.ElementSize != sizeof(byte)) throw new ArgumentException("imageOut: Wrong pixel format");
			if (!imageOut.Size.Equals(imageIn.Size)) throw new ArgumentException("imageOut: Size differs from imageIn");
			imageWidth = imageIn.Width;
			imageHeight = imageIn.Height;
			imageStride = imageWidth;
			// buffers
			if (imagePoints.Length < options.MaxPointNo) {
				imagePoints = new Point2i[options.MaxPointNo];
			}
			if (clusterPoints.Length < options.MaxClusterPointNo) {
				clusterPoints = new Point2i[options.MaxClusterPointNo];
			}
			if (clusterCenters.Length < options.MaxClusterNo) {
				clusterCenters = new Point2i[options.MaxClusterNo];
			}
			if (suppressionMap.Length < imageWidth*imageHeight) {
				suppressionMap = new bool[imageWidth*imageHeight];
			}
			if (visibilityMap.Length < imageWidth*imageHeight) {
				visibilityMap = new bool[imageWidth*imageHeight];
			}
			Array.Clear(suppressionMap, 0, suppressionMap.Length);
			// counters
			imagePointNo = 0;
			clusterPointNo = 0;
			clusterCenterNo = 0;

		}
		private void Threshold(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {			
			int threshold = options.BrightnessThreshold;
			IntPtr bufferIn = imageIn.DataPointer;
			IntPtr bufferOut = imageOut.DataPointer;
			for (int r = 0; r < imageHeight; r++) {
				for (int c = 0; c < imageWidth; c++) {
					byte bright = Marshal.ReadByte(bufferIn, r*imageStride + c);
					if (bright >= threshold) {
						visibilityMap[r*imageStride + c] = true;
						if (imagePointNo < imagePoints.Length) {
							imagePoints[imagePointNo] = new Point2i(c, r);
							imagePointNo += 1;
						}
						Marshal.WriteByte(bufferOut, r*imageStride + c, bright);
					} else {
						visibilityMap[r*imageStride + c] = false;
						Marshal.WriteByte(bufferOut, r*imageStride + c, 0);
					}
				}
			}
		}
		private void FindClusters(Emgu.CV.Mat imageIn) {
			int stride = imageIn.Width;
			for (int p = 0; p < imagePointNo; p++) {
				Point2i point = imagePoints[p];
				if (!suppressionMap[point.Y*stride + point.X]) {
					int clusterRadius;
					Point2i clusterCenter;
					bool isCluster = DetectCluster(point, out clusterCenter, out clusterRadius);
					if (isCluster) {
						if (clusterCenterNo < clusterCenters.Length) {
							clusterCenters[clusterCenterNo] = clusterCenter;
							clusterCenterNo += 1;
						}
					}
				}
			}
		}
		private bool DetectCluster(Point2i start, out Point2i center, out int radius) {
			int minLayerEnergy = options.MinLayerEnergy;
			Point2i averageClusterPoint = new Point2i(0, 0);
			int layerNo;
			for (layerNo = 1;; layerNo += 1) {
				if (start.X - layerNo < 0 || start.X + layerNo >= imageWidth || start.Y - layerNo < 0 || start.Y >= imageHeight) {
					break; // eventually end loop as an image border is encountered
				}
				int layerEnergy = 0;
				// evaluate horizontal edge points
				for (int dr = -layerNo; dr <= layerNo; dr += layerNo*2) { // -radius and +radius
					int r = start.Y + dr;
					for (int dc = -layerNo; dc <= layerNo; dc++) { // all horizontal edge points
						// main horizontal points
						int c = start.X + dc;
						if (visibilityMap[r*imageStride + c] && !suppressionMap[r*imageStride + c]) {
							layerEnergy += 1;
							if (clusterPointNo < clusterPoints.Length) {
								clusterPoints[clusterPointNo] = new Point2i(c, r);
								clusterPointNo += 1;
								averageClusterPoint.X += c;
								averageClusterPoint.Y += r;
							}
						}

					}
				}
				// evaluate vertical edge points
				// better cache usage if evaluation separated
				for (int dc = -layerNo; dc <= layerNo; dc += layerNo*2) {
					int c = start.X + dc;
					for (int dr = -layerNo + 1; dr < layerNo; dr++) { // corner points included above
						int r = start.Y + dr;
						if (visibilityMap[r*imageStride + c] && !suppressionMap[r*imageStride + c]) {
							layerEnergy += 1;
							if (clusterPointNo < clusterPoints.Length) {
								clusterPoints[clusterPointNo] = new Point2i(c, r);
								clusterPointNo += 1;
								averageClusterPoint.X += c;
								averageClusterPoint.Y += r;
							}
						}
					}
				}
				if (layerEnergy < minLayerEnergy) {
					// finished if no more layers can be applied
					break;
				}
				// TODO: break on layer boundary pass
			}
			radius = layerNo;
			if (clusterPointNo > 0) {
				center = new Point2i(averageClusterPoint.X/clusterPointNo, averageClusterPoint.Y/clusterPointNo);
			} else {
				center = new Point2i(0, 0);
			}
			// check for size constraints
			if (layerNo < options.MinClusterRadius) {
				return false;
			}
			if (layerNo > options.MaxClusterRadius) {
				return false;
			}
			// suppress further point usage
			for (int p = 0; p < clusterPointNo; p++) {
				Point2i point = clusterPoints[p];
				suppressionMap[point.Y*imageStride + point.X] = true;
			}

			return true;
		}
		private void MarkClusters(Emgu.CV.Mat imageOut) {
			int radius = 10;
			IntPtr dataOut = imageOut.DataPointer;
			for (int i = 0; i < clusterCenterNo; i++) {
				Point2i center = clusterCenters[i];
				int rLow = Math.Max(0, center.Y - radius);
				int rHigh = Math.Min(imageHeight, center.Y + radius);
				for (int r = rLow; r < rHigh; r++) {
					Marshal.WriteByte(dataOut, r*imageStride + center.X, 255);
				}
				int cLow = Math.Max(0, center.X - radius);
				int cHigh = Math.Min(imageWidth, center.X + radius);
				for (int c = cLow; c < cHigh; c++) {
					Marshal.WriteByte(dataOut, center.Y*imageStride + c, 255);
				}
			}
		}
	}
}

