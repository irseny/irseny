using System;
using System.Collections.Generic;
using Irseny.Capture.Video;
using Irseny.Tracking;
using Irseny.Inco.Device;

namespace Irseny.Content.Profile {
	public class SetupProfile {
		Dictionary<int, CaptureSettings> captures = new Dictionary<int, CaptureSettings>(16);
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
		public void AddVideoCapture(int index, CaptureSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			captures[index] = settings;
		}
		public CaptureSettings GetVideoCapture(int index) {
			CaptureSettings result;
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