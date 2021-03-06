﻿// This file is part of Irseny.
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
using System.Collections.Generic;

namespace Irseny.Core.Inco.Device {
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
			contextHandle = Ivj.CreateContext();
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
				constructionInfo = Ivj.AllocKeyboardConstructionInfo();
				handle = Ivj.ConnectKeyboard(contextHandle, constructionInfo);
				Ivj.FreeKeyboardConstructionInfo(constructionInfo);
				break;
			case VirtualDeviceType.TrackingInterface:
				constructionInfo = Ivj.AllocFreetrackConstructionInfo();
				handle = Ivj.ConnectFreetrackInterface(contextHandle, constructionInfo);
				Ivj.FreeFreetrackConstructionInfo(constructionInfo);
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
				KeyState state = device.GetKeyState(VirtualDeviceCapability.Key, key);
				int keyIndex = Ivj.GetKeyboardKeyIndex(key);
				if (keyIndex > -1) {
					Ivj.SetKeyboardKey(handle, keyIndex, state.SmoothPressed);
				}
			}
			bool result = Ivj.SendKeyboard(handle);
			device.Send();
			return result;

		}
		private bool SendFreetrackInterface(IVirtualDevice device, IntPtr handle) {
			object[] modifiedKeys = device.GetModifiedKeys(VirtualDeviceCapability.Axis);
			foreach (object key in modifiedKeys) {
				KeyState state = device.GetKeyState(VirtualDeviceCapability.Axis, key);
				int axisIndex = Ivj.GetFreetrackAxisIndex(key);
				if (axisIndex > -1) {
					Ivj.SetFreetrackAxis(handle, axisIndex, state.SmoothAxis, state.RawAxis);
				}
			}
			bool result = Ivj.SendFreetrackInterface(handle);
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
				Ivj.DisconnectKeyboard(contextHandle, handle);
				break;
			case VirtualDeviceType.TrackingInterface:
				Ivj.DisconnectFreetrackInterface(contextHandle, handle);
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
					Ivj.DisconnectKeyboard(contextHandle, pair.Value);
					break;
				case VirtualDeviceType.TrackingInterface:
					Ivj.DisconnectFreetrackInterface(contextHandle, pair.Value);
					break;
				default:
					throw new NotImplementedException();
					// TODO: implement missing cases
				}
			}
			deviceHandles.Clear();
			bool result = Ivj.DestroyContext(contextHandle);
			contextHandle = IntPtr.Zero;
			return result;
		}
		// TODO: create context, add/remove devices
	}
}

