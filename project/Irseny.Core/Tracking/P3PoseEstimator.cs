// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;
using Size2i = System.Drawing.Size;
using Irseny.Core.Log;
using Irseny.Core.Listing;
using Irseny.Core.Inco.Device;
using Irseny.Core.Util;

namespace Irseny.Core.Tracking {
	public class P3PoseEstimator {
		const int TopPointIndex = 0;
		const int RightPointIndex = 1;
		const int LeftPointIndex = 2;
		const int AcceptedPointNo = 3;

		EquipmentSettings settings;
		IObjectModel model;
		Point2i[] inPoints = new Point2i[0];
		int[] inLabels = new int[0];
		Point2i[] centerPoints = new Point2i[0];

		int topPointLabel = -1;
		int rightPointLabel = -1;
		int leftPointLabel = -1;



		LinkedList<CapPosition> positionHistory = new LinkedList<CapPosition>();

		public P3PoseEstimator(EquipmentSettings settings, IObjectModel model) {
			if (settings == null) throw new ArgumentNullException("settings");
			if (model == null) throw new ArgumentNullException("model");
			this.settings = settings;
			this.model = model;
		}
		private bool Centered {
			get { return topPointLabel > -1 && rightPointLabel > -1 && leftPointLabel > -1; }
		}
		private CapPosition LastPosition {
			get {
				if (positionHistory.Count > 0) {
					return positionHistory.Last.Value;
				} else {
					return new CapPosition();
				}
			}
		}
		public CapPosition Estimate(Point2i[] points, int[] labels, int pointNo) {
			if (!Setup(points, labels, pointNo)) {
				return LastPosition;
			}
			if (!Centered) {
				if (!Center()) {
					return LastPosition;
				}
			}

			CapPosition position = EstimatePosition();
			position = RegisterPosition(position);
			return position;
		}
		public bool Center() {
			topPointLabel = -1;
			leftPointLabel = -1;
			rightPointLabel = -1;
			// break if not enough information available
			// assumes that the labels are also set correctly
			if (inPoints.Length != AcceptedPointNo) {
				return false;
			}
			if (centerPoints.Length < AcceptedPointNo) {
				centerPoints = new Point2i[AcceptedPointNo];
			}
			// classify points
			// associate required point names with labels
			// maximum expected image extends
			int minTop = 1080;
			int minLeft = 1920;
			int minRight = 0;
			for (int i = 0; i < AcceptedPointNo; i++) {
				if (inPoints[i].Y < minTop) {
					topPointLabel = inLabels[i];
					centerPoints[TopPointIndex] = inPoints[i];
					minTop = inPoints[i].Y;
				}
				if (inPoints[i].X < minLeft) {
					leftPointLabel = inLabels[i];
					centerPoints[LeftPointIndex] = inPoints[i];
					minLeft = inPoints[i].X;
				}
				if (inPoints[i].X > minRight) {
					rightPointLabel = inLabels[i];
					centerPoints[RightPointIndex] = inPoints[i];
					minRight = inPoints[i].X;
				}
			}
			// break if labels can not be set uniquely or some points are not usable
			if (topPointLabel == leftPointLabel || topPointLabel == rightPointLabel || leftPointLabel == rightPointLabel ||
				topPointLabel < 0 || leftPointLabel < 0 || rightPointLabel < 0) {
				topPointLabel = -1;
				leftPointLabel = -1;
				rightPointLabel = -1;
				return false;
			}
			// bring points into default order
			// create copy to not read and write from inPoints/inLabels
			MakeDefaultOrdering((Point2i[])inPoints.Clone(), (int[])inLabels.Clone());
			return true;
		}
		private void MakeDefaultOrdering(Point2i[] points, int[] labels) {
			// sort with respect to constant label-index order
			for (int i = 0; i < AcceptedPointNo; i++) {
				if (labels[i] == topPointLabel) {
					inPoints[TopPointIndex] = points[i];
					inLabels[TopPointIndex] = topPointLabel;
				} else if (labels[i] == leftPointLabel) {
					inPoints[LeftPointIndex] = points[i];
					inLabels[LeftPointIndex] = leftPointLabel;
				} else if (labels[i] == rightPointLabel) {
					inPoints[RightPointIndex] = points[i];
					inLabels[RightPointIndex] = rightPointLabel;
				} else {
					throw new ArgumentException("labels: Unknown label: " + labels[i]);
				}

			}
		}
		private bool Setup(Point2i[] points, int[] labels, int pointNo) {
			if (pointNo != AcceptedPointNo) {
				return false;
			}
			if (inPoints.Length != AcceptedPointNo) {
				inPoints = new Point2i[AcceptedPointNo];
				inLabels = new int[AcceptedPointNo];
			}
			if (Centered) {
				MakeDefaultOrdering(points, labels);
			} else {
				// just copy the input for classification
				for (int i = 0; i < AcceptedPointNo; i++) {
					inPoints[i] = points[i];
					inLabels[i] = labels[i];
				}
				if (!Center()) {
					return false;
				}
			}
			return true;
		}
		private CapPosition EstimatePosition() {
			var result = new CapPosition();
			// working
			//var objectPoints = new Emgu.CV.Mat(3, 3, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
			//var imagePoints = new Emgu.CV.Mat(3, 2, Emgu.CV.CvEnum.DepthType.Cv32F, 1);

			// TODO call external pose reconstruction code

			/*Tuple<int, int, int> topPoint = model.GetPoint(0);
			Tuple<int, int, int> leftPoint = model.GetPoint(1);
			Tuple<int, int, int> rightPoint = model.GetPoint(2);
			var objectPoints = new Emgu.CV.Matrix<float>(new float[,] {
				{ topPoint.Item1, topPoint.Item2, topPoint.Item3 },
				{ rightPoint.Item1, rightPoint.Item2, rightPoint.Item3 },
				{ leftPoint.Item1, leftPoint.Item2, leftPoint.Item3 }
			});
			var imagePoints = new Emgu.CV.Matrix<float>(new float[,] {
				{ inPoints[TopPointIndex].X, inPoints[TopPointIndex].Y },
				{ inPoints[RightPointIndex].X, inPoints[RightPointIndex].Y },
				{ inPoints[LeftPointIndex].X, inPoints[RightPointIndex].Y }
			});
			float fx = (float)(320/Math.Tan(57.5*Math.PI/360)*0.5);
			float fy = (float)(240/Math.Tan(45*Math.PI/360)*0.5);
			var mIntrinsic = new Emgu.CV.Matrix<float>(new float[,] {
				{ fx, 0, 320/2 },
				{ 0, fy, 240/2 },
				{ 0, 0, 1 }
			});

			//mIntrinsic.SetIdentity();
			var distortion = new Emgu.CV.Matrix<float>(new float[] { 0, 0, 0, 0 });
			var rotation = new Emgu.CV.Matrix<float>(new float[] { 0, 0, 0 });
			var translation = new Emgu.CV.Matrix<float>(new float[] { 0, 0, 64 });
			bool success = false;
			try {
				// working
				success = Emgu.CV.CvInvoke.SolvePnP(objectPoints, imagePoints, mIntrinsic, distortion, rotation, translation, true, Emgu.CV.CvEnum.SolvePnpMethod.Iterative);
				//success = Emgu.CV.CvInvoke.SolvePnP(objectPoints, imagePoints, mIntrinsic, distortion, rotation, translation, true, Emgu.CV.CvEnum.SolvePnpMethod.Iterative);
			} catch(Emgu.CV.Util.CvException e) {
				LogManager.Instance.LogError(this, "Failed to detect pose: " + e.StackTrace);
			}
			if (success) {
				result.Yaw = new KeyState(rotation.Data[1, 0], rotation.Data[1, 0]);
				result.Pitch = new KeyState(rotation.Data[0, 0], rotation.Data[0, 0]);
				result.Roll = new KeyState(rotation.Data[2, 0], rotation.Data[2, 0]);

				result.PosX = new KeyState(translation.Data[0, 0], translation.Data[0, 0]);
				result.PosY = new KeyState(translation.Data[1, 0], translation.Data[1, 0]);
				result.PosZ = new KeyState(translation.Data[2, 0], translation.Data[2, 0]);
			}*/
			return result;
		}

		private CapPosition RegisterPosition(CapPosition position) {
			positionHistory.Clear();
			positionHistory.AddLast(position);
			// TODO: smooth with history
			return position;
		}
	}
}

