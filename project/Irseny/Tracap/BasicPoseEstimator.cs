using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;
using Size2i = System.Drawing.Size;

namespace Irseny.Tracap {
	public class BasicPoseEstimator {
		IBasicPoseEstimatorOptions options;
		Point2i[] inPoints;
		int[] inLabels;
		int[] labelInPointMap;
		Point2f[] averagePoints;
		int[] labelAveragePointMap;
		Point2i[] centerPoints;
		int[] labelCenterPointMap;

		int topPointLabel;
		int rightPointLabel;
		int leftPointLabel;
		LinkedList<CapPosition> poseHistory = new LinkedList<CapPosition>();

		public BasicPoseEstimator(IBasicPoseEstimatorOptions options) {
			this.options = options;

			this.labelInPointMap = new int[0];
			this.averagePoints = new Point2f[0];
			this.labelAveragePointMap = new int[0];
			this.centerPoints = new Point2i[0];
			this.labelCenterPointMap = new int[0];

		}
		private CapPosition LastPose {
			get {
				if (poseHistory.Count > 0) {
					return poseHistory.Last.Value;
				} else {
					return new CapPosition();
				}
			}
		}
		public CapPosition Estimate(Point2i[] points, int[] labels, int pointNo) {
			if (!Setup(points, labels, pointNo)) {
				return LastPose;
			}
			ClassifyPoints();
			// TODO: smooth with history
			CapPosition pose = EstimatePose();
			return pose;
		}
		public bool Center() {
			if (averagePoints.Length < 3 || centerPoints.Length < 3) {
				return false;
			}
			for (int i = 0; i < 3; i++) {
				Point2f source = inPoints[labelInPointMap[i]];
				// put point iwth label i at index i
				centerPoints[i] = new Point2i((int)source.X, (int)source.Y);
				labelCenterPointMap[i] = i;
			}
			return true;
		}
		private bool Setup(Point2i[] points, int[] labels, int pointNo) {
			if (pointNo != 3) {
				return false;
			}
			if (averagePoints.Length != 3) {
				averagePoints = new Point2f[3];
				labelAveragePointMap = new int[3];
				for (int i = 0; i < 3; i++) {
					// put point with label i at index i
					averagePoints[i] = new Point2f(0, 0);
					labelAveragePointMap[i] = i;
				}

			}
			inPoints = points;
			inLabels = labels;
			if (labelInPointMap.Length != 3) {
				labelInPointMap = new int[3];
			}
			for (int i = 0; i < 3; i++) {
				// updated every frame, point with label i can have variable index
				labelInPointMap[inLabels[i]] = i;
			}
			if (centerPoints.Length != 3) {
				centerPoints = new Point2i[3];
				labelCenterPointMap = new int[3];
				for (int i = 0; i < 3; i++) {
					// put point with label i at index i
					centerPoints[i] = inPoints[labelInPointMap[i]];
					labelCenterPointMap[i] = i;
				}
			}
			return true;
		}
		private void ClassifyPoints() {
			float weight = options.PointFrameLocationWeight;
			for (int i = 0; i < 3; i++) {
				float dx = inPoints[i].X - averagePoints[i].X;
				float dy = inPoints[i].Y - averagePoints[i].Y;
				averagePoints[i] = new Point2f(averagePoints[i].X + dx*weight, averagePoints[i].Y + dy*weight);
			}
			float minTop = 1080;
			float minLeft = 1920;
			float minRight = 0;
			topPointLabel = -1;
			rightPointLabel = -1;
			leftPointLabel = -1;
			for (int i = 0; i < 3; i++) {
				if (averagePoints[i].Y < minTop) {
					topPointLabel = inLabels[i];
					minTop = averagePoints[i].Y;
				}
				if (averagePoints[i].X < minLeft) {
					leftPointLabel = inLabels[i];
					minLeft = averagePoints[i].X;
				}
				if (averagePoints[i].X > minRight) {
					rightPointLabel = inLabels[i];
					minRight = averagePoints[i].X;
				}
			}
		}
		private CapPosition EstimatePose() {
			var result = new CapPosition();
			Point2i tPos, rPos, lPos, bMid;
			Point2i tDelta, rDelta, lDelta, bDelta, mDelta; // position relative to centered
			{ // position
				Point2i tCenter = centerPoints[labelCenterPointMap[topPointLabel]];
				tPos = inPoints[labelInPointMap[topPointLabel]];
				tDelta = new Point2i(tPos.X - tCenter.X, tPos.Y - tCenter.Y);
				Point2i lCenter = centerPoints[labelCenterPointMap[leftPointLabel]];
				lPos = inPoints[labelInPointMap[leftPointLabel]];
				lDelta = new Point2i(lPos.X - lCenter.X, lPos.Y - lCenter.Y);
				Point2i rCenter = centerPoints[labelCenterPointMap[rightPointLabel]];
				rPos = inPoints[labelInPointMap[rightPointLabel]];
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



			/*Point2i bottomTranslation; // average bottom point movement
			Point2i horizontalDelta; // bottom point distance
			Point2i topTranslation; // top point movement
			Point2i verticalDelta; // distance bottom to top
			Point2i averageTranslation; // point movement
			{
				// translation
				Point2i topCenter = centerPoints[labelCenterPointMap[topPointLabel]];
				Point2i topIn = inPoints[labelInPointMap[topPointLabel]];
				topTranslation = new Point2i(topIn.X - topCenter.X, topIn.Y - topCenter.Y);
				// left difference
				Point2i leftCenter = centerPoints[labelCenterPointMap[leftPointLabel]];
				Point2i leftIn = inPoints[labelInPointMap[leftPointLabel]];
				var leftTranslation = new Point2i(
					                      leftIn.X - leftCenter.X,
					                      leftIn.Y - leftCenter.Y);
				// right difference
				Point2i rightCenter = centerPoints[labelCenterPointMap[rightPointLabel]];
				Point2i rightIn = inPoints[labelInPointMap[rightPointLabel]];
				var rightTranslation = new Point2i(
					                       rightIn.X - rightCenter.X,
					                       rightIn.Y - rightCenter.Y);
				// horizontal difference
				horizontalDelta = new Point2i(rightIn.X - leftIn.X, rightIn.Y - leftIn.Y);
				bottomTranslation = new Point2i(
					(leftTranslation.X + rightTranslation.X)/2,
					(leftTranslation.Y + rightTranslation.Y)/2);
				Point2i bottomIn = new Point2i((rightIn.X + leftIn.X)/2, (rightIn.Y + leftIn.Y)/2);
				// vertical difference
				verticalDelta = new Point2i(bottomIn.X - topIn.X, bottomIn.Y - topIn.Y);
				averageTranslation = new Point2i(
					(topTranslation.X + rightTranslation.X + leftTranslation.X)/3,
					(topTranslation.Y + rightTranslation.Y + leftTranslation.Y)/3);
			}
			var result = new CapPosition();
			result.Yaw = horizontalDelta.X*0.024f; // TODO: implement usable functions
			result.Pitch = verticalDelta.Y*0.024f;
			return result;*/
		}
	}
}

