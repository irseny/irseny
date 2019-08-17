using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Tracking;
using Irseny.Capture.Video;
using Irseny.Log;
using Irseny.Util;

namespace Irseny.Iface.Main.Config.Tracking {
	public class CapTrackingFactory : InterfaceFactory {
		readonly int trackerIndex;
		VideoTrackerConnection connection = new VideoTrackerConnection();
		TrackerSettings settings;

		public CapTrackingFactory(int index, TrackerSettings settings) : base() {
			if (settings == null) throw new ArgumentNullException("settings");
			this.settings = settings;
			this.trackerIndex = index;
		}
		public int TrackerIndex {
			get { return trackerIndex; }
		}
		public TrackerSettings GetSettings() {
			UpdateSettings();
			return new TrackerSettings(settings);
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CapTrackingConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var btnApply = Container.GetWidget<Gtk.Button>("btn_Apply");
			btnApply.Clicked += delegate {
				var settings = GetSettings();
				DetectionSystem.Instance.Invoke(delegate {
					int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
					if (trackerId < 0) {
						LogManager.Instance.LogError(this, "Tracker " + trackerIndex + " not found");
						return;
					}
					IPoseTracker tracker = DetectionSystem.Instance.GetTracker(trackerId);
					if (tracker == null) {
						LogManager.Instance.LogError(this, "Tracker " + trackerIndex + " not found");
						return;
					}
					tracker.ApplySettings(settings);
				});
			};
			// start the tracker
			// delay so that settings can be queried
			DetectionSystem.Instance.Invoke(delegate {
				var tracker = new Cap3PointTracker();
				var settings = GetSettings();
				//DetectionSystem.Instance.
				int trackerId = DetectionSystem.Instance.StartTracker(tracker, settings);
				if (trackerId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to create tracker " + trackerIndex));
					return;
				}
				EquipmentMaster.Instance.HeadTracker.Update(trackerIndex, Listing.EquipmentState.Active, trackerId);
				int streamId = settings.GetInteger(TrackerProperty.Stream0, 0);
				connection.StartConnection(tracker, streamId);
				LogManager.Instance.LogSignal(this, "Started Tracker " + trackerIndex);
			});
			return true;
		}
		protected override bool DisconnectInternal() {
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to destroy tracker " + trackerIndex));
					return;
				}
				EquipmentMaster.Instance.HeadTracker.Update(trackerIndex, EquipmentState.Missing, -1);

				IPoseTracker tracker = DetectionSystem.Instance.StopTracker(trackerId);
				if (tracker == null) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to stop tracker " + trackerIndex));
				}
				tracker.Dispose();
				connection.StopConnection();
				LogManager.Instance.LogSignal(this, "Stopped Tracker " + trackerIndex);
			});
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void UpdateSettings() {
			settings.SetInteger(TrackerProperty.Stream0, 0);
			settings.SetInteger(TrackerProperty.MinBrightness, 16);
			// TODO: read from UI
		}

		private class VideoTrackerConnection {
			CaptureStream lockedSource = null;
			IPoseTracker sink = null;
			EventHandler<ImageCapturedEventArgs> tunnel = null;

			public VideoTrackerConnection() {

			}
			public void StartConnection(ISingleImageCapTracker tracker, int sourceIndex) {
				StopConnection();
				CaptureSystem.Instance.Invoke(delegate {
					int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(sourceIndex, -1);
					if (streamId < 0) {
						return;
					}
					CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
					if (stream == null) {
						return;
					}
					lockedSource = stream;
					sink = tracker;
					tunnel = GenerateTunnelCallback(tracker, stream);
					lockedSource.ImageAvailable += tunnel;
				});
			}
			public void StopConnection() {
				CaptureSystem.Instance.Invoke(delegate {
					if (lockedSource == null || tunnel == null) {
						return;
					}
					lockedSource.ImageAvailable -= tunnel;
					lockedSource = null;
					tunnel = null;
					sink = null;
				});
			}
			private EventHandler<ImageCapturedEventArgs> GenerateTunnelCallback(ISingleImageCapTracker tracker, CaptureStream stream) {
				return (object sender, ImageCapturedEventArgs args) => {
					using (SharedRef<Emgu.CV.Mat> image = args.GrayImage) {
						if (tracker.Running) {
							tracker.QueueInput(image);
						}
					}
				};
			}
		}
	}
	/*private class VideoTrackerConnection {
		readonly object sourceSync = new object();
		readonly object targetSync = new object();
		readonly object connectSync = new object();
		int[] sourceIndexes;
		int targetIndex;
		Capture.Video.CaptureStream[] variableSources;
		Capture.Video.CaptureStream[] loadedSources;
		Tracap.ICapTracker variableTarget;
		Tracap.ICapTracker loadedTarget;
		EventHandler<Listing.EquipmentUpdateArgs<int>> sourceListener = null;
		EventHandler<Listing.EquipmentUpdateArgs<int>> targetListener = null;
		EventHandler<Capture.Video.ImageCapturedEventArgs> transmitImage = null;

		public VideoTrackerConnection(int targetIndex, int[] sourceIndexes) {
			this.targetIndex = targetIndex;
			this.sourceIndexes = sourceIndexes;
			this.variableSources = new Capture.Video.CaptureStream[sourceIndexes.Length];
			this.loadedSources = new Capture.Video.CaptureStream[sourceIndexes.Length];
			this.variableTarget = null;
			this.loadedTarget = null;
		}
		private int SourceNo {
			get { return sourceIndexes.Length; }
		}
		private bool SourcesFound {
			get {
				for (int i = 0; i < SourceNo; i++) {
					if (variableSources[i] == null) {
						return false;
					}
				}
				return true;
			}
		}
		private bool SourcesChanged {
			get {
				for (int i = 0; i < SourceNo; i++) {
					if (variableSources[i] != loadedSources[i]) {
						return true;
					}
				}
				return false;
			}
		}
		private bool TargetFound {
			get { return variableTarget != null; }
		}
		private bool TargetChanged {
			get { return variableTarget != loadedTarget; }
		}
		public void BeginListening() {
			BeginTargetListening();
			BeginSourceListening();
		}
		private void UpdateConnection() {
			lock (connectSync) {
				Disconnect();
				Connect();
			}
		}
		private void Connect() {
			lock (targetSync) { // nested locks beginning with connect, target, source
				loadedTarget = variableTarget;
				lock (sourceSync) {
					if (TargetFound && SourcesFound) {
						transmitImage = (object sender, Capture.Video.ImageCapturedEventArgs args) => {
							using (Util.SharedRef<Emgu.CV.Mat> image = args.GrayImage) {
								//loadedTarget.(image);
								if (loadedTarget is Tracap.ISingleImageCapTracker) {
									((Tracap.ISingleImageCapTracker)loadedTarget).QueueInput(image);
								}
							}
						};
						for (int i = 0; i < SourceNo; i++) {
							loadedSources[i] = variableSources[i];
							loadedSources[i].ImageAvailable += transmitImage;
						}
					}
				}
			}
		}
		private void Disconnect() {
			lock (sourceSync) {
				for (int i = 0; i < SourceNo; i++) {
					if (loadedSources[i] != null) {
						loadedSources[i].ImageAvailable -= transmitImage;
						loadedSources[i] = null;
					}
				}
				transmitImage = null;
			}
			lock (targetSync) {
				loadedTarget = null; // target not modified
			}

		}
		private void BeginSourceListening() {
			// listener
			sourceListener = (object sender, Listing.EquipmentUpdateArgs<int> args) => {
				bool modified = false;
				for (int i = 0; i < SourceNo; i++) {
					if (args.Index == sourceIndexes[i]) {
						modified = true;
					}
				}
				if (modified) {
					Capture.Video.CaptureSystem.Instance.Invoke(delegate {
						if (UpdateSources()) {
							UpdateConnection();
						}
					});
				}
			};
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated += sourceListener;
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				if (UpdateSources()) {
					UpdateConnection();
				}
			});
		}
		private void BeginTargetListening() {
			// listener
			targetListener = (object sender, Listing.EquipmentUpdateArgs<int> args) => {
				if (args.Index == targetIndex) {
					Tracap.DetectionSystem.Instance.Invoke(delegate {
						if (UpdateTarget()) {
							UpdateConnection();
						}
					});
				}
			};
			Listing.EquipmentMaster.Instance.HeadTracker.Updated += targetListener;
			// query started
			Tracap.DetectionSystem.Instance.Invoke(delegate {
				if (UpdateTarget()) {
					UpdateConnection();
				}
			});
		}
		public void EndListening() {
			Disconnect();
			Listing.EquipmentMaster.Instance.HeadTracker.Updated -= targetListener;
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated -= sourceListener;
		}
		public bool UpdateTarget() {
			Tracap.ICapTracker target = null;
			// detect target availability
			int targetId = Listing.EquipmentMaster.Instance.HeadTracker.GetEquipment(targetIndex, -1);
			if (targetId > -1) {
				Tracap.IPoseTracker detector = Tracap.DetectionSystem.Instance.GetTracker(targetId);
				target = detector as Tracap.ICapTracker;
			}
			bool changed = false;
			lock (targetSync) {
				if (target != variableTarget) {
					variableTarget = target;
					changed = true;
				}
			}
			return changed;
		}
		public bool UpdateSources() {
			bool changed = false;
			for (int i = 0; i < SourceNo; i++) {
				// detect source availability
				int sourceId = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(sourceIndexes[i], -1);
				Capture.Video.CaptureStream stream = null;
				if (sourceId > -1) {
					stream = Capture.Video.CaptureSystem.Instance.GetStream(sourceId);
				}
				lock (sourceSync) {
					if (variableSources[i] != stream) {
						variableSources[i] = stream;
						changed = true;
					}
				}
			}

			return changed;
		}
	}
}*/

}
