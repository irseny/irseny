using System;
using System.Threading;
using System.Collections.Generic;

namespace Irseny.Tracap {
	public class DetectionSystem : IDisposable {
		static object instanceSync = new object();
		static DetectionSystem instance = null;

		object detectorSync = new object();
		List<IHeadDetector> detectors = new List<IHeadDetector>(4);
		List<Thread> detectorThreads = new List<Thread>(4);

		public DetectionSystem() {
		}
		public static DetectionSystem Instance {
			get {
				lock (instanceSync) {
					return instance;
				}
			}
		}
		public int Start(IHeadDetector detector) {
			if (detector == null) throw new ArgumentNullException("detector");
			lock (detectorSync) {
				int id;
				// find unused index
				for (id = 0; id < detectors.Count; id++) {
					if (detectors[id] == null) {
						break;
					}
				}
				if (id < detectors.Count) {
					detectors[id] = detector;
				} else {
					detectors.Add(detector);
				}
				if (!detector.Start()) {
					detectors[id] = null;
					return -1;
				} else {
					return id;
				}
			}
		}
		public IHeadDetector GetDetector(int id) {
			lock (detectorSync) {
				if (id < 0 || id >= detectors.Count) {
					return null;
				} else {
					return detectors[id];
				}
			}
		}
		public bool Stop(int id) {
			lock (detectorSync) {
				if (id < 0 || id >= detectors.Count) {
					return false;
				}
				if (detectors[id] == null) {
					return false;
				}
				detectors[id].Stop();
				detectors[id].Dispose();
				detectors[id] = null;
				return true;
			}
		}
		// TODO: create detection system thread and invoke method


		public static void MakeInstance(DetectionSystem instance) {
			lock (instanceSync) {
				if (DetectionSystem.instance != null) {
					DetectionSystem.instance.Dispose();
					DetectionSystem.instance = null;

				}
				if (instance != null) {
					DetectionSystem.instance = instance;
				}
			}
		}

		public void Dispose() {
			lock (detectorSync) {
				for (int id = 0; id < detectors.Count; id++) {
					Stop(id);
				}
			}
		}
	}
}

