using System;
using System.Collections.Generic;

namespace Irseny.Tracap {
	public class CapTrackerOptions {
		List<int> streams;

		public CapTrackerOptions() {
			streams = new List<int>(2);
			Threshold = 0.5f;
			Smoothing = 0;
		}
		public CapTrackerOptions(CapTrackerOptions source) {
			streams = new List<int>(source.streams);
			Threshold = source.Threshold;
			Smoothing = source.Smoothing;
		}
		public float Threshold { get; set; }
		public int Smoothing { get; set; }

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