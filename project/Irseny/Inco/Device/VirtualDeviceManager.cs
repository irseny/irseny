using System;
using System.Threading;
using System.Collections.Generic;

namespace Irseny.Inco.Device {
	public class VirtualDeviceManager {
		static VirtualDeviceManager instance = null;
		static readonly object instanceSync = new object();
		static Thread instanceThread;

		volatile int stopSignal = 0;
		readonly VirtualDeviceContext context = new VirtualDeviceContext();
		readonly AutoResetEvent invokeSignal = new AutoResetEvent(false);
		readonly object invokeSync = new object();
		readonly object deviceSync = new object();
		List<IVirtualDevice> devices = new List<IVirtualDevice>(16);
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();

		public VirtualDeviceManager() {
		}
		public static VirtualDeviceManager Instance {
			get { return instance; }
		}
		public void Start() {
			while (stopSignal < 1) {
				invokeSignal.WaitOne();
				InvokePending();
			}
			stopSignal -= 1;
		}
		public void Stop() {
			stopSignal += 1;
			invokeSignal.Set();
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
		}
		public void Invoke(EventHandler handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
			}
			invokeSignal.Set();
		}
		public int MountDevice(IVirtualDevice device) {
			if (device == null) throw new ArgumentNullException("device");
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
				// TODO: initialize device
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
		public IVirtualDevice UnmountDevice(int deviceId) {
			lock (deviceSync) {
				if (deviceId < 0 || deviceId >= devices.Count) {
					return null;
				}
				IVirtualDevice result = devices[deviceId];
				// TODO: uninitialize device
				return result;
			}
		}
		public void BeginUpdate() {
			// nothing to do
		}
		public void EndUpdate() {
			// TODO: send 
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