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
using Irseny.Core.Util;
using Irseny.Core.Sensors;
using Irseny.Core.Tracking;
using Irseny.Core.Inco.Device;

namespace Irseny.Main.Content.Profile {
	public class SetupProfile {
		Dictionary<int, EquipmentSettings> captures = new Dictionary<int, EquipmentSettings>(16);
		Dictionary<int, EquipmentSettings> trackers = new Dictionary<int, EquipmentSettings>(16);
		Dictionary<int, CapInputRelay> bindings = new Dictionary<int, CapInputRelay>(16);
		Dictionary<int, VirtualDeviceSettings> devices = new Dictionary<int, VirtualDeviceSettings>(16);

		public SetupProfile(string name) {
			if (name == null) throw new ArgumentNullException("name");
			Name = name;
		}
		public string Name { get; private set; }

		public ICollection<int> VideoCaptureIndexes {
			get { return captures.Keys; }
		}
		public void AddVideoCapture(int index, EquipmentSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			captures[index] = settings;
		}
		public EquipmentSettings GetVideoCapture(int index) {
			EquipmentSettings result;
			if (!captures.TryGetValue(index, out result)) {
				return null;
			}
			return result;
		}
		public ICollection<int> TrackerIndexes {
			get { return trackers.Keys; }
		}
		public void AddTracker(int index, EquipmentSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			trackers[index] = settings;
		}
		public EquipmentSettings GetTracker(int index) {
			EquipmentSettings result;
			if (!trackers.TryGetValue(index, out result)) {
				return null;
			}
			return result;
		}
		public ICollection<int> DeviceIndexes {
			get { return devices.Keys; }
		}
		public void AddDevice(int index, VirtualDeviceSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			devices[index] = settings;
		}
		public VirtualDeviceSettings GetDevice(int index) {
			VirtualDeviceSettings result;
			if (!devices.TryGetValue(index, out result)) {
				return null;
			}
			return result;
		}
		public ICollection<int> BindingIndexes {
			get { return bindings.Keys; }
		}
		public void AddBindings(int index, CapInputRelay settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			bindings[index] = settings;
		}
		public CapInputRelay GetBindings(int index) {
			CapInputRelay result;
			if (!bindings.TryGetValue(index, out result)) {
				return null;
			}
			return result;
		}
	}
}