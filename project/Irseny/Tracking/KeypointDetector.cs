using System;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;

namespace Irseny.Tracking {
	public class KeypointDetector {
		TrackerSettings settings;
		Point2i[] imagePoints = new Point2i[0];
		int imagePointNo;
		Point2i[] clusterPoints = new Point2i[0];
		Point2i[] clusterCenters = new Point2i[0];
		int clusterCenterNo;
		bool[] suppressionMap = new bool[0];
		bool[] visibilityMap = new bool[0];
		int imageWidth;
		int imageHeight;
		int imageStride;

		public KeypointDetector(TrackerSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			this.settings = settings;
		}
		/// <summary>
		/// Detects keypoints.
		/// </summary>
		/// <returns>The detect.</returns>
		/// <param name="imageIn">Input Image.</param>
		/// <param name="imageOut">Output Image.</param>
		/// <param name="keypoints">Detected keypoints.</param>
		public int Detect(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut, out Point2i[] keypoints) {
			Setup(imageIn, imageOut);
			Threshold(imageIn, imageOut);
			FindClusters(imageIn);

			MarkClusters(imageOut);

			keypoints = clusterCenters;
			return clusterCenterNo;
		}
		/// <summary>
		/// Performs the setup steps for the next iteration.
		/// </summary>
		/// <param name="imageIn">Input Image.</param>
		/// <param name="imageOut">Output Image.</param>
		private void Setup(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			if (imageIn.ElementSize != sizeof(byte)) throw new ArgumentException("imageIn: Wrong pixel format");
			if (imageOut.ElementSize != sizeof(byte)) throw new ArgumentException("imageOut: Wrong pixel format");
			if (!imageOut.Size.Equals(imageIn.Size)) throw new ArgumentException("imageOut: Size differs from imageIn");
			imageWidth = imageIn.Width;
			imageHeight = imageIn.Height;
			imageStride = imageWidth;
			// buffers
			int maxPointNo = settings.GetInteger(TrackerProperty.MaxPointNo, 1024);
			if (imagePoints.Length < maxPointNo) {
				imagePoints = new Point2i[maxPointNo];
			}
			int maxClusterMembers = settings.GetInteger(TrackerProperty.MaxClusterMembers, 512);
			if (clusterPoints.Length < maxClusterMembers) {
				clusterPoints = new Point2i[maxClusterMembers];
			}
			int maxClusterNo = settings.GetInteger(TrackerProperty.MaxClusterNo, 16);
			if (clusterCenters.Length < maxClusterNo) {
				clusterCenters = new Point2i[maxClusterNo];
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
			clusterCenterNo = 0;

		}
		/// <summary>
		/// Apply thresholding to extract bright spots within the given image.
		/// </summary>
		/// <param name="imageIn">Input image.</param>
		/// <param name="imageOut">Output image.</param>
		private void Threshold(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			int threshold = settings.GetInteger(TrackerProperty.MinBrightness, 32);
			// assumes that one pixel is represented with one byte
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
		/// <summary>
		/// Finds clusters within the given image.
		/// </summary>
		/// <param name="imageIn">Input image.</param>
		private void FindClusters(Emgu.CV.Mat imageIn) {
			int stride = imageIn.Width;
			// try with cluster origins at all bright points in the image
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
		/// <summary>
		/// Detects one cluster in the image starting from a given position.
		/// </summary>
		/// <returns><c>true</c>, if a cluster was detected, <c>false</c> otherwise.</returns>
		/// <param name="start">Potential cluster start.</param>
		/// <param name="center">Cluster center.</param>
		/// <param name="radius">Cluster radius.</param>
		private bool DetectCluster(Point2i start, out Point2i center, out int radius) {
			// initialization to be able to return at any point
			int minClusterRadius = settings.GetInteger(TrackerProperty.MinClusterRadius, 2);
			int maxClusterRadius = settings.GetInteger(TrackerProperty.MinClusterRadius, 32);
			int minLayerEnergy = settings.GetInteger(TrackerProperty.MinLayerEnergy, 6);
			radius = 0;
			center = new Point2i(-1, -1);
			// determine points that are members of the cluster
			// and find the center within the members
			Point2i averageClusterPoint = new Point2i(0, 0);
			int clusterPointNo = 0;
			// iteratively find cluster bounds and points within the bounds
			// starting from the smalles try all radiuses
			// stop when the point population on a layer becomes too thin
			// visually one layer is the edge of a quadratic region
			int layerNo;
			for (layerNo = 1; layerNo < maxClusterRadius; layerNo += 1) {
				if (start.X - layerNo < 0 || start.X + layerNo >= imageWidth || start.Y - layerNo < 0 || start.Y + layerNo >= imageHeight) {
					// skip if we trip an image border

					// we get multiple clusters at edges if not omitted
					// eventually end loop as an image border is encountered
					// TODO: do evaluation on a pixel level to allow for better edge cluster detection
					return false;
				}
				// calculate the layer energy and additional cluster points
				// perform the search with a rectangular shape
				int layerEnergy = 0;
				// determine the points on the edges that connect the end points of the left and right edges
				// and calculate the layer energy
				for (int dr = -layerNo; dr <= layerNo; dr += layerNo*2) { // -radius and +radius
					int r = start.Y + dr;
					// search through all columns within the radius range
					for (int dc = -layerNo; dc <= layerNo; dc++) {
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
				// determine the points on the edges that connect the end points of the top and bottom edges
				// and calculate the layer energy
				for (int dc = -layerNo; dc <= layerNo; dc += layerNo*2) {
					int c = start.X + dc;
					// search through all rows within radius range
					for (int dr = -layerNo + 1; dr < layerNo; dr++) {
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
				return false;
			}
			// check for size constraints
			if (layerNo < minClusterRadius) {
				return false;
			}
			if (layerNo > maxClusterRadius) {
				// TODO: let the layer number grow beyond the cluster radius ?
				return false;
			}
			// suppress further point usage for succeeding clusters
			for (int p = 0; p < clusterPointNo; p++) {
				Point2i point = clusterPoints[p];
				suppressionMap[point.Y*imageStride + point.X] = true;
			}
			return true;
		}
		/// <summary>
		/// Marks detected clusters visually.
		/// </summary>
		/// <param name="imageOut">Output image.</param>
		private void MarkClusters(Emgu.CV.Mat imageOut) {
			int radius = 10;
			IntPtr dataOut = imageOut.DataPointer;
			for (int i = 0; i < clusterCenterNo; i++) {
				Point2i center = clusterCenters[i];
				// draws a cross at the cluster center
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

