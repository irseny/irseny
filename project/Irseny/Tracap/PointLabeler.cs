using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Point2i = System.Drawing.Point;
using Size2i = System.Drawing.Size;

namespace Irseny.Tracap {
	public partial class PointLabeler {
		IPointLabelerOptions options;
		Point2i[] inPoints;
		Point2i[] lastPoints;
		int[] lastLabels;
		int inPointNo;

		int[] outLabels;
		Point2i[] outPoints;
		int outPointNo;

		int[][] permutations;
		int permutationPointNo;
		bool relabel;



		public PointLabeler(IPointLabelerOptions options) {
			if (options == null) throw new ArgumentNullException("options");
			this.options = options;
			this.lastLabels = new int[0];
			this.lastPoints = new Point2i[0];
			this.inPoints = new Point2i[0];
			this.outPoints = new Point2i[0];
			this.outLabels = new int[0];
			this.permutations = new int[0][];
			this.permutationPointNo = 0;
		}

		public int Label(Point2i[] points, int pointNo, Emgu.CV.Mat imageOut, out int[] labels) {
			if (!Setup(points, pointNo)) {
				labels = lastLabels;
				return 0;
			}
			if (relabel) {
				AssignLabels();
			} else {
				MinmizeDistance();
			}
			if (options.ShowLabels) {
				ShowLabels(imageOut);
			}
			labels = outLabels;
			return outPointNo;
		}
		public bool Setup(Point2i[] points, int pointNo) {
			if (pointNo < 1) {
				return false; // nothing to detect in that case
			}
			if (pointNo != options.LabelNo) {
				return false; // in-out point number equality assured
			} 
			if (options.LabelNo != inPoints.Length) {
				inPoints = new Point2i[options.LabelNo];
			}
			if (options.LabelNo != lastPoints.Length) {
				lastPoints = new Point2i[options.LabelNo];
				lastLabels = new int[options.LabelNo];
			}
			if (options.LabelNo != outPoints.Length) {
				outPoints = new Point2i[options.LabelNo];
				outLabels = new int[options.LabelNo];
				outPointNo = 0; // relabel triggered below
			}
			if (options.LabelNo != permutationPointNo) {
				permutations = BuildPermutations(options.LabelNo);
				permutationPointNo = options.LabelNo;
			}

			Array.Copy(points, inPoints, options.LabelNo);
			Array.Copy(outPoints, lastPoints, outPointNo);
			Array.Copy(outLabels, lastLabels, outPointNo);


			if (outPointNo != pointNo) {
				relabel = true; // relabel on algorithm start and label numbner modifications
			} else {
				relabel = false;
			}

			inPoints = points;
			inPointNo = pointNo;
			return true;

		}
		private void AssignLabels() {
			for (int p = 0; p < inPointNo; p++) {
				outPoints[p] = inPoints[p];
				outLabels[p] = p;
			}
			outPointNo = inPointNo;
		}
		private void AssignLabels(int[] permutation) {
			for (int p = 0; p < inPointNo; p++) {
				outPoints[p] = inPoints[p];
				outLabels[p] = lastLabels[permutation[p]];
			}
			outPointNo = inPointNo;
		}
		private void MinmizeDistance() {
			// in-out point number equality assumed
			// at least one in point assumed
			{
				bool[] indexOccupation = new bool[inPointNo];
				int[] permutation = new int[inPointNo]; // from in to last point
				// first try the fast approximation
				// for every point search for the nearest correspondence
				int totalDistance = 0;

				for (int pIn = 0; pIn < inPointNo; pIn++) {
					Point2i inPoint = inPoints[pIn];
					int minIndex = -1;
					int minDistance = -1;
					for (int pOut = 0; pOut < inPointNo; pOut++) {
						Point2i outPoint = lastPoints[pOut];
						int dx = inPoint.X - outPoint.X;
						int dy = inPoint.Y - outPoint.Y;
						int distance = dx*dx + dy*dy;
						if (minIndex < 0 || distance < minDistance) {
							minIndex = pOut;
							minDistance = distance;
						}
					}
					if (indexOccupation[minIndex]) {
						break; // every last point must be assigned exactly one in point
					} else {
						indexOccupation[minIndex] = true;
						permutation[pIn] = minIndex;
						totalDistance += minDistance;
					}

				}
				if (totalDistance <= options.FastApproximationThreshold) {
					AssignLabels(permutation);
					return;
				}
			}
			{
				// find best match of all permutations
				int minIndex = -1;
				int minDistance = -1;
				for (int i = 0; i < permutations.Length; i++) {
					int[] permutation = permutations[i]; // from in to last point
					int totalDistance = 0;
					for (int p = 0; p < inPointNo; p++) {
						Point2i inPoint = inPoints[p];
						Point2i outPoint = lastPoints[permutation[p]];
						int dx = inPoint.X - outPoint.X;
						int dy = inPoint.Y - outPoint.Y;
						int distance = dx*dx + dy*dy;
						totalDistance += distance;
					}
					if (minIndex < 0 || totalDistance < minDistance) {
						minIndex = i;
						minDistance = totalDistance;
					}
				}
				AssignLabels(permutations[minIndex]);
				return;
			}
		}


		private static int[][] BuildPermutations(int indexNo) {
			var result = new List<int[]>(indexNo*indexNo); // all permutations, we omit indexNo! calculation here
			// nothing occupied at start
			BuildPermutations(0, indexNo, new bool[indexNo], new int[indexNo], result); // recursive permutation generation
			return result.ToArray();
		}
		private static void BuildPermutations(int index, int indexNo, bool[] indexOccupation, int[] permutation, List<int[]> result) {
			if (index == indexNo - 1) {
				// fill list
				for (int i = 0; i < indexNo; i++) {
					if (!indexOccupation[i]) {
						permutation[index] = i;
						result.Add((int[])permutation.Clone());
					}
				}
			} else {
				// recursive calls with all configurations, respecting array boundaries
				for (int i = 0; i < indexNo; i++) {
					if (!indexOccupation[i]) { 
						indexOccupation[i] = true; 
						permutation[index] = i;
						BuildPermutations(index + 1, indexNo, indexOccupation, permutation, result);
						indexOccupation[i] = false;
					}
				}
			}
		}

	}

	public partial class PointLabeler {
		static Size2i[] visualSize;
		static Point2i[] visualOffset;
		static Point2i[][] visualPixels;

		static PointLabeler() {
			BuildVisuals();
		}
		private void ShowLabels(Emgu.CV.Mat imageOut) {
			int width = imageOut.Width;
			int height = imageOut.Height;
			int stride = width;
			IntPtr dataOut = imageOut.DataPointer;
			for (int p = 0; p < outPointNo; p++) {
				Point2i point = outPoints[p];
				int label = outLabels[p];
				Size2i size = visualSize[label];
				Point2i offset = visualOffset[label];
				int cLow = point.X + offset.X;
				int cHigh = point.X + offset.X + size.Width;
				int rLow = point.Y + offset.Y;
				int rHigh = point.Y + offset.Y + size.Height;
				if (cLow < 0 || cHigh >= width || rLow < 0 || rHigh >= height) {
					continue; // avoid out of bounds drawing
				}
				Point2i[] pixels = visualPixels[label];
				for (int i = 0; i < pixels.Length; i++) {
					int c = point.X + offset.X + pixels[i].X;
					int r = point.Y + offset.Y + pixels[i].Y;
					Marshal.WriteByte(dataOut, r*stride + c, 255);
				}
			}
		}
		private static void BuildVisuals() {
			visualSize = new Size2i[3];
			visualOffset = new Point2i[3];
			visualPixels = new Point2i[3][];
			{ // 0
				visualSize[0] = new Size2i(4, 8);
				visualOffset[0] = new Point2i(-8, -12);
				visualPixels[0] = new Point2i[] {
					
					new Point2i(1, 0), // top
					new Point2i(2, 0),
					new Point2i(0, 1), // left
					new Point2i(0, 2),
					new Point2i(0, 3),
					new Point2i(0, 4),
					new Point2i(0, 5),
					new Point2i(0, 6),
					new Point2i(3, 1), // right
					new Point2i(3, 2),
					new Point2i(3, 3),
					new Point2i(3, 4),
					new Point2i(3, 5),
					new Point2i(3, 6),
					new Point2i(1, 7), // bottom
					new Point2i(2, 7)
				};
			}
			{ // 1
				visualSize[1] = new Size2i(4, 8);
				visualOffset[1] = new Point2i(-8, -12);
				visualPixels[1] = new Point2i[] {
					new Point2i(0, 2), // flag
					new Point2i(1, 1),
					new Point2i(2, 0), // vertical line
					new Point2i(2, 1),
					new Point2i(2, 2),
					new Point2i(2, 3),
					new Point2i(2, 4),
					new Point2i(2, 5),
					new Point2i(2, 6),
					new Point2i(1, 7), // base
					new Point2i(2, 7),
					new Point2i(3, 7)
				};
			}
			{ // 2
				visualSize[2] = new Size2i(4, 8);
				visualOffset[2] = new Point2i(-8, -12);
				visualPixels[2] = new Point2i[] {
					new Point2i(0, 2),
					new Point2i(0, 1), // turn
					new Point2i(1, 0),
					new Point2i(2, 0),
					new Point2i(3, 1),
					new Point2i(3, 2),
					new Point2i(3, 3), // diagonal
					new Point2i(2, 4),
					new Point2i(1, 5),
					new Point2i(0, 6),
					new Point2i(0, 7), // base
					new Point2i(1, 7),
					new Point2i(2, 7),
					new Point2i(3, 7)
				};
			}
		}
	}
}

