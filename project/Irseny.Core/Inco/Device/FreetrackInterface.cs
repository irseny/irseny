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
using System.Collections.Generic;

namespace Irseny.Core.Inco.Device {
	class FreetrackInterface : VirtualDevice {
		Dictionary<string, KeyState> keyState = new Dictionary<string, KeyState>(16);
		object[] keyHandles = new object[] {
			"X", "Y", "Z", "Yaw", "Pitch", "Roll"
		};
		List<object> modifiedKeys = new List<object>();
		public FreetrackInterface(int index) : base(index) {
			SendPolicy = VirtualDeviceSendPolicy.FixedRate;
			foreach (object handle in keyHandles) {
				keyState.Add(handle.ToString(), new KeyState(0, 0));
			}
		}
		public override VirtualDeviceType DeviceType {
			get { return VirtualDeviceType.TrackingInterface; }
		}
		public override VirtualDeviceCapability[] GetSupportedCapabilities() {
			return new VirtualDeviceCapability[] {
				VirtualDeviceCapability.Axis
			};
		}
		public override object[] GetKeyHandles(VirtualDeviceCapability capability) {
			return (object[])keyHandles.Clone();
		}
		public override int GetKeyNo(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				return keyHandles.Length;
			default:
				return 0;
			}
		}
		public override KeyState GetKeyState(VirtualDeviceCapability capability, object keyHandle) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				KeyState result;
				if (keyState.TryGetValue(keyHandle.ToString(), out result)) {
					return result;
				}
				return new KeyState(0, 0);
			default:
				return new KeyState(0, 0);
			}
		}
		public override bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, KeyState state) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				if (!keyState.ContainsKey(keyHandle.ToString())) {
					return false;
				}
				keyState[keyHandle.ToString()] = state;
				modifiedKeys.Add(keyHandle);
				return true;
			default:
				return false;
			}
		}
		public override object[] GetModifiedKeys(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				return modifiedKeys.ToArray();
			default:
				return new object[0];
			}
		}
		public override void BeginUpdate() {
			base.BeginUpdate();
			modifiedKeys.Clear();
		}
	}
}
