using System;
using System.IO;
using Irseny.Content;

namespace Irseny.Viol.Main.Control.Camera {
	public class CameraFactory : InterfaceFactory {
		private readonly int index;

		public CameraFactory(int index) : base() {
			this.index = index;
		}

		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CameraControl");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(index, Listing.EquipmentState.Passive, -1);
			});

			var btnCapture = Container.GetWidget<Gtk.ToggleButton>("btn_Start");
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
				} else {
					StopCapture();
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
			StopCapture();
			// update as missing after the capture has been stopped
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(index, Listing.EquipmentState.Missing, -1);
			});
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
				if (stream.Start(new Capture.Video.CaptureSettings())) {
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(index, Listing.EquipmentState.Active, streamId);
					Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Capture {0} started", index));
					// TODO: apply stream settings to this instance
					Capture.Video.CaptureSettings settings = stream.Settings;
				} else {
					Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Failed to start capture {0}", index));
				}
			});
		}
		private void StopCapture() {
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				// keep in mind that the capture could be missing here 
				// this is currently prohibited by implicitly enforcing an order: all updates are performed on the capture thread
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
