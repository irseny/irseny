using System;
namespace Irseny.Capture.Video {
	public class StreamEventArgs : EventArgs {
		public StreamEventArgs(CaptureStream stream, int streamId) {
			if (stream == null) throw new ArgumentNullException("stream");
			if (streamId < 0) throw new ArgumentOutOfRangeException("streamId");
			Stream = stream;
			StreamId = streamId;
		}
		public CaptureStream Stream { get; private set; }
		public int StreamId { get; private set; }

	}
}
