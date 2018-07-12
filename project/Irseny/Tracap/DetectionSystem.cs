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
		List<object> detectorsSync = new List<object>(4);
		AutoResetEvent invokeSignal = new AutoResetEvent(false);
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();
		readonly object detectorSync = new object();
		List<IPoseDetector> detectors = new List<IPoseDetector>(4);


		public DetectionSystem() {
		}
		/// <summary>
		/// Gets the currently active instance.
		/// </summary>
		/// <value>The instance.</value>
		public static DetectionSystem Instance {
			get {
				lock (instanceSync) {
					return instance;
				}
			}
		}
		/// <summary>
		/// Starts the given detector.
		/// </summary>
		/// <returns>Identifier for the detector. Less than 0 if starting was not successful.</returns>
		/// <param name="detector">Detector.</param>
		public int StartDetector(IPoseDetector detector) {
			if (detector == null) throw new ArgumentNullException("detector");
			lock (detectorSync) {
				int id;
				// find unused index
				for (id = 0; id < detectors.Count; id++) {
					lock (detectorsSync[id]) {
						if (detectors[id] == null) {
							break;
						}
					}
				}
				if (id < detectors.Count) {
					detectors[id] = detector;
					// detector instance sync was not removed
				} else {
					detectors.Add(detector);
					detectorsSync.Add(new object());
				}
				lock (detectorsSync[id]) {
					if (detector.Start()) {
						detector.InputAvailable += SignalStep;
					} else {
						detectors[id] = null;
						return -1;

					}
				}
				return id;
			}
		}
		/// <summary>
		/// Gets the detector specified by the given identifier.
		/// </summary>
		/// <returns>The detector. Null if it does not exist.</returns>
		/// <param name="id">Detector identifier previously returned by <see cref="StartDetector"></see>.</param>
		public IPoseDetector GetDetector(int id) {
			lock (detectorSync) {
				if (id < 0 || id >= detectors.Count) {
					return null;
				} else {
					lock (detectorsSync[id]) {
						return detectors[id];
					}
				}
			}
		}
		/// <summary>
		/// Stops, disposes and removes the detector specified by the given identifier.
		/// </summary>
		/// <returns><c>true</c>, if the detector existed, <c>false</c> otherwise.</returns>
		/// <param name="id">Detector identifier previously returned by <see cref="StartDetector"></see>.</param>
		public bool StopDetector(int id) {
			lock (detectorSync) {
				if (id < 0 || id >= detectors.Count) {
					return false;
				}
				lock (detectorsSync[id]) {
					if (detectors[id] == null) {
						return false;
					}
					detectors[id].Stop();
					detectors[id].Dispose();
					detectors[id] = null;
				}
				return true;
			}
		}
		/// <summary>
		/// Invoke the specified handler on the detection thread.
		/// </summary>
		/// <param name="handler">Handler to invoke.</param>
		public void Invoke(EventHandler handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
				invokeSignal.Set();
			}
		}
		/// <summary>
		/// Invokes all pending event handlers.
		/// </summary>
		private void InvokePending() {
			Queue<EventHandler> pending;
			lock (invokeSync) {
				pending = toInvoke;
				if (pending.Count > 0) {
					toInvoke = new Queue<EventHandler>();
				}
			}
			foreach (EventHandler handler in pending) {
				handler(this, new EventArgs());
			}
		}
		/// <summary>
		/// Executes a single iteration of all started detectors. Removes all stopped detectors.
		/// </summary>
		private void StepDetectors() {
			int detectorNo;
			lock (detectorSync) {
				detectorNo = detectors.Count;
			}
			for (int i = 0; i < detectorNo; i++) {
				// get the detector and its synchronization object
				// for that we need to lock the detector list and the detector instance
				IPoseDetector detector;
				object sync;
				lock (detectorSync) {
					sync = detectorsSync[i];
					lock (sync) {
						detector = detectors[i];
						if (detector != null) {
							// dispose and remove stopped detectors
							if (!detector.Running) {
								detector.Dispose();
								detectors[i] = null;
								detector = null;
							}
						}
					}
				}
				// only the detector instance lock is required to step the detector
				if (detector != null) {
					lock (sync) {
						if (detector.Running) { // could be stopped between here and the lock above
							detector.Step();
						}
					}
				}
			}
		}
		/// <summary>
		/// Sets the stop signal that causes this instance to stop execution.
		/// </summary>
		private void SignalStop() {
			running = false;
			invokeSignal.Set();
		}
		/// <summary>
		/// Signals this instance to step the detectors due to available data to process.
		/// </summary>
		/// <param name="sender">Ignnored.</param>
		/// <param name="args">Ignored.</param>
		private void SignalStep(object sender, EventArgs args) {
			invokeSignal.Set();
		}
		/// <summary>
		/// Signals this instance to step the detectors due to available data to process.
		/// </summary>
		private void SignalStep() {
			invokeSignal.Set();
		}
		/// <summary>
		/// Runs this instance until the stop signal is set.
		/// </summary>
		private void Run() {
			running = true; // should be executed before the stop signal is set
			while (running) {
				invokeSignal.WaitOne();
				InvokePending();
				StepDetectors();
			}
			// cleanup
			lock (detectorSync) {
				for (int id = 0; id < detectors.Count; id++) {
					StopDetector(id);
				}
			}
		}



		/// <summary>
		/// Makes the given instance the current instance. Stops the previously active instance.
		/// Runs the new instance on a separate thread.
		/// </summary>
		/// <param name="instance">Instance to make current.</param>
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

