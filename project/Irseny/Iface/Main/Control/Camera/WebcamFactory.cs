using System;
using System.IO;
using Irseny.Content;
using Irseny.Log;
using Irseny.Capture.Video;

namespace Irseny.Iface.Main.Control.Camera {
	public class WebcamFactory : InterfaceFactory {
		private readonly int factoryIndex;

		public WebcamFactory(int index) : base() {
			this.factoryIndex = index;
		}

		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("WebcamControl");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			CaptureSystem.Instance.Invoke(delegate {
				Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(factoryIndex, Listing.EquipmentState.Passive, -1);
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
			CaptureSystem.Instance.Invoke(delegate {
				Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(factoryIndex, Listing.EquipmentState.Missing, -1);
			});
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void StartCapture() {
			CaptureSystem.Instance.Invoke(delegate {
				bool captureActive = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetState(factoryIndex) == Listing.EquipmentState.Active;
				if (captureActive) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Unable to start capture {0}: Already running", factoryIndex));
					return;
				}
				int streamId = CaptureSystem.Instance.CreateStream();
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream.Start(new CaptureSettings())) {
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(factoryIndex, Listing.EquipmentState.Active, streamId);
					LogManager.Instance.Log(LogMessage.CreateMessage(this, "Capture {0} started", factoryIndex));
					// TODO: apply stream settings to this instance
					CaptureSettings settings = stream.Settings;
				} else {
					LogManager.Instance.Log(LogMessage.CreateMessage(this, "Failed to start capture {0}", factoryIndex));
				}
			});
		}
		private void StopCapture() {
			CaptureSystem.Instance.Invoke(delegate {
				// keep in mind that the capture could be missing here 
				// this is currently prohibited by implicitly enforcing an order: all updates are performed on the capture thread
				int streamId = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(factoryIndex, -1);
				if (streamId > -1) {
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(factoryIndex, Listing.EquipmentState.Passive, -1); // switched between missing and passive in base factory
					if (!CaptureSystem.Instance.DestroyStream(streamId)) {
						LogManager.Instance.Log(LogMessage.CreateError(this, "Capture {0}: Destruction failed", factoryIndex));
					}
					LogManager.Instance.Log(LogMessage.CreateMessage(this, "Capture {0} stopped", factoryIndex));
				} else {
					LogManager.Instance.Log(LogMessage.CreateMessage(this, "Failed to stop capture {0}: Not running", factoryIndex));
				}
			});
		}
	}
}
