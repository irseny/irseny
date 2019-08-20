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
		int activeSource = -1;
		HashSet<string> selectionSources = new HashSet<string>();
		bool lockSelection = false;

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
			btnApply.Clicked += SettingsApplied;
			var btnCenter = Container.GetWidget<Gtk.Button>("btn_Center");
			btnCenter.Clicked += TrackerCentered;
			var cbbSource = Container.GetWidget<Gtk.ComboBoxText>("cbb_Source");
			cbbSource.Changed += ActiveSourceUpdated;
			ConnectModel();
			ConnectTracker();
			EquipmentMaster.Instance.VideoCaptureStream.Updated += AvailableSourceUpdated;
			Invoke(delegate {
				EquipmentMaster.Instance.VideoCaptureStream.SendEquipment(AvailableSourceUpdated);
			});
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.VideoCaptureStream.Updated -= AvailableSourceUpdated;
			DisconnectTracker();
			DisconnectModel();
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void UpdateSettings() {
			if (!Initialized) {
				return;
			}
			{ // source
				// connection does not initialize if nothing is selected
				int activeSource = GetActiveSource();
				settings.SetInteger(TrackerProperty.Stream0, activeSource);
			}
			{ // brightness
				var txtBrightness = Container.GetWidget<Gtk.SpinButton>("txt_Brightness");
				int brightness = (int)txtBrightness.Adjustment.Value;
				settings.SetInteger(TrackerProperty.MinBrightness, brightness);
			}
			{ // smoothing
				var txtSmooth = Container.GetWidget<Gtk.SpinButton>("txt_Smooth");
				int smoothing = (int)txtSmooth.Adjustment.Value;
				settings.SetInteger(TrackerProperty.Smoothing, smoothing);
			}
			{ // radius
				var txtRadius = Container.GetWidget<Gtk.SpinButton>("txt_MinRadius");
				int minRadius = (int)txtRadius.Adjustment.Value;
				settings.SetInteger(TrackerProperty.MinClusterRadius, minRadius);
				txtRadius = Container.GetWidget<Gtk.SpinButton>("txt_MaxRadius");
				int maxRadius = (int)txtRadius.Adjustment.Value;
				settings.SetInteger(TrackerProperty.MaxClusterRadius, maxRadius);
			}
			// TODO: read from UI
		}
		private IObjectModel GetModel() {
			var result = new CapModel();
			if (!Initialized) {
				return result;
			}
			var txtWidth = Container.GetWidget<Gtk.SpinButton>("txt_VisorWidth");
			result.VisorWidth = (int)txtWidth.Adjustment.Value;
			var txtHeight = Container.GetWidget<Gtk.SpinButton>("txt_VisorHeight");
			result.VisorHeight = (int)txtHeight.Adjustment.Value;
			var txtLength = Container.GetWidget<Gtk.SpinButton>("txt_VisorLength");
			result.VisorLength = (int)txtLength.Adjustment.Value;
			return result;
		}
		private void ConnectTracker() {
			var tracker = new Cap3PointTracker();
			var settings = GetSettings();
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = DetectionSystem.Instance.StartTracker(tracker, settings);
				if (trackerId < 0) {
					LogManager.Instance.LogError(this, "Failed to create tracker " + trackerIndex);
					return;
				}
				EquipmentMaster.Instance.HeadTracker.Update(trackerIndex, Listing.EquipmentState.Active, trackerId);
				int streamId = settings.GetInteger(TrackerProperty.Stream0, 0);
				connection.StartConnection(tracker, streamId);
				LogManager.Instance.LogSignal(this, "Started Tracker " + trackerIndex);
			});
		}
		private void ConnectModel() {
			var model = GetModel();
			DetectionSystem.Instance.Invoke(delegate {
				int modelId = DetectionSystem.Instance.RegisterModel(model);
				if (modelId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to create model " + trackerIndex));
					return;
				}
				EquipmentMaster.Instance.HeadModel.Update(trackerIndex, EquipmentState.Active, modelId);
				LogManager.Instance.LogSignal(this, "Created model " + trackerIndex);
			});
		}
		private void DisconnectTracker() {
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					LogManager.Instance.LogWarning(this, "Tracker " + trackerIndex + " not found");
					return;
				}
				EquipmentMaster.Instance.HeadTracker.Update(trackerIndex, EquipmentState.Missing, -1);

				IPoseTracker tracker = DetectionSystem.Instance.StopTracker(trackerId);
				if (tracker == null) {
					LogManager.Instance.LogWarning(this, "Failed to stop tracker " + trackerIndex);
				}
				tracker.Dispose();
				connection.StopConnection();
				LogManager.Instance.LogSignal(this, "Stopped Tracker " + trackerIndex);
			});
		}
		private void DisconnectModel() {
			DetectionSystem.Instance.Invoke(delegate {
				int modelId = EquipmentMaster.Instance.HeadModel.GetEquipment(trackerIndex, -1);
				if (trackerIndex < 0) {
					LogManager.Instance.LogWarning(this, "Model " + trackerIndex + " not found");
					return;
				}
				EquipmentMaster.Instance.HeadModel.Update(trackerIndex, EquipmentState.Missing, -1);
				if (DetectionSystem.Instance.RemoveModel(modelId)) {
					LogManager.Instance.LogWarning(this, "Failed to destroy model " + trackerIndex);
					return;
				}
			});
		}
		private void SettingsApplied(object sender, EventArgs args) {
			var model = GetModel();
			var settings = GetSettings();
			DetectionSystem.Instance.Invoke(delegate {
				int modelId = EquipmentMaster.Instance.HeadModel.GetEquipment(trackerIndex, -1);

				if (modelId < 0) {
					LogManager.Instance.LogError(this, "Model " + trackerIndex + " not found");
					return;
				}
				if (!DetectionSystem.Instance.ReplaceModel(modelId, model)) {
					LogManager.Instance.LogError(this, "Model " + trackerIndex + " could not be updated");
					return;
				}
				LogManager.Instance.LogSignal(this, "Model " + trackerIndex + " updated");
			});
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
				if (!tracker.ApplySettings(settings)) {
					LogManager.Instance.LogError(this, "Failed to apply settings for tracker " + trackerIndex);
					return;
				}
				LogManager.Instance.LogSignal(this, "Applied settings for tracker " + trackerIndex);
			});
		}
		private void TrackerCentered(object sender, EventArgs args) {
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
				if (!tracker.Center()) {
					LogManager.Instance.LogError(this, "Failed to center tracker " + trackerIndex);
					return;
				}
			});
		}
		private void ActiveSourceUpdated(object sender, EventArgs args) {
			if (!lockSelection) {
				activeSource = GetActiveSource();
			}
		}
		private void AvailableSourceUpdated(object sender, EquipmentUpdateArgs<int> args) {
			int sourceIndex = args.Index;
			bool sourceActive = args.Active;
			Invoke(delegate {
				if (sourceActive) {
					selectionSources.Add("Camera" + sourceIndex);
				} else {
					selectionSources.Remove("Camera" + sourceIndex);
				}
				BuildSourceSelection();
			});
		}
		private void BuildSourceSelection() {
			if (!Initialized) {
				return;
			}
			lockSelection = true;
			var cbbSource = Container.GetWidget<Gtk.ComboBoxText>("cbb_Source");
			string activeSource = cbbSource.ActiveText;
			var store = (Gtk.ListStore)cbbSource.Model;
			store.Clear();
			store.AppendValues("None");
			int iActiveEntry = -1;
			int iEntry = 1;
			foreach (string sourceName in selectionSources) {
				store.AppendValues(sourceName);
				if (sourceName.Equals(activeSource)) {
					iActiveEntry = iEntry;
				}
				iEntry += 1;
			}
			cbbSource.Active = iActiveEntry;
			if (iActiveEntry < 1) {
				ClearSourceSelection();
			}
			lockSelection = false;
		}
		private void ClearSourceSelection() {
			if (!Initialized) {
				return;
			}
			var cbbSource = Container.GetWidget<Gtk.ComboBoxText>("cbb_Source");
			cbbSource.Active = 0;
		}
		private int GetActiveSource() {
			if (!Initialized) {
				return -1;
			}
			var cbbSource = Container.GetWidget<Gtk.ComboBoxText>("cbb_Source");
			return cbbSource.Active - 1;
		}
		private bool SetActiveSource(int sourceIndex) {
			if (!Initialized) {
				return false;
			}
			var cbbSource = Container.GetWidget<Gtk.ComboBoxText>("cbb_Source");
			cbbSource.Active = sourceIndex + 1;
			return true;
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
