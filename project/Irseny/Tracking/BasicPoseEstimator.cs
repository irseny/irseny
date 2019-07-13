using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;
using Size2i = System.Drawing.Size;

namespace Irseny.Tracking {
	public class BasicPoseEstimator {
		const int TopPointIndex = 0;
		const int RightPointIndex = 1;
		const int LeftPointIndex = 2;
		const int AcceptedPointNo = 3;

		TrackerSettings settings;
		Point2i[] inPoints;
		int[] inLabels;
		Point2i[] centerPoints;

		int topPointLabel;
		int rightPointLabel;
		int leftPointLabel;

		LinkedList<CapPosition> positionHistory = new LinkedList<CapPosition>();

		public BasicPoseEstimator(TrackerSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			this.settings = settings;
			this.inPoints = new Point2i[0];
			this.inLabels = new int[0];
			this.centerPoints = new Point2i[0];
			this.topPointLabel = -1;
			this.rightPointLabel = -1;
			this.leftPointLabel = -1;

		}
		public bool Centered {
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
			Point2i tPos, rPos, lPos, bMid;
			Point2i tDelta, rDelta, lDelta, bDelta, mDelta; // position relative to centered
			{ // position
				Point2i tCenter = centerPoints[TopPointIndex];
				tPos = inPoints[TopPointIndex];
				tDelta = new Point2i(tPos.X - tCenter.X, tPos.Y - tCenter.Y);
				Point2i lCenter = centerPoints[LeftPointIndex];
				lPos = inPoints[LeftPointIndex];
				lDelta = new Point2i(lPos.X - lCenter.X, lPos.Y - lCenter.Y);
				Point2i rCenter = centerPoints[RightPointIndex];
				rPos = inPoints[RightPointIndex];
				rDelta = new Point2i(rPos.X - rCenter.X, rPos.Y - rCenter.Y);
				bDelta = new Point2i((lDelta.X + rDelta.X)/2, (lDelta.Y + rDelta.Y)/2);
				bMid = new Point2i((rPos.X + lPos.X)/2, (rPos.Y + lPos.Y)/2);
				Point2i pMid = new Point2i((rPos.X + lPos.X + tPos.X)/3, (rPos.Y + lPos.Y + tPos.Y)/3);
				Point2i refMid = new Point2i((tCenter.X + lCenter.X + rCenter.X)/3, (tCenter.Y + lCenter.Y + rCenter.Y)/3);
				mDelta = new Point2i(pMid.X - refMid.X, pMid.Y - refMid.Y);
			}
			{
				result.PosX = tDelta.X*0.02f;
				result.PosY = tDelta.Y*0.02f;
			}
			{
				Point2i bRelDelta = new Point2i(bDelta.X - tDelta.X, bDelta.Y - tDelta.Y);
				result.Yaw = -bRelDelta.X*0.02f;
				result.Pitch = bRelDelta.Y*0.02f;
			}
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

