using System;
using System.Threading;
using System.Collections.Generic;

namespace Irseny.Tracap {
	public class DetectionSystem : IDisposable {
		static object instanceSync = new object();
		static DetectionSystem instance = null;

		object detectorSync = new object();
		List<object> detectors = new List<object>(4);
		List<Thread> detectorThreads = new List<Thread>(4);

		public DetectionSystem() {
		}



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
			
		}
	}
}

