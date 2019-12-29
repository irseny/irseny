using System;
using System.IO;
using Irseny.Content;
using Irseny.Log;
using Irseny.Util;
using Irseny.Listing;
using Irseny.Capture.Video;


namespace Irseny.Iface.Main.Config.Camera {
	public class WebcamFactory : InterfaceFactory {
		readonly int streamIndex;

		public WebcamFactory(int index) : base() {
			this.streamIndex = index;
		}
		public int StreamIndex {
			get { return streamIndex; }
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
			var btnApply = Container.GetWidget<Gtk.Button>("btn_Apply");
			btnApply.Clicked += delegate {
				ApplySettings();
			};
			// TODO: connect value setting widgets with value visualizers
			// create the stream to use
			CaptureSystem.Instance.Invoke(delegate {
				int streamId = CaptureSystem.Instance.CreateStream();
				if (streamId < 0) {
					LogManager.Instance.Log(LogEntry.CreateError(this, "Failed to create capture " + streamIndex));
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
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to destroy capture " + streamIndex));
					return;
				}
				EquipmentMaster.Instance.VideoCaptureStream.Update(streamIndex, EquipmentState.Missing, -1);
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to stop capture " + streamIndex));
				} else if (!stream.Stop()) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to stop capture " + streamIndex));
				}
				if (!CaptureSystem.Instance.DestroyStream(streamId)) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to destroy capture " + streamIndex));
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
			if (!Initialized) {
				return;
			}
			CaptureSettings settings = GetSettings();
			// TODO: start existing stream
			CaptureSystem.Instance.Invoke(delegate {
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to start capture " + streamIndex));
					return;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to start capture " + streamIndex));
					return;
				}
				if (!stream.Start(settings)) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to start capture " + streamIndex));
					return;
				}

				LogManager.Instance.Log(LogEntry.CreateMessage(this, "Started capture " + streamIndex));
				// the stream may alternate the settings used internally
				// communicate these changes back to the user
				settings = stream.GetSettings();
				Invoke(delegate {
					ApplySettings(settings);
				});

			});
		}
		private void StopCapture() {
			CaptureSystem.Instance.Invoke(delegate {
				// keep in mind that the capture could be missing here
				// this is currently prohibited by implicitly enforcing an order: all updates are performed on the capture thread
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to stop capture " + streamIndex));
					return;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to stop capture " + streamIndex));
					return;
				}
				if (!stream.Stop()) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to stop capture " + streamIndex));
					return;
				}
				LogManager.Instance.Log(LogEntry.CreateMessage(this, "Stopped capture " + streamIndex));
			});
		}
		public CaptureSettings GetSettings() {
			var result = new CaptureSettings();
			if (!Initialized) {
				return result;
			}
			// the settings start with everything on auto
			// so we set the manual entries
			// this way they are set non auto in the returned object
			var cbxCamera = Container.GetWidget<Gtk.CheckButton>("cbx_AutoDeviceId");
			if (!cbxCamera.Active) {
				var txtCamera = Container.GetWidget<Gtk.SpinButton>("txt_DeviceId");
				result.SetInteger(CaptureProperty.CameraId, (int)txtCamera.Adjustment.Value);
			}
			var cbxWidth = Container.GetWidget<Gtk.CheckButton>("cbx_AutoFrameWidth");
			if (!cbxWidth.Active) {
				var txtWidth = Container.GetWidget<Gtk.SpinButton>("txt_FrameWidth");
				result.SetInteger(CaptureProperty.FrameWidth, (int)txtWidth.Adjustment.Value);
			}
			var cbxHeight = Container.GetWidget<Gtk.CheckButton>("cbx_AutoFrameHeight");
			if (!cbxHeight.Active) {
				var txtHeight = Container.GetWidget<Gtk.SpinButton>("txt_FrameHeight");
				int height = TextParseTools.ParseInt(txtHeight.Text, 480);
				result.SetInteger(CaptureProperty.FrameHeight, (int)txtHeight.Adjustment.Value);
			}
			var cbxRate = Container.GetWidget<Gtk.CheckButton>("cbx_AutoFrameRate");
			if (!cbxRate.Active) {
				var txtRate = Container.GetWidget<Gtk.SpinButton>("txt_FrameRate");
				int rate = TextParseTools.ParseInt(txtRate.Text, 30);
				result.SetInteger(CaptureProperty.FrameRate, (int)txtRate.Adjustment.Value);
			}
			var cbxExposure = Container.GetWidget<Gtk.CheckButton>("cbx_AutoExposure");
			if (!cbxExposure.Active) {
				var txtExposure = Container.GetWidget<Gtk.SpinButton>("txt_Exposure");
				result.SetDecimal(CaptureProperty.Exposure, txtExposure.Adjustment.Value);
			}
			var cbxBrightness = Container.GetWidget<Gtk.CheckButton>("cbx_AutoBrightness");
			if (!cbxBrightness.Active) {
				var txtBrightness = Container.GetWidget<Gtk.SpinButton>("txt_Brightness");
				result.SetDecimal(CaptureProperty.Brightness, txtBrightness.Adjustment.Value);
			}
			//var cbxContrast = Container.GetWidget<Gtk.CheckButton>("cbx_AutoContrast");
			//if (!cbxContrast.Active) {
			//	var txtContrast = Container.GetWidget<Gtk.SpinButton>("txt_Contrast");
			//	result.SetDecimal(CaptureProperty.Contrast, txtContrast.Adjustment.Value);
			//}
			//var cbxGain = Container.GetWidget<Gtk.CheckButton>("cbx_AutoGain");
			//if (!cbxGain.Active) {
			//	var txtGain = Container.GetWidget<Gtk.SpinButton>("txt_Gain");
			//	result.SetDecimal(CaptureProperty.Gain, txtGain.Adjustment.Value);
			//}
			return result;
		}
		public bool ApplySettings(CaptureSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			if (!Initialized) {
				return false;
			}
			var cbxCamera = Container.GetWidget<Gtk.CheckButton>("cbx_AutoDeviceId");
			//cbxCamera.Active = settings.IsAuto(CaptureProperty.CameraId);
			var txtCamera = Container.GetWidget<Gtk.SpinButton>("txt_DeviceId");
			txtCamera.Adjustment.Value = settings.GetInteger(CaptureProperty.CameraId, 0);
			var cbxWidth = Container.GetWidget<Gtk.CheckButton>("cbx_AutoFrameWidth");
			//cbxWidth.Active = settings.IsAuto(CaptureProperty.FrameWidth);
			var txtWidth = Container.GetWidget<Gtk.SpinButton>("txt_FrameWidth");
			txtWidth.Adjustment.Value = settings.GetInteger(CaptureProperty.FrameWidth, 640);
			var cbxHeight = Container.GetWidget<Gtk.CheckButton>("cbx_AutoFrameHeight");
			var txtHeight = Container.GetWidget<Gtk.SpinButton>("txt_FrameHeight");
			txtHeight.Adjustment.Value = settings.GetInteger(CaptureProperty.FrameHeight, 320);
			var cbxRate = Container.GetWidget<Gtk.CheckButton>("cbx_AutoFrameRate");
			var txtRate = Container.GetWidget<Gtk.SpinButton>("txt_FrameRate");
			txtRate.Adjustment.Value = settings.GetInteger(CaptureProperty.FrameRate, 30);
			var cbxExposure = Container.GetWidget<Gtk.CheckButton>("cbx_AutoExposure");
			var txtExposure = Container.GetWidget<Gtk.SpinButton>("txt_Exposure");
			txtExposure.Adjustment.Value = settings.GetDecimal(CaptureProperty.Exposure, 0.0);
			var cbxBrightness = Container.GetWidget<Gtk.CheckButton>("cbx_AutoBrightness");
			var txtBrightness = Container.GetWidget<Gtk.SpinButton>("txt_Brightness");
			txtBrightness.Adjustment.Value = settings.GetDecimal(CaptureProperty.Brightness, 0.0);
			//var cbxContrast = Container.GetWidget<Gtk.CheckButton>("cbx_AutoContrast");
			//var txtContrast = Container.GetWidget<Gtk.SpinButton>("txt_Contrast");
			//txtContrast.Adjustment.Value = settings.GetDecimal(CaptureProperty.Contrast, 0.0);
			//var cbxGain = Container.GetWidget<Gtk.CheckButton>("cbx_AutoGain");
			//var txtGain = Container.GetWidget<Gtk.SpinButton>("txt_Gain");
			//txtGain.Adjustment.Value = settings.GetDecimal(CaptureProperty.Gain, 0.0);
			// TODO: apply auto settings where applicable
			return true;
		}
		public void ApplySettings() {
			if (!Initialized) {
				return;
			}
			CaptureSettings settings = GetSettings();
			CaptureSystem.Instance.Invoke(delegate {
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.LogError(this, "Video capture " + streamIndex + " is missing");
					return;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.LogError(this, "Video capture" + streamIndex + " is missing");
					return;
				}
				stream.ApplySettings(settings);
			});
		}
	}
}
