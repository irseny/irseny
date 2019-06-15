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
		readonly object stepSync = new object();
		readonly object detectorSync = new object();
		AutoResetEvent invokeSignal = new AutoResetEvent(false);
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();
		Queue<IPoseDetector> toStep = new Queue<IPoseDetector>();
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
		/// Connects the given detector.
		/// </summary>
		/// <returns>Identifier for the detector. Less than 0 if starting was not successful.</returns>
		/// <param name="detector">Detector.</param>
		public int ConnectDetector(IPoseDetector detector) {
			if (detector == null) throw new ArgumentNullException("detector");
			lock (detectorSync) {
				int id;
				// find unused index
				for (id = 0; id < detectors.Count; id++) {
					if (detectors[id] == null) {
						break;
					}
				}
				// add or insert detector
				if (id < detectors.Count) {
					detectors[id] = detector;
				} else {
					detectors.Add(detector);
				}
				// the detector signals when it has data to process
				// to let it process we pick up the signal
				detector.InputAvailable += SignalStep;
				return id;
			}
		}

		/// <summary>
		/// Gets the detector specified by the given identifier.
		/// </summary>
		/// <returns>The detector. Null if it does not exist.</returns>
		/// <param name="id">Detector identifier previously returned by <see cref="ConnectDetector"/>.</param>
		public IPoseDetector GetDetector(int id) {
			lock (detectorSync) {
				if (id < 0 || id >= detectors.Count) {
					return null;
				}
				return detectors[id];
			}
		}

		/// <summary>
		/// Gets the detector specified by the given identifier and type.
		/// </summary>
		/// <returns>The detector. Null if it does not exist or there is a type mismatch.</returns>
		/// <param name="id">Detector identifier previously returned by <see cref="ConnectDetector"/>.</param>
		/// <param name="fallback">Default value.</param>
		/// <typeparam name="Tdetect">The detector type.</typeparam>
		public Tdetect GetDetector<Tdetect>(int id, Tdetect fallback) where Tdetect : IPoseDetector {
			lock (detectorSync) {
				if (id < 0 || id >= detectors.Count) {
					return fallback;
				}
				if (detectors[id] is Tdetect) {
					return (Tdetect)detectors[id];
				}
				return fallback;
			}
		}

		/// <summary>
		/// Stops, disposes and removes the detector specified by the given identifier.
		/// </summary>
		/// <returns><c>true</c>, if the detector existed, <c>false</c> otherwise.</returns>
		/// <param name="id">Detector identifier previously returned by <see cref="ConnectDetector"></see>.</param>
		public bool DisconnectDetector(int id) {
			lock (detectorSync) {
				if (id < 0 || id >= detectors.Count) {
					return false;
				}
				if (detectors[id] == null) {
					return false;
				}
				detectors[id].InputAvailable -= SignalStep;
				detectors[id].Stop();
				detectors[id].Dispose();
				detectors[id] = null;
				return true;
			}
		}

		/// <summary>
		/// Invoke the specified handler on the detection thread.
		/// </summary>
		/// <param name="handler">Handler to invoke.</param>
		public void Invoke(EventHandler handler) {
			if (handler == null)
				throw new ArgumentNullException("handler");
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
		/// Executes a single iteration of all started detectors.
		/// </summary>
		private void StepDetectors() {
			lock (stepSync) {
				while (toStep.Count > 0) {
					IPoseDetector detector = toStep.Dequeue();
					detector.Step();
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
			IPoseDetector detector = sender as IPoseDetector;
			if (detector != null) {
				lock (stepSync) {
					toStep.Enqueue(detector);
				}
			}
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
					DisconnectDetector(id);
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

