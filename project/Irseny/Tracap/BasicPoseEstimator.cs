using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Point2i = System.Drawing.Point;
using Size2i = System.Drawing.Size;

namespace Irseny.Tracap {
	public class BasicPoseEstimator {
		IBasicPoseEstimatorOptions options;
		Point2i[] inPoints;
		int[] inLabels;
		int inPointNo;
		Point2i[] averagePoints;
		LinkedList<CapPosition> poseHistory = new LinkedList<CapPosition>();

		public BasicPoseEstimator(IBasicPoseEstimatorOptions options) {
			this.options = options;
			averagePoints = new Point2i[3];
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
			return new CapPosition();
		}

		private bool Setup(Point2i[] points, int[] labels, int pointNo) {
			if (pointNo != 3) {
				return false;
			}
			if (averagePoints.Length != 3) {
				averagePoints = new Point2i[3];
			}
			inPoints = points;
			inLabels = labels;
			inPointNo = pointNo;
			return true;
		}
	}
}

