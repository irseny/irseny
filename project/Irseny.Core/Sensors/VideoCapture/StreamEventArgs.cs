using System;
namespace Irseny.Core.Sensors.VideoCapture {
	public class StreamEventArgs : EventArgs {
		public StreamEventArgs(WebcamCapture stream, int streamId) {
			if (stream == null) throw new ArgumentNullException("stream");
			if (streamId < 0) throw new ArgumentOutOfRangeException("streamId");
			Stream = stream;
			StreamId = streamId;
		}
		public WebcamCapture Stream { get; private set; }
		public int StreamId { get; private set; }

	}
}
