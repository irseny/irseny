﻿using System;
using System.Collections.Generic;
using Irseny.Content;

namespace Irseny.Iface.Main.Config.Tracking {
	public class CapTrackingFactory : InterfaceFactory {
		readonly int index;
		VideoTrackerConnection connection = null;
		public CapTrackingFactory(int index) : base() {
			this.index = index;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CapTrackingConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			Listing.EquipmentMaster.Instance.HeadTracker.Update(index, Listing.EquipmentState.Passive, -1);
			var btnTrack = Container.GetWidget<Gtk.ToggleButton>("btn_Start");
			btnTrack.Clicked += delegate {
				if (btnTrack.Active) {
					StartTracking();
				} else {
					StopTracking();
				}
			};
			return true;
		}
		protected override bool DisconnectInternal() {
			StopTracking();
			// TODO: make sure that the tracker is stopped before unregistering it
			Listing.EquipmentMaster.Instance.HeadTracker.Update(index, Listing.EquipmentState.Missing, -1);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private bool StartTracking() {
			int trackerId = Listing.EquipmentMaster.Instance.HeadTracker.GetEquipment(index, -1);
			if (trackerId < 0) {
				// TODO: parse options
				var options = new Tracap.Basic3PointOptions();
				var tracker = new Tracap.Basic3PointCapTracker(options);
				trackerId = Tracap.DetectionSystem.Instance.StartDetector(tracker);
				Listing.EquipmentMaster.Instance.HeadTracker.Update(index, Listing.EquipmentState.Active, trackerId);
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Head tracker {0} started", index));
				// TODO: change to stream index from options
				int[] sourceIndexes = new int[] { 0 };
				connection = new VideoTrackerConnection(index, sourceIndexes);
				connection.BeginListening();
				return true;
			} else {
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateWarning(this, "Unable to start head tracker {0}: Already running", index));
				return false;
			}
		}
		// TODO: implement option application
		private bool StopTracking() {
			if (connection != null) {
				connection.EndListening();
				connection = null;
			}
			int trackerId = Listing.EquipmentMaster.Instance.HeadTracker.GetEquipment(index, -1);
			if (trackerId > -1) {
				Listing.EquipmentMaster.Instance.HeadTracker.Update(index, Listing.EquipmentState.Passive, -1);
				Tracap.DetectionSystem.Instance.StopDetector(trackerId); // auto disconnect from events in dispose
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Head tracker {0} stopped", index));
				return true;
			} else {
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateWarning(this, "Unable to stop head tracker {0}: Not running", index));
				return false;
			}

		}
		private class VideoTrackerConnection {
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
					Tracap.IPoseDetector detector = Tracap.DetectionSystem.Instance.GetDetector(targetId);
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
	}

}
