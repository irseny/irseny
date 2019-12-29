using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Point2i = System.Drawing.Point;
using Size2i = System.Drawing.Size;

namespace Irseny.Tracking {
	public partial class PointLabeler {
		TrackerSettings settings;
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



		public PointLabeler(TrackerSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			this.settings = settings;
			this.lastLabels = new int[0];
			this.lastPoints = new Point2i[0];
			this.inPoints = new Point2i[0];
			this.outPoints = new Point2i[0];
			this.outLabels = new int[0];
			this.permutations = new int[0][];
			this.permutationPointNo = 0;
		}
		/// <summary>
		/// Executes one labelling iteration.
		/// </summary>
		/// <returns>The label.</returns>
		/// <param name="points">Detected points.</param>
		/// <param name="pointNo">Number of points detected.</param>
		/// <param name="imageOut">Image out.</param>
		/// <param name="labels">Point labels.</param>
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

			ShowLabels(imageOut);

			labels = outLabels;
			return outPointNo;
		}
		/// <summary>
		/// Performs setup operations for the next iteration.
		/// </summary>
		/// <returns>The setup.</returns>
		/// <param name="points">Detected points.</param>
		/// <param name="pointNo">Number of points detected.</param>
		public bool Setup(Point2i[] points, int pointNo) {
			if (pointNo < 1) {
				return false; // nothing to detect in that case
			}
			int targetLabelNo = settings.GetInteger(TrackerProperty.LabelNo, 3);
			if (pointNo != targetLabelNo) {
				return false; // in-out point number equality assured
			}
			if (targetLabelNo != inPoints.Length) {
				inPoints = new Point2i[targetLabelNo];
			}
			if (targetLabelNo != lastPoints.Length) {
				lastPoints = new Point2i[targetLabelNo];
				lastLabels = new int[targetLabelNo];
			}
			if (targetLabelNo != outPoints.Length) {
				outPoints = new Point2i[targetLabelNo];
				outLabels = new int[targetLabelNo];
				outPointNo = 0; // relabel triggered below
			}
			if (targetLabelNo != permutationPointNo) {
				permutations = BuildPermutations(targetLabelNo);
				permutationPointNo = targetLabelNo;
			}

			Array.Copy(points, inPoints, targetLabelNo);
			Array.Copy(outPoints, lastPoints, outPointNo);
			Array.Copy(outLabels, lastLabels, outPointNo);


			if (outPointNo != pointNo) {
				// relabel on algorithm start and label numbner modifications
				relabel = true;
			} else {
				relabel = false;
			}

			inPoints = points;
			inPointNo = pointNo;
			return true;

		}
		/// <summary>
		/// Assigns standard labels to out-points in ascending order.
		/// </summary>
		private void AssignLabels() {
			for (int p = 0; p < inPointNo; p++) {
				outPoints[p] = inPoints[p];
				outLabels[p] = p;
			}
			outPointNo = inPointNo;
		}
		/// <summary>
		/// Assigns labels to out-points according to the given labelling.
		/// </summary>
		/// <param name="permutation">Labelling.</param>
		private void AssignLabels(int[] permutation) {
			for (int p = 0; p < inPointNo; p++) {
				outPoints[p] = inPoints[p];
				outLabels[p] = lastLabels[permutation[p]];
			}
			outPointNo = inPointNo;
		}
		/// <summary>
		/// Finds the best match of new in-points to last-points.
		/// First tries a fast approximation and applies a full search if not sufficient.
		/// Assigns labels to points.
		/// </summary>
		private void MinmizeDistance() {
			// in-out-point number equality assumed
			// requires at least one in point
			int approxThreshold = settings.GetInteger(TrackerProperty.FastApproxThreshold, 0);
			if (approxThreshold > 0) { // test with last result
				bool[] indexOccupation = new bool[inPointNo];
				int[] permutation = new int[inPointNo];
				bool fastApproxValid = true;
				// from in-points to last-point
				// first try the fast approximation
				// for every point search for the nearest correspondence
				int totalDistance = 0;
				for (int pIn = 0; pIn < inPointNo; pIn++) {
					Point2i inPoint = inPoints[pIn];
					// find the last point with the lowest distance
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
						// avoid multiple assignments to the same index
						// every last-point must be assigned to exactly one in-point
						// continue with a test against all permutations
						fastApproxValid = false;
						break;
					} else {
						// build the fast approximation permutation
						indexOccupation[minIndex] = true;
						permutation[pIn] = minIndex;
						totalDistance += minDistance;
					}

				}
				if (fastApproxValid && totalDistance <= approxThreshold) {
					// if the fast approximation error is small enough use the permutation build above
					AssignLabels(permutation);
					return;
				}
			}
			{ // find best match of all permutations
				int minIndex = -1;
				int minDistance = -1;
				// test against all permutations and choose the one with lowest error
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

		/// <summary>
		/// Builds all permutations with the given number of elements.
		/// Permutated values start at 0.
		/// </summary>
		/// <returns>The permutations.</returns>
		/// <param name="elementNo">Index no.</param>
		private static int[][] BuildPermutations(int elementNo) {
			var result = new List<int[]>(elementNo*elementNo);
			BuildPermutations(0, elementNo, new bool[elementNo], new int[elementNo], result); // recursive permutation generation
			return result.ToArray();
		}
		/// <summary>
		/// Builds all permutations with the given number of elements starting from the given index.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="elementNo">Element no.</param>
		/// <param name="iOccupation">Elements already occupied.</param>
		/// <param name="permutation">Current permutation.</param>
		/// <param name="result">Result.</param>
		private static void BuildPermutations(int index, int elementNo, bool[] iOccupation, int[] permutation, List<int[]> result) {
			if (index == elementNo - 1) {
				// fill the last element and save the permutation
				for (int i = 0; i < elementNo; i++) {
					if (!iOccupation[i]) {
						permutation[index] = i;
						result.Add((int[])permutation.Clone());
					}
				}
			} else {
				// recursive calls with all configurations, respecting array boundaries
				for (int i = 0; i < elementNo; i++) {
					if (!iOccupation[i]) {
						iOccupation[i] = true;
						permutation[index] = i;
						BuildPermutations(index + 1, elementNo, iOccupation, permutation, result);
						iOccupation[i] = false; // clean after build
					}
				}
			}
		}

	}
	/// <summary>
	/// Draws visual representations of labels.
	/// </summary>
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

