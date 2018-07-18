using System;

namespace Irseny.Tracap {
	public interface IBasicPoseEstimatorOptions {
		/// <summary>
		/// Gets the amount of smoothing to apply to the detected pose.
		/// </summary>
		/// <value>The amount of smoothing.</value>
		int Smoothing { get; }
		/// <summary>
		/// Gets the weight of an input point that is used in classifying a point as top, left or right.
		/// </summary>
		/// <value>The weight of a single point frame.</value>
		float PointFrameLocationWeight { get; }
	}
}

