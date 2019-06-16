﻿using System;
using System.IO;
using Irseny.Content;
using Irseny.Log;
using Irseny.Listing;
using Irseny.Capture.Video;

namespace Irseny.Iface.Main.Config.Camera {
	public class WebcamFactory : InterfaceFactory {
		readonly int streamIndex;

		public WebcamFactory(int index) : base() {
			this.streamIndex = index;
		}

		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("WebcamConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var btnCapture = Container.GetWidget<Gtk.ToggleButton>("btn_Start");
			btnCapture.Clicked += delegate {
				if (btnCapture.Active) {
					StartCapture();
				} else {
					StopCapture();
				}
			};
			// TODO: connect value setting widgets with value visualizers
			// create the stream to use
			CaptureSystem.Instance.Invoke(delegate {
				int streamId = CaptureSystem.Instance.CreateStream();
				if (streamId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to create capture {0}", streamIndex));
					return;
				}
				EquipmentMaster.Instance.VideoCaptureStream.Update(streamIndex, Listing.EquipmentState.Active, streamId);
			});

			return true;
		}
		protected override bool DisconnectInternal() {
			// stop and destroy stream
			CaptureSystem.Instance.Invoke(delegate {
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to destroy capture {0}", streamIndex));
					return;
				}
				EquipmentMaster.Instance.VideoCaptureStream.Update(streamIndex, EquipmentState.Missing, -1);
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to stop capture {0}", streamIndex));
				} else if (!stream.Stop()) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to stop capture {0}", streamIndex));
				}
				if (!CaptureSystem.Instance.DestroyStream(streamId)) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to destroy capture {0}", streamIndex));
					return;
				}
			});
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void StartCapture() {
			var settings = new CaptureSettings();
			// TODO: start existing stream
			CaptureSystem.Instance.Invoke(delegate {
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to start capture {0}", streamIndex));
					return;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to start capture {0}", streamIndex));
					return;
				}
				if (!stream.Start(settings)) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to start capture {0}", streamIndex));
					return;
				}
				LogManager.Instance.Log(LogMessage.CreateMessage(this, "Started capture {0}", streamIndex));
			});
		}
		private void StopCapture() {
			CaptureSystem.Instance.Invoke(delegate {
				// keep in mind that the capture could be missing here 
				// this is currently prohibited by implicitly enforcing an order: all updates are performed on the capture thread
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to stop capture {0}", streamIndex));
					return;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to stop capture {0}", streamIndex));
					return;
				}
				if (!stream.Stop()) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to stop capture {0}", streamIndex));
					return;
				}
				LogManager.Instance.Log(LogMessage.CreateMessage(this, "Stopped capture {0}", streamIndex));
			});
		}
	}
}