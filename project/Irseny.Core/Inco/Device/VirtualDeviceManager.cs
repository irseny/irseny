// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Irseny.Core.Log;

namespace Irseny.Core.Inco.Device {
	public class VirtualDeviceManager {
		static VirtualDeviceManager instance = null;
		static readonly object instanceSync = new object();
		static Thread instanceThread;

		volatile int stopSignal = 0;
		volatile int invokeSignal = 0;
		volatile int sendSignal = 0;
		volatile int ftRequestNo = 0;


		readonly VirtualDeviceContext context = new VirtualDeviceContext();
		readonly object contextSync = new object();
		readonly object invokeSync = new object();
		readonly object deviceSync = new object();
		readonly object ftProcessSync = new object();

		Process ftProcess = null;
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
					LogManager.Instance.Log(LogEntry.CreateError(this, "Cannot inject events to OS"));
				}
			}
			while (stopSignal < 1) {
				if (Interlocked.CompareExchange(ref invokeSignal, 0, 1) == 1) {
					InvokePending();
				}
				if (Interlocked.CompareExchange(ref sendSignal, 0, 1) == 1) {
					SendState();
				}
				HandleFTRequest();
				Thread.Sleep(1);
			}
			InvokePending();
			SendState();
			context.Destroy();
			Interlocked.Decrement(ref stopSignal);
			ftRequestNo = 0;
			HandleFTRequest();
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
			int result = -1;
			lock (deviceSync) {
				// find empty device index
				for (int i = 0; i < devices.Count; i++) {
					if (devices[i] == null) {
						result = i;
						break;
					}
				}
				// add at empty index or append to end
				if (result < 0) {
					result = devices.Count;
					devices.Add(device);
				} else {
					devices[result] = device;
				}
			}
			if (device.DeviceType == VirtualDeviceType.TrackingInterface) {
				Interlocked.Increment(ref ftRequestNo);
			}
			return result;
		}
		/// <summary>
		/// Reconnects the device with the given ID.
		/// This method can be used to disconnect and connect a device without changing its ID.
		/// </summary>
		/// <returns><c>true</c>, if the operation was successful, <c>false</c> otherwise.</returns>
		/// <param name="deviceId">Device identifier.</param>
		/// <param name="device">New device specification.</param>
		public bool ReconnectDevice(int deviceId, IVirtualDevice device) {
			Process process = new Process();
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
				if (oldDevice.DeviceType == VirtualDeviceType.TrackingInterface) {
					Interlocked.Decrement(ref ftRequestNo);
				}
				if (!context.ConnectDevice(device)) {
					return false;
				}
				if (device.DeviceType == VirtualDeviceType.TrackingInterface) {
					Interlocked.Increment(ref ftRequestNo);
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
			if (device.DeviceType == VirtualDeviceType.TrackingInterface) {
				Interlocked.Decrement(ref ftRequestNo);
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
		private void HandleFTRequest() {
			if (ftRequestNo > 0) {
				lock (ftProcessSync) {
					if (ftProcess == null) {
						ftProcess = new Process();
#if WINDOWS
						ftProcess.StartInfo.FileName = "TrackIR.exe";
#elif LINUX
						ftProcess.StartInfo.FileName = "TrackIR";
#endif
						ftProcess.StartInfo.CreateNoWindow = true;
						ftProcess.StartInfo.UseShellExecute = false;
						ftProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						try {
							ftProcess.Start();
						} catch (System.ComponentModel.Win32Exception) {
							LogManager.Instance.LogWarning(this, "Failed to start Freetrack dummy process");
						} catch (IOException) {
							LogManager.Instance.LogWarning(this, "Failed to start Freetrack dummy process");
						}
						try {
							ftProcess.PriorityClass = ProcessPriorityClass.Idle;
						} catch (InvalidOperationException) {
							LogManager.Instance.LogMessage(this, "Cannot set Freetrack dummy process to low priority");
						}
					}
				}
			} else {
				lock (ftProcessSync) {
					if (ftProcess != null) {
						try {
							ftProcess.Kill();
						} catch (InvalidOperationException) {
							LogManager.Instance.LogWarning(this, "Failed to stop Freetrack dummy process");
						} catch (IOException) {
							LogManager.Instance.LogWarning(this, "Failed to stop Freetrack dummy process");
						}
						ftProcess.Dispose();
						ftProcess = null;
					}
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