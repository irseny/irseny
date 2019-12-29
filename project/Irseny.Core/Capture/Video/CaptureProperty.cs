using System;
using InternalProperty = Emgu.CV.CvEnum.CapProp;

namespace Irseny.Core.Capture.Video {
	public enum CaptureProperty {
		CameraId = 0,
		FrameWidth = InternalProperty.FrameWidth,
		FrameHeight = InternalProperty.FrameHeight,
		FrameRate = InternalProperty.Fps,
		Exposure = InternalProperty.Exposure,
		Brightness = InternalProperty.Brightness,
		Contrast = InternalProperty.Contrast,
		Gain = InternalProperty.Gain

	}
}
