using System;
using System.Collections.Generic;
using System.Threading;
using Irseny.Core.Log;
using Irseny.Core.Sensors.VideoCapture;

namespace Irseny.Core.Sensors {
	/// <summary>
	/// Handles sensor management.
	/// Provides a collection of <see cref="ISensorBase"/> instances alongside functionlity 
	/// to simplify sensor event handling and data capturing.
	/// </summary>
	public class CaptureSystem {
		static CaptureSystem instance = null;

		readonly int MaxSensorNo = 16;

		Thread thread;
		CancellationTokenSource threadCancel;

		readonly object invokeSync;
		readonly object sensorSync;
		readonly object observableSync;
		Queue<EventHandler> toInvoke;
		readonly AutoResetEvent invokeSignal;
		readonly ISensorBase[] sensors;
		readonly SensorObservable[] observables;
		readonly IDisposable[] subscriptions;


		public static CaptureSystem Instance {
			get { return instance; }
		}

		public CaptureSystem() {
			invokeSync = new object();
			sensorSync = new object();
			observableSync = new object();
			thread = null;
			threadCancel = null;
			toInvoke = new Queue<EventHandler>();
			invokeSignal = new AutoResetEvent(false);
			sensors = new ISensorBase[MaxSensorNo];
			subscriptions = new IDisposable[MaxSensorNo];
			observables = new SensorObservable[MaxSensorNo];
			for (int i = 0; i < observables.Length; i++) {
				observables[i] = new SensorObservable();
			}
		}

		public static void MakeInstance(CaptureSystem instance) {
			if (CaptureSystem.instance != null) {
				CaptureSystem.instance.WaitStop(20480);
			}
			CaptureSystem.instance = instance;
			if (instance != null) {
				instance.Start();
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
		private void ProcessSensors() {
			ISensorBase[] toProcess;
			lock (sensorSync) {
				if (sensors.Length > 0) {
					toProcess = new ISensorBase[sensors.Length];
					Array.Copy(sensors, toProcess, sensors.Length);
				} else {
					return;
				}
			}
			// process all available sensors
			// and relay captured data
			for (int i = 0; i < toProcess.Length; i++) {
				if (toProcess[i] == null || !toProcess[i].Capturing) {
					continue;
				}
				SensorDataPacket packet = toProcess[i].Process(-1);
				if (packet == null) {
					continue;
				}
				SensorObservable observable;
				lock (observableSync) {
					observable = observables[i];
				}
				observable.OnDataAvailable(packet);
			}
		}
		private void Start() {
			if (thread != null) {
				return;
			}
			threadCancel = new CancellationTokenSource();
			thread = new Thread(() => Run(threadCancel.Token)); 
			thread.Start();
		}
		private void WaitStop(int timeout=2048) {
			if (thread == null) {
				return;
			}
			threadCancel.Cancel();
			invokeSignal.Set(); // wake up the operation invoking thread for exit
			thread.Join(timeout);
			if (thread.IsAlive) {
				LogManager.Instance.LogWarning(this, "Capture thread does not terminate. Aborting");
				thread.Abort();
			}
			thread = null;
			threadCancel = null;
		}
		private void Run(CancellationToken cancelToken) {
			while (!cancelToken.IsCancellationRequested) {
				invokeSignal.WaitOne(4);
				InvokePending();
				ProcessSensors();
			}
			// cleanup: stop all running sensors
			// TODO: make sure that this deallocates all external memory
			for (int i = 0; i < MaxSensorNo; i++) {
				StopSensor(i);
			}
		}

		public int ConnectSensor(ISensorBase sensor, int preferredIndex=-1) {
			if (sensor == null) throw new ArgumentNullException("sensor");
			int iSensor = -1;
			lock (sensorSync) {
				// first try to assign the sensor to its preferred index
				if (preferredIndex >= 0 && preferredIndex < sensors.Length) {
					
					if (sensors[preferredIndex] == null) {
						sensors[preferredIndex] = sensor;
						iSensor = preferredIndex;
					}
				}
				// if the index is already taken, find the next unassigned index
				if (iSensor < 0) {
					for (int i = 0; i < sensors.Length; i++) {
						if (sensors[i] == null) {
							sensors[i] = sensor;
							iSensor = i;
							break;
						}
					}
				}
				if (iSensor < 0) {
					// all indexes assigned, increase storage size
					return -1;
				}
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[iSensor];
			}
			observable.OnConnected(sensor);
			return iSensor;
		}
		public bool DisconnectSensor(int index) {
			ISensorBase sensor;
			bool stopped = false;
			lock (sensorSync) {
				// free the index if it has been assigned
				if (index < 0 || index >= sensors.Length) {
					return false;
				}
				if (sensors[index] == null) {
					return false;
				}
				sensor = sensors[index];
				sensors[index] = null;
				if (sensor.Capturing) {
					stopped = sensor.Stop();
				}
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[index];
			}
			if (stopped) {
				observable.OnStopped(sensor);
			}
			observable.OnDisconnected(sensor);

			return true;
		}
		public bool StartSensor(int index) {
			ISensorBase sensor;
			lock (sensorSync) {
				if (index < 0 || index >= sensors.Length) {
					return false;
				}
				if (sensors[index] == null) {
					return false;
				}
				sensor = sensors[index];
			}
			if (!sensor.Start()) {
				return false;
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[index];
			}
			observable.OnStarted(sensor);
			return true;
		}
		public bool StopSensor(int index) {
			ISensorBase sensor;
			lock (sensorSync) {
				if (index < 0 || index >= sensors.Length || sensors[index] == null) {
					return false;
				}
				sensor = sensors[index];
			}
			if (!sensor.Stop()) {
				return false;
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[index];
			}
			observable.OnStopped(sensor);
			return true;
		}

		public ISensorBase GetSensor(int index) {
			lock (sensorSync) {
				if (index < 0 || index >= sensors.Length) {
					return null;
				}
				return sensors[index];
			}
		}
		public ISensorObservable ObserveSensor(int index) {
			lock (observableSync) {
				if (index < 0 || index >= observables.Length) {
					return null;
				}
				return observables[index];
			}
		}

		public void Invoke(EventHandler handler) {
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
				invokeSignal.Set();
			}
		}

	}
}

