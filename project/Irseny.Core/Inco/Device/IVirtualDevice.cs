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
namespace Irseny.Core.Inco.Device {
	public interface IVirtualDevice {
		VirtualDeviceType DeviceType { get; }
		VirtualDeviceSendPolicy SendPolicy { get; set; }
		int SendRate { get; set; }
		bool SendRequired { get; }
		int DeviceIndex { get; }
		VirtualDeviceCapability[] GetSupportedCapabilities();
		object[] GetKeyHandles(VirtualDeviceCapability capability);
		int GetKeyNo(VirtualDeviceCapability capability);
		void BeginUpdate();
		bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, KeyState state);
		KeyState GetKeyState(VirtualDeviceCapability capability, object keyHandle);
		object[] GetModifiedKeys(VirtualDeviceCapability capability);
		void EndUpdate();
		void Send();
	}
}
