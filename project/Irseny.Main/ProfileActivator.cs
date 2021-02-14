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
using System.Threading;
using System.Threading.Tasks;
using Irseny.Core.Sensors;
using Irseny.Core.Sensors.VideoCapture;
using Irseny.Core.Tracking;
using Irseny.Core.Tracking.HeadTracking;
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

			CaptureSystem.Instance.Invoke((object sender, EventArgs args) => {
				var system = (CaptureSystem)sender;
				foreach (int i in profile.VideoCaptureIndexes) {
					var settings = profile.GetVideoCapture(i);
					var sensor = new WebcamCapture(settings);
					int index = system.ConnectSensor(sensor, i);
					if (index != i) {
						LogManager.Instance.LogWarning(this, string.Format("VideoCapture {0} assigned to different index {1}",
							i, index));
					}
				}
				captureSignal.Set();
			});

			TrackingSystem.Instance.Invoke(delegate {
				foreach (int i in profile.TrackerIndexes) {
					var settings = profile.GetTracker(i);
					var tracker = new P3CapTracker(settings);
					int index = TrackingSystem.Instance.StartTracker(tracker);
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

