using System;
using System.Collections.Generic;
using Irseny.Capture.Video;
using Irseny.Tracap;
using Irseny.Inco.Device;

namespace Irseny.Content.Profile {
	public class SetupProfile {
		Dictionary<int, CaptureSettings> captures = new Dictionary<int, CaptureSettings>(16);
		Dictionary<int, ICapTrackerOptions> trackers = new Dictionary<int, ICapTrackerOptions>(16);
		Dictionary<int, CapInputRelay> bindings = new Dictionary<int, CapInputRelay>(16);
		Dictionary<int, VirtualDeviceType> devices = new Dictionary<int, VirtualDeviceType>(16);

		public SetupProfile(string name) {
			if (name == null) throw new ArgumentNullException("name");
			Name = name;
		}
		public string Name { get; private set; }

		public ICollection<int> VideoCaptureIndexes {
			get { return captures.Keys; }
		}
		public void AddVideoCapture(int index, CaptureSettings profile) {
			if (profile == null) throw new ArgumentNullException("profile");
			captures[index] = profile;
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
		public void AddTracker(int index, ICapTrackerOptions profile) {
			if (profile == null) throw new ArgumentNullException("profile");
			trackers[index] = profile;
		}
		public ICapTrackerOptions GetTracker(int index) {
			ICapTrackerOptions result;
			if (!trackers.TryGetValue(index, out result)) {
				return null;
			}
			return result;
		}
		public ICollection<int> DeviceIndexes {
			get { return devices.Keys; }
		}
		public void AddDevice(int index, VirtualDeviceType profile) {
			if (profile == null) throw new ArgumentNullException("profile");
			devices[index] = profile;
		}
		public VirtualDeviceType GetDevice(int index) {
			VirtualDeviceType result;
			if (!devices.TryGetValue(index, out result)) {
				return default(VirtualDeviceType);
			}
			return result;
		}
		public ICollection<int> BindingIndexes {
			get { return bindings.Keys; }
		}
		public void AddBindings(int index, CapInputRelay profile) {
			if (profile == null) throw new ArgumentNullException("profile");
			bindings[index] = profile;
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