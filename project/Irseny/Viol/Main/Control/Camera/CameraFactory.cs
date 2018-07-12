using System;
using System.IO;

namespace Irseny.Viol.Main.Control.Camera {
	public class CameraFactory : InterfaceFactory {
		private readonly int index;

		public CameraFactory(int index) : base() {
			this.index = index;
		}

		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("CameraControl"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var btnCapture = Container.GetWidget<Gtk.ToggleButton>("btn_Capture");
			/*btnCapture.Toggled += delegate {
				if (btnCapture.Active) {
					StartCapture();
				} else {
					StopCapture();
				}
			};*/
			btnCapture.Clicked += delegate {
				if (btnCapture.Active) {
					StartCapture();
					Console.WriteLine("start");
				} else {
					StopCapture();
					Console.WriteLine("stop");
				}
				/*Console.WriteLine("capture button clicked");
				var btn = Container.GetWidget<Gtk.ToggleButton>("btn_Capture");
				Console.WriteLine("button active: " + btn.Active);
				StartCapture();*/
			};
			// TODO: connect value setting widgets with value visualizers
			// update video sources

			return true;
		}
		protected override bool DisconnectInternal() {
			//var btnCapture = Container.GetWidget<Gtk.ToggleButton>("btn_Capture");
			StopCapture(); // TODO: use trystopcapture variant that does not log failures
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void StartCapture() {
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				bool captureActive = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetState(index) == Listing.EquipmentState.Active;
				if (captureActive) {
					Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateWarning(this, "Unable to start capture {0}: Already running", index));
					return;
				}
				int streamId = Capture.Video.CaptureSystem.Instance.CreateStream();
				Capture.Video.CaptureStream stream = Capture.Video.CaptureSystem.Instance.GetStream(streamId);
				stream.CaptureStarted += delegate {
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(index, Listing.EquipmentState.Active, streamId);
				};
				stream.CaptureStarted += delegate {
					Capture.Video.CaptureSettings settings = stream.Settings;
					Invoke(delegate {
						// TODO: apply settings to this instance
					});
				};
				if (stream.Start(new Capture.Video.CaptureSettings())) {
					Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Capture {0} started", index));
				} else {
					Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Failed to start capture {0}", index));
				}
			});
		}
		private void StopCapture() {
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				int streamId = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(index, -1);
				if (streamId > -1) {
					if (!Capture.Video.CaptureSystem.Instance.DestroyStream(streamId)) {
						Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateError(this, "Capture {0}: Destruction failed", index));
					}
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(index, Listing.EquipmentState.Passive, -1); // switched between missing and passive in base factory
					Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Capture {0} stopped", index));
				} else {
					Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Failed to stop capture {0}: Not running", index));
				}
			});
		}
	}
}
