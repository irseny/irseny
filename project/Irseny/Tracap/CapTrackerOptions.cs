using System;
using System.Collections.Generic;

namespace Irseny.Tracap {
	public class CapTrackerOptions {
		List<int> streams;

		public CapTrackerOptions() {
			streams = new List<int>(2);
			Threshold = 250;
			Smoothing = 0;
			PointBufferLength = 1000;
			MaxLights = 3;
		}
		public CapTrackerOptions(CapTrackerOptions source) {
			streams = new List<int>(source.streams);
			Threshold = source.Threshold;
			Smoothing = source.Smoothing;
			PointBufferLength = source.PointBufferLength;
			MaxLights = source.MaxLights;
		}
		public int Threshold { get; set; }
		public int Smoothing { get; set; }
		public int PointBufferLength { get; set; }
		public int MaxLights { get; set; }

		public int StreamNo {
			get { return streams.Count; }
		}
		public int GetStreamId(int index) {
			if (index < 0 || index >= streams.Count) {
				return -1;
			} else {
				return streams[index];
			}
		}
		public void AddStreamId(int id) {
			streams.Add(id);
		}
	}
}