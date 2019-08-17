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
		/// <summary>
		/// Executes the operation loop.
		/// Returns when stopped.
		/// </summary>
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
		/// <summary>
		/// Signals this instance to stop the operation loop.
		/// </summary>
		public void Stop() {
			Interlocked.Increment(ref stopSignal);
		}
		/// <summary>
		/// Invokes all pending event handlers.
		/// </summary>
		private void InvokePending() {
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
		/// <summary>
		/// Adds the given event handler to the pending list.
		/// Signals pending event handler invocation.
		/// </summary>
		/// <param name="handler">Handler to invoke.</param>
		public void Invoke(EventHandler handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
			}
			invokeSignal = 1;
		}
		/// <summary>
		/// Connects the given device.
		/// </summary>
		/// <returns>Id to reference the device.</returns>
		/// <param name="device">Device specification.</param>
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
		/// <summary>
		/// Reconnects the device with the given ID.
		/// This method can be used to disconnect and connect a device without changing its ID.
		/// </summary>
		/// <returns><c>true</c>, if the operation was successful, <c>false</c> otherwise.</returns>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="device">New device specification.</param>
		public bool ReconnectDevice(int deviceId, IVirtualDevice device) {
			if (deviceId < 0) throw new ArgumentOutOfRangeException("deviceId");
			if (device == null) throw new ArgumentOutOfRangeException("device");
			// find the old device
			IVirtualDevice oldDevice;
			lock (deviceSync) {
				if (deviceId < 0 || deviceId >= devices.Count) {
					return false;
				}
				if (devices[deviceId] == null) {
					return false;
				}
				oldDevice = devices[deviceId];
			}
			// internal reconnect
			lock (contextSync) {
				if (!context.DisconnectDevice(oldDevice)) {
					return false;
				}
				if (!context.ConnectDevice(device)) {
					return false;
				}
			}
			// exchange the devices
			// since the old device is still officially connected the device id has not been reassigned
			lock (deviceSync) {
				devices[deviceId] = device;
			}
			return true;
		}
		/// <summary>
		/// Gets the device with the given ID.
		/// </summary>
		/// <returns>The device or null if it is not available.</returns>
		/// <param name="deviceId">Device identifier.</param>
		public IVirtualDevice GetDevice(int deviceId) {
			lock (deviceSync) {
				if (deviceId < 0 || deviceId >= devices.Count) {
					return null;
				}
				return devices[deviceId];
			}
		}
		/// <summary>
		/// Disconnects the device with the given ID from this instance.
		/// </summary>
		/// <returns><c>true</c>, if the operation wass successful, <c>false</c> otherwise.</returns>
		/// <param name="deviceId">Device identifier.</param>
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
		/// <summary>
		/// Signals that devices state is updated.
		/// </summary>
		public void BeginUpdate() {
			// nothing to do
		}
		/// <summary>
		/// Signals that device state updates have finished.
		/// </summary>
		public void EndUpdate() {
			sendSignal = 1;
		}
		/// <summary>
		/// Sends device state updates to the OS or HID backend.
		/// </summary>
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
		/// <summary>
		/// Starts the operation loop of the given manager in a new thread.
		/// Stops the previously active manager.
		/// </summary>
		/// <param name="manager">Manager.</param>
		public static void MakeInstance(VirtualDeviceManager manager) {
			lock (instanceSync) {
				if (VirtualDeviceManager.instance != null) {
					VirtualDeviceManager.instance.Stop();
					VirtualDeviceManager.instanceThread.Join(2048);
					if (VirtualDeviceManager.instanceThread.IsAlive) {
						LogManager.Instance.LogWarning(manager, "Virtual device thread does not terminate. Aborting.");
						VirtualDeviceManager.instanceThread.Abort();
					}
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