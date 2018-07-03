using System;
using System.Threading;
using System.Collections.Generic;

namespace Irseny.Tracap {
	public class DetectionSystem {
		static object instanceSync = new object();
		static DetectionSystem instance = null;
		static Thread instanceThread = null;

		volatile bool running = false;
		readonly object invokeSync = new object();
		AutoResetEvent invokeSignal = new AutoResetEvent(false);
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();
		readonly object detectorSync = new object();
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
		public void Invoke(EventHandler handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
				invokeSignal.Set();
			}
		}
		private void InvokePending() {
			Queue<EventHandler> pending;
			lock (invokeSync) {
				pending = toInvoke;
				toInvoke = new Queue<EventHandler>();
			}
			foreach (EventHandler handler in pending) {
				handler(this, new EventArgs());
			}
		}
		private void StepDetectors() {
			// TODO: run detector update
		}
		private void SignalStop() {
			running = false;
			invokeSignal.Set();
		}
		private void Run() {
			running = true;
			while (running) {
				invokeSignal.WaitOne();
				InvokePending();
				StepDetectors();
			}
			// cleanup
			lock (detectorSync) {
				for (int id = 0; id < detectors.Count; id++) {
					Stop(id);
				}
			}
		}

		// TODO: create detection system thread and invoke method


		public static void MakeInstance(DetectionSystem instance) {			
			lock (instanceSync) {				
				if (DetectionSystem.instance != null) {
					DetectionSystem.instance.SignalStop();
					DetectionSystem.instanceThread.Join();
					DetectionSystem.instanceThread = null;
					DetectionSystem.instance = null;
				}
				if (instance != null) {
					DetectionSystem.instance = instance;
					DetectionSystem.instanceThread = new Thread(instance.Run);
					instanceThread.Start();
				}
			}
		}


	}
}

