using System;
using System.Collections.Generic;

namespace Irseny.Inco.Device {
	public class VirtualDeviceContext {
		IntPtr contextHandle = IntPtr.Zero;
		Dictionary<IVirtualDevice, IntPtr> deviceHandles = new Dictionary<IVirtualDevice, IntPtr>(16);

		public VirtualDeviceContext() {
		}
		public bool Created {
			get { return contextHandle != IntPtr.Zero; }
		}
		public bool Create() {
			if (contextHandle != IntPtr.Zero) {
				return true;
			}
			contextHandle = Extrack.Ivj.CreateContext();
			return contextHandle != IntPtr.Zero;
		}
		public bool ConnectDevice(IVirtualDevice device) {
			if (device == null) throw new ArgumentNullException("device");
			if (!Created) {
				return false;
			}
			if (deviceHandles.ContainsKey(device)) {
				return true;
			}
			IntPtr handle = IntPtr.Zero;
			IntPtr constructionInfo;
			switch (device.DeviceType) {
			case VirtualDeviceType.Keyboard:
				constructionInfo = Extrack.Ivj.AllocKeyboardConstructionInfo();
				handle = Extrack.Ivj.ConnectKeyboard(contextHandle, constructionInfo);
				Extrack.Ivj.FreeKeyboardConstructionInfo(constructionInfo);
				break;
			case VirtualDeviceType.TrackingInterface:
				constructionInfo = Extrack.Ivj.AllocFreetrackConstructionInfo();
				handle = Extrack.Ivj.ConnectFreetrackInterface(contextHandle, constructionInfo);
				Extrack.Ivj.FreeFreetrackConstructionInfo(constructionInfo);
				break;
			default:
				throw new NotImplementedException();
			}
			if (handle == IntPtr.Zero) {
				return false;
			}
			deviceHandles.Add(device, handle);
			return true;
		}
		public bool SendDevice(IVirtualDevice device) {
			if (device == null) throw new ArgumentNullException("device");
			if (!Created) {
				return false;
			}
			IntPtr handle;
			if (!deviceHandles.TryGetValue(device, out handle)) {
				return false;
			}
			switch (device.DeviceType) {
			case VirtualDeviceType.Keyboard:
				return SendKeyboard(device, handle);
			case VirtualDeviceType.TrackingInterface:
				return SendFreetrackInterface(device, handle);
			default:
				throw new NotImplementedException();
			}
		}
		private bool SendKeyboard(IVirtualDevice device, IntPtr handle) {
			object[] modifiedKeys = device.GetModifiedKeys(VirtualDeviceCapability.Key);
			foreach (object key in modifiedKeys) {
				float state = device.GetKeyState(VirtualDeviceCapability.Key, key);
				int keyIndex = Extrack.Ivj.GetKeyboardKeyIndex(key);
				if (keyIndex > -1) {
					Extrack.Ivj.SetKeyboardKey(handle, keyIndex, state);
				}
			}
			bool result = Extrack.Ivj.SendKeyboard(handle);
			device.Send();
			return result;

		}
		private bool SendFreetrackInterface(IVirtualDevice device, IntPtr handle) {
			object[] modifiedKeys = device.GetModifiedKeys(VirtualDeviceCapability.Axis);
			foreach (object key in modifiedKeys) {
				float state = device.GetKeyState(VirtualDeviceCapability.Axis, key);
				int axisIndex = Extrack.Ivj.GetFreetrackAxisIndex(key);
				if (axisIndex > -1) {
					Extrack.Ivj.SetFreetrackAxis(handle, axisIndex, state);
				}
			}
			bool result = Extrack.Ivj.SendFreetrackInterface(handle);
			device.Send();
			return result;
		}
		public bool DisconnectDevice(IVirtualDevice device) {
			if (device == null) throw new ArgumentNullException("device");
			if (!Created) {
				return false;
			}
			IntPtr handle;
			if (!deviceHandles.TryGetValue(device, out handle)) {
				return true;
			}
			switch (device.DeviceType) {
			case VirtualDeviceType.Keyboard:
				Extrack.Ivj.DisconnectKeyboard(contextHandle, handle);
				break;
			case VirtualDeviceType.TrackingInterface:
				Extrack.Ivj.DisconnectFreetrackInterface(contextHandle, handle);
				break;
			default:
				throw new NotImplementedException();
			}
			deviceHandles.Remove(device);
			return true;
		}
		public bool Destroy() {
			if (!Created) {
				return true;
			}
			// destroy devices
			foreach (var pair in deviceHandles) {
				switch (pair.Key.DeviceType) {
				case VirtualDeviceType.Keyboard:
					Extrack.Ivj.DisconnectKeyboard(contextHandle, pair.Value);
					break;
				case VirtualDeviceType.TrackingInterface:
					Extrack.Ivj.DisconnectFreetrackInterface(contextHandle, pair.Value);
					break;
				default:
					throw new NotImplementedException();
					// TODO: implement missing cases
				}
			}
			deviceHandles.Clear();
			bool result = Extrack.Ivj.DestroyContext(contextHandle);
			contextHandle = IntPtr.Zero;
			return result;
		}
		// TODO: create context, add/remove devices
	}
}

