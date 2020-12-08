using System;
using System.Threading;
using System.Threading.Tasks;
using Irseny.Core.Sensors;
using Irseny.Core.Sensors.VideoCapture;
using Irseny.Core.Tracking;
using Irseny.Core.Log;
using Irseny.Core.Inco.Device;
using Irseny.Main.Content.Profile;


namespace Irseny.Main {
	public class ProfileActivator {
		public ProfileActivator() {
		}
		public Task<bool> ActivateProfile(SetupProfile profile) {
			if (profile == null) throw new ArgumentNullException("profile");
			var captureSignal = new ManualResetEvent(false);
			var trackerSignal = new ManualResetEvent(false);
			var deviceSignal = new ManualResetEvent(false);
			var cancel = new CancellationToken();

			CaptureSystem.Instance.Invoke(delegate {
				foreach (int i in profile.VideoCaptureIndexes) {
					var settings = profile.GetVideoCapture(i);
					int index = CaptureSystem.Instance.CreateStream();
					var capture = CaptureSystem.Instance.GetStream(index);
					capture.ApplySettings(settings);

				}
				captureSignal.Set();
			});

			DetectionSystem.Instance.Invoke(delegate {
				foreach (int i in profile.TrackerIndexes) {
					var settings = profile.GetTracker(i);
					var tracker = new Cap3PointTracker();
					int index = DetectionSystem.Instance.StartTracker(tracker, settings);
				}
				trackerSignal.Set();
			});

			VirtualDeviceManager.Instance.Invoke(delegate {
				foreach (int i in profile.DeviceIndexes) {
					var settings = profile.GetDevice(i);
					var device = VirtualDevice.CreateFromSettings(settings);
					int index = VirtualDeviceManager.Instance.ConnectDevice(device);
				}
				deviceSignal.Set();
			});
			return Task.Factory.StartNew(delegate {
				captureSignal.WaitOne();
				trackerSignal.WaitOne();
				deviceSignal.WaitOne();
				return true;
			}, cancel);

			// we need to wait for the detection system to update tracker equipment
//			DetectionSystem.Instance.Invoke(delegate {
//				Invoke(delegate {
//					foreach (int iTracker in profile.BindingIndexes) {
//						if (!bindingsFactory.ApplyBindings(iTracker, profile.GetBindings(iTracker))) {
//							LogManager.Instance.LogError(this, "Failed to restore tracker bindings");
//							return;
//						}
//					}
//				});
//			});
		}
	}
}

