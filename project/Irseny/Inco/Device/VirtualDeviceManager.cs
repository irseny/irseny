using System;
using System.Threading;
using System.Collections.Generic;
using Irseny.Log;

namespace Irseny.Inco.Device {
	public class VirtualDeviceManager {
		static VirtualDeviceManager instance = null;
		static readonly object instanceSync = new object();
		static Thread instanceThread;

		volatile int stopSignal = 0;
		volatile int invokeSignal = 0;
		volatile int sendSignal = 0;

		readonly VirtualDeviceContext context = new VirtualDeviceContext();
		readonly object contextSync = new object();
		readonly object invokeSync = new object();
		readonly object deviceSync = new object();

		List<IVirtualDevice> devices = new List<IVirtualDevice>(16);
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();

		public VirtualDeviceManager() {
		}
		public static VirtualDeviceManager Instance {
			get { return instance; }
		}
		private void Start() {
			lock (contextSync) {
				if (!context.Create()) {
					// TODO: make this visible in UI
					LogManager.Instance.Log(LogMessage.CreateError(this, "Cannot inject events to OS"));
				}
			}
			while (stopSignal < 1) {
				if (Interlocked.CompareExchange(ref invokeSignal, 0, 1) == 1) {
					InvokePending();
				}
				if (Interlocked.CompareExchange(ref sendSignal, 0, 1) == 1) {
					SendState();
				}
				Thread.Sleep(1);
			}
			InvokePending();
			SendState();
			context.Destroy();
			Interlocked.Decrement(ref stopSignal);
		}
		public void Stop() {
			Interlocked.Increment(ref stopSignal);
		}
		public void InvokePending() {
			Queue<EventHandler> pending;
			lock (invokeSync) {
				pending = toInvoke;
				toInvoke = new Queue<EventHandler>();
			}
			var args = new EventArgs();
			foreach (EventHandler handler in pending) {
				handler(this, args);
			}
			if (sendSignal > 0) {
				sendSignal -= 1;
				SendState();
			}
		}
		public void Invoke(EventHandler handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
			}
			invokeSignal = 1;
		}
		public int ConnectDevice(IVirtualDevice device) {
			if (device == null) throw new ArgumentNullException("device");
			lock (contextSync) {
				if (!context.ConnectDevice(device)) {
					return -1;
				}
			}
			lock (deviceSync) {
				// find empty device index
				int mountIndex = -1;
				for (int i = 0; i < devices.Count; i++) {
					if (devices[i] == null) {
						mountIndex = i;
						break;
					}
				}
				// add at empty index or append to end
				if (mountIndex < 0) {
					mountIndex = devices.Count;
					devices.Add(device);
				} else {
					devices[mountIndex] = device;
				}
				return mountIndex;
			}
		}
		public IVirtualDevice GetDevice(int deviceId) {
			lock (deviceSync) {
				if (deviceId < 0 || deviceId >= devices.Count) {
					return null;
				}
				return devices[deviceId];
			}
		}
		public bool DisconnectDevice(int deviceId) {
			IVirtualDevice device = null;

			lock (deviceSync) {
				if (deviceId < 0 || deviceId >= devices.Count) {
					return false;
				}
				device = devices[deviceId];
			}
			lock (contextSync) {
				if (!context.DisconnectDevice(device)) {
					return false;
				}
			}
			lock (deviceSync) {
				if (deviceId == devices.Count - 1) {
					devices.RemoveAt(deviceId);
				} else {
					devices[deviceId] = null;
				}
			}
			return true;
		}

		public void BeginUpdate() {
			// nothing to do
		}
		public void EndUpdate() {
			sendSignal = 1;
		}
		private void SendState() {
			var toSend = new Queue<IVirtualDevice>();
			lock (deviceSync) {
				for (int i = 0; i < devices.Count; i++) {
					if (devices[i] != null && devices[i].SendRequired) {
						toSend.Enqueue(devices[i]);
					}
				}
			}
			lock (contextSync) {
				while (toSend.Count > 0) {
					IVirtualDevice device = toSend.Dequeue();
					context.SendDevice(device);
				}
			}
		}

		public static void MakeInstance(VirtualDeviceManager manager) {
			lock (instanceSync) {
				if (VirtualDeviceManager.instance != null) {
					VirtualDeviceManager.instance.Stop();
					VirtualDeviceManager.instanceThread.Join();
					VirtualDeviceManager.instanceThread = null;
					VirtualDeviceManager.instance = null;
				}
				if (manager != null) {
					VirtualDeviceManager.instance = manager;
					VirtualDeviceManager.instanceThread = new Thread(manager.Start);
					VirtualDeviceManager.instanceThread.Start();
				}

			}
		}
	}
}