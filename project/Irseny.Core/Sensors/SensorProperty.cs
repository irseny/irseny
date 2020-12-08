using System;
using InternalProperty = Emgu.CV.CvEnum.CapProp;

namespace Irseny.Core.Sensors.VideoCapture {
	/// <summary>
	/// Generic properties for sensor configuration.
	/// </summary>
	public enum SensorProperty {
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
