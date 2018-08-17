using System;
using System.Threading;
using System.Collections.Generic;

namespace Irseny.Inco.Device {
	public class VirtualDeviceManager {
		static VirtualDeviceManager instance = null;
		static readonly object instanceSync = new object();
		static Thread instanceThread;

		volatile int stopSignal = 0;
		readonly AutoResetEvent invokeSignal = new AutoResetEvent(false);
		readonly object invokeSync = new object();
		readonly object deviceSync = new object();
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

		public void MakeInstance(VirtualDeviceManager manager) {
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