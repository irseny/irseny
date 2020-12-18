using System;
using System.Collections.Generic;
using Irseny.Core.Sensors;
using Irseny.Core.Tracking;
using Irseny.Core.Inco.Device;

namespace Irseny.Main.Content.Profile {
	public class SetupProfile {
		Dictionary<int, SensorSettings> captures = new Dictionary<int, SensorSettings>(16);
		Dictionary<int, TrackerSettings> trackers = new Dictionary<int, TrackerSettings>(16);
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
		public void AddVideoCapture(int index, SensorSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			captures[index] = settings;
		}
		public SensorSettings GetVideoCapture(int index) {
			SensorSettings result;
			if (!captures.TryGetValue(index, out result)) {
				return null;
			}
			return result;
		}
		public ICollection<int> TrackerIndexes {
			get { return trackers.Keys; }
		}
		public void AddTracker(int index, TrackerSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			trackers[index] = settings;
		}
		public TrackerSettings GetTracker(int index) {
			TrackerSettings result;
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