using System;
namespace Irseny.Core.Tracking {
	public enum TrackerProperty {
		Tracking,
		Stream0,
		Stream1,
		Smoothing,
		SmoothingDropoff,
		MinBrightness,
		MinClusterRadius,
		MaxClusterRadius,
		MaxClusterMembers,
		MaxClusterNo,
		MaxPointNo,
		MinLayerEnergy,
		LabelNo,
		FastApproxThreshold,
		MaxQueuedImages
	}
}
