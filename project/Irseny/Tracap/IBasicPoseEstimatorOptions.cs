using System;

namespace Irseny.Tracap {
	public interface IBasicPoseEstimatorOptions {
		/// <summary>
		/// Gets the amount of smoothing to apply to the detected pose.
		/// </summary>
		/// <value>The amount of smoothing.</value>
		int Smoothing { get; }
	}
}

