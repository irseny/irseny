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

		public KeypointDetector(IKeypointDetectorOptions options) {
			if (options == null) throw new ArgumentNullException("options");
			this.options = options;
		}

		public void Process(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			Setup(imageIn, imageOut);
			Threshold(imageIn, imageOut);
			FindClusters();
			MarkClusters(imageOut);
		}
		private void Setup(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			if (imageIn.ElementSize != sizeof(byte)) throw new ArgumentException("imageIn: Wrong pixel format");
			if (imageOut.ElementSize != sizeof(byte)) throw new ArgumentException("imageOut: Wrong pixel format");
			if (!imageOut.Size.Equals(imageIn.Size)) throw new ArgumentException("imageOut: Size differs from imageIn");
			// buffers
			if (suppressionMap.Length < imageIn.Width*imageIn.Height) {
				suppressionMap = new bool[imageIn.Width*imageIn.Height];
			}
			if (visibilityMap.Length < imageIn.Width*imageIn.Height) {
				visibilityMap = new bool[imageIn.Width*imageIn.Height];
			}
			Array.Clear(suppressionMap, 0, suppressionMap.Length);
			// counters
			imagePointNo = 0;
			clusterPointNo = 0;
			clusterCenterNo = 0;

		}
		private void Threshold(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			int width = imageIn.Width;
			int height = imageIn.Height;
			int stride = width;
			int threshold = options.BrightnessThreshold;
			IntPtr bufferIn = imageIn.DataPointer;
			IntPtr bufferOut = imageOut.DataPointer;
			for (int r = 0; r < height; r++) {
				for (int c = 0; c < width; c++) {
					byte bright = Marshal.ReadByte(bufferIn, r*stride + c);
					if (bright < threshold) {
						visibilityMap[r*stride + c] = true;
						if (imagePointNo < imagePoints.Length) {
							imagePoints[imagePointNo] = new Point2i(c, r);
							imagePointNo += 1;
						}
						Marshal.WriteByte(bufferOut, r*stride + c, bright);
					} else {
						visibilityMap[r*stride + c] = false;
						Marshal.WriteByte(bufferOut, r*stride + c, 0);
					}
				}
			}
		}
		private void FindClusters() {
			
		}
		private void MarkClusters(Emgu.CV.Mat imageOut) {
			
		}
	}
}

