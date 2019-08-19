using System;
using System.Threading;
using System.Collections.Generic;
using Irseny.Log;

namespace Irseny.Tracking {
	public class DetectionSystem {
		static object instanceSync = new object();
		static DetectionSystem instance = null;
		static Thread instanceThread = null;

		volatile bool running = false;
		readonly object invokeSync = new object();
		readonly object stepSync = new object();
		readonly object trackerSync = new object();
		readonly object modelSync = new object();
		AutoResetEvent invokeSignal = new AutoResetEvent(false);
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();
		Queue<IPoseTracker> toStep = new Queue<IPoseTracker>();
		List<IPoseTracker> trackers = new List<IPoseTracker>(4);
		List<IObjectModel> models = new List<IObjectModel>(4);


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
		/// Connects the given tracker.
		/// </summary>
		/// <returns>Identifier for the tracker. Less than 0 if starting was not successful.</returns>
		/// <param name="tracker">Tracker.</param>
		/// <param name="settings">Settings.</param>
		public int StartTracker(IPoseTracker tracker, TrackerSettings settings) {
			if (tracker == null) throw new ArgumentNullException("tracker");
			if (settings == null) throw new ArgumentNullException("settings");
			lock (trackerSync) {
				int id;
				// find unused index
				for (id = 0; id < trackers.Count; id++) {
					if (trackers[id] == null) {
						break;
					}
				}
				// add or insert tracker
				if (id < trackers.Count) {
					trackers[id] = tracker;
				} else {
					trackers.Add(tracker);
				}
				// the tracker signals when it has data to process
				// to let it process we pick up the signal
				tracker.InputAvailable += SignalStep;
				if (!tracker.Start(settings)) {
					return -1;
				}
				return id;
			}
		}

		/// <summary>
		/// Gets the tracker specified by the given identifier.
		/// </summary>
		/// <returns>The tracker. Null if it does not exist.</returns>
		/// <param name="id">Tracker identifier previously returned by <see cref="StartTracker"/>.</param>
		public IPoseTracker GetTracker(int id) {
			lock (trackerSync) {
				if (id < 0 || id >= trackers.Count) {
					return null;
				}
				return trackers[id];
			}
		}

		/// <summary>
		/// Gets the tracker specified by the given identifier and type.
		/// </summary>
		/// <returns>The tracker. Null if it does not exist or there is a type mismatch.</returns>
		/// <param name="id">Tracker identifier previously returned by <see cref="StartTracker"/>.</param>
		/// <param name="fallback">Default value.</param>
		/// <typeparam name="Ttrack">The tracker type.</typeparam>
		public Ttrack GetTracker<Ttrack>(int id, Ttrack fallback) where Ttrack : IPoseTracker {
			lock (trackerSync) {
				if (id < 0 || id >= trackers.Count) {
					return fallback;
				}
				if (trackers[id] is Ttrack) {
					return (Ttrack)trackers[id];
				}
				return fallback;
			}
		}

		/// <summary>
		/// Stops, disposes and removes the tracker specified by the given identifier.
		/// </summary>
		/// <returns><c>true</c>, if the tracker existed, <c>false</c> otherwise.</returns>
		/// <param name="id">Tracker identifier previously returned by <see cref="StartTracker"></see>.</param>
		public IPoseTracker StopTracker(int id) {
			lock (trackerSync) {
				if (id < 0 || id >= trackers.Count) {
					return null;
				}
				if (trackers[id] == null) {
					return null;
				}
				IPoseTracker result = trackers[id];
				trackers[id] = null;
				result.InputAvailable -= SignalStep;
				result.Stop();
				return result;
			}
		}
		public int RegisterModel(IObjectModel model) {
			if (model == null) throw new ArgumentNullException("model");
			lock (modelSync) {
				for (int i = 0; i < models.Count; i++) {
					if (models[i] == null) {
						models[i] = model;
						return i;
					}
				}
				int index = models.Count;
				models.Add(model);
				return index;
			}
		}
		public bool RemoveModel(int id) {
			lock (modelSync) {
				if (id < 0 || id >= models.Count) {
					return false;
				}
				if (models[id] == null) {
					return false;
				}
				models[id] = null;
				return true;
			}
		}
		public bool ReplaceModel(int id, IObjectModel model) {
			lock (modelSync) {
				if (id < 0 || id >= models.Count) {
					return false;
				}
				if (models[id] == null) {
					return false;
				}
				models[id] = model;
				return true;
			}
		}
		public IObjectModel GetModel(int id) {
			lock (modelSync) {
				if (id < 0 || id >= models.Count) {
					return null;
				}
				return models[id];
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
		/// Executes a single iteration of all started trackers.
		/// </summary>
		private void StopTrackers() {
			lock (stepSync) {
				while (toStep.Count > 0) {
					IPoseTracker tracker = toStep.Dequeue();
					tracker.Step();
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
		/// Signals this instance to step the trackers due to available data to process.
		/// </summary>
		/// <param name="sender">Ignnored.</param>
		/// <param name="args">Ignored.</param>
		private void SignalStep(object sender, EventArgs args) {
			IPoseTracker tracker = sender as IPoseTracker;
			if (tracker != null) {
				lock (stepSync) {
					toStep.Enqueue(tracker);
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
				StopTrackers();
			}
			// cleanup
			lock (trackerSync) {
				for (int id = 0; id < trackers.Count; id++) {
					StopTracker(id);
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
					DetectionSystem.instanceThread.Join(2048);
					if (DetectionSystem.instanceThread.IsAlive) {
						LogManager.Instance.LogWarning(instance, "Detection system thread does not terminate. Aborting.");
						DetectionSystem.instanceThread.Abort();
					}
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

