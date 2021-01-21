using System;
using System.Runtime.InteropServices;
using Irseny.Core.Util;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;

namespace Irseny.Core.Tracking {
	public class KeypointDetector {
		EquipmentSettings settings;
		Point2i[] imagePoints = new Point2i[0];
		int imagePointNo;
		Point2i[] clusterMembers = new Point2i[0];
		Point2i[] clusterCenters = new Point2i[0];
		int clusterCenterNo;
		bool[] suppressionMap = new bool[0];
		bool[] visibilityMap = new bool[0];
		int imageWidth;
		int imageHeight;
		int imageStride;

		public KeypointDetector(EquipmentSettings settings) {
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
			FindClusters(imageIn, imageOut);

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
			if (clusterMembers.Length < maxClusterMembers) {
				clusterMembers = new Point2i[maxClusterMembers];
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
						Marshal.WriteByte(bufferOut, r*imageStride + c, 64);
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
		private void FindClusters(Emgu.CV.Mat imageIn, Emgu.CV.Mat imageOut) {
			// try with cluster origins at all bright points in the image
			for (int p = 0; p < imagePointNo; p++) {
				Point2i point = imagePoints[p];
				if (!suppressionMap[point.Y*imageStride + point.X]) {
					int clusterRadius;
					Point2i clusterCenter;
					bool terminate;
					bool found = DetectCluster(point, out clusterCenter, out clusterRadius, out terminate, imageOut);
					// terminate if not successful
					if (terminate) {
						clusterCenterNo = 0;
						return;
					}
					// add cluster if satisfactory
					if (found) {
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
		/// <prama name="critical">Critical error.</prama>
		private bool DetectCluster(Point2i start, out Point2i center, out int radius, out bool critical, Emgu.CV.Mat imageOut) {
			// initialization to be able to return at any point
			int minClusterRadius = settings.GetInteger(TrackerProperty.MinClusterRadius, 2);
			int maxClusterRadius = settings.GetInteger(TrackerProperty.MaxClusterRadius, 32);
			int minLayerEnergy = settings.GetInteger(TrackerProperty.MinLayerEnergy, 32);
			IntPtr bufferOut = imageOut.DataPointer;
			center = new Point2i(-1, -1);
			radius = -1;
			critical = false;
			clusterMembers[0] = start;
			int clusterMemberNo = 1;
			suppressionMap[start.Y*imageStride + start.X] = true;
			// process all cluster members
			for (int clusterProgress = 0; clusterProgress < clusterMemberNo; clusterProgress += 1) {
				Point2i point = clusterMembers[clusterProgress];
				// ignore edge points
				// do not read outside image bounds
				if (point.X <= 0 || point.X + 1 >= imageWidth) {
					continue;
				}
				if (point.Y <= 0 || point.Y + 1 >= imageHeight) {
					continue;
				}
				// terminate if the cluster grows too big
				if (clusterMemberNo + 4 >= clusterMembers.Length) {
					critical = true;
					return false;
				}
				// queue bright neighbors
				for (int d = -1; d <= 1; d += 2) {
					// evaluate horizontal and vertical neighbors separately
					int c = point.X + d;
					if (visibilityMap[point.Y*imageStride + c] && !suppressionMap[point.Y*imageStride + c]) {
						// mark as cluster member
						clusterMembers[clusterMemberNo] = new Point2i(c, point.Y);
						clusterMemberNo += 1;
						suppressionMap[point.Y*imageStride + c] = true;
						Marshal.WriteByte(bufferOut, point.Y*imageStride + c, 146);
					}
					int r = point.Y + d;
					if (visibilityMap[r*imageStride + point.X] && !suppressionMap[r*imageStride + point.X]) {
						// mark as cluster member
						clusterMembers[clusterMemberNo] = new Point2i(point.X, r);
						clusterMemberNo += 1;
						suppressionMap[r*imageStride + point.X] = true;
						Marshal.WriteByte(bufferOut, r*imageStride + point.X, 146);
					}
				}

			}
			if (clusterMemberNo < minLayerEnergy) {
				critical = false;
				return false;
			}
			// find cluster center
			// weighted sum of members
			var clusterSum = new Point2i(0, 0);
			for (int i = 0; i < clusterMemberNo; i++) {
				clusterSum.X += clusterMembers[i].X;
				clusterSum.Y += clusterMembers[i].Y;
			}
			center = new Point2i(clusterSum.X/clusterMemberNo, clusterSum.Y/clusterMemberNo);
			// find cluster radius
			// average deviation from center
			var clusterDeviation = new Point2i(0, 0);
			for (int i = 0; i < clusterMemberNo; i++) {
				clusterDeviation.X += Math.Abs(clusterMembers[i].X - center.X);
				clusterDeviation.Y += Math.Abs(clusterMembers[i].Y - center.Y);
			}
			// with increasing size we get a tendency to high deviation
			// as there are can exist more points further away from the center
			// TODO: evaluate how to get a good radius estimate (at which radius are the most points located)
			var clusterRadius = new Point2i((int)(clusterDeviation.X/clusterMemberNo*1.4f), (int)(clusterDeviation.Y/clusterMemberNo*1.4f));
			radius = Math.Max(clusterRadius.X, clusterRadius.Y);
			if (clusterRadius.X < minClusterRadius || clusterRadius.X > maxClusterRadius) {
				return false;
			}
			if (clusterRadius.Y < minClusterRadius || clusterRadius.Y > maxClusterRadius) {
				return false;
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

