using System;

namespace Irseny.Core.Sensors.VideoCapture {
	public struct VideoFrameMetadata {
		public int FrameRate { get; set; }
		public int FrameTime { get; set; }
		public int FrameTimeDeviation { get; set; }
	}
}

