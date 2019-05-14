using System;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Util;

namespace Irseny.Iface.Main.View.Bindings {
	public class CapBindingsFactory : InterfaceFactory {
		int trackerIndex;
		Gdk.Color backgroundColor = new Gdk.Color(0xFF, 0xFF, 0xFF) { Pixel = 0xFF };
		/*string videoOutStock = "gtk-missing-image";
		Gtk.IconSize videoOutSize = Gtk.IconSize.Button;*/

		public CapBindingsFactory(int trackerIndex) : base() {
			this.trackerIndex = trackerIndex;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CapBindingsView");
			Container = factory.CreateWidget("box_Root");
			{
				var imgTopSource = Container.GetGadget<Gtk.Image>("img_AlignedTop");
				var imgTopTarget = Container.GetWidget<Gtk.Image>("img_Top");
				imgTopTarget.Pixbuf = ImageTools.Rotate(imgTopSource.Pixbuf, 0,
					ImageTools.RotatedImageSize.Source, ImageTools.RotatedImageAlpha.Source);

			}
			{
				var imgSideSource = Container.GetGadget<Gtk.Image>("img_AlignedSide");
				var imgSideTarget = Container.GetWidget<Gtk.Image>("img_Side");
				imgSideTarget.Pixbuf = ImageTools.Rotate(imgSideSource.Pixbuf, 0,
					ImageTools.RotatedImageSize.Source, ImageTools.RotatedImageAlpha.Source);
			}
			return true;
		}
		protected override bool ConnectInternal() {
			EquipmentMaster.Instance.HeadTracker.Updated += TrackerStateChanged;
			/*var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			videoOut.GetStock(out videoOutStock, out videoOutSize);*/
			return true;
		}

		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.HeadTracker.Updated -= TrackerStateChanged;
			StopCapture();
			return true;
		}

		protected override bool DestroyInternal() {
			// TODO: fix bug: removing a started tracker will make subsequently added trackers not receive images
			Container.Dispose();
			return true;
		}
		private void TrackerStateChanged(object sender, EquipmentUpdateArgs<int> args) {
			if (args.Index == trackerIndex) {
				bool start = args.Active;
				Invoke(delegate {
					if (start) {
						StartCapture();
					} else {
						StopCapture();
					}
				});
			}
		}
		private void StartCapture() {
			if (!Initialized) {
				return;
			}
			Tracap.DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId > -1) {
					var tracker = Tracap.DetectionSystem.Instance.GetDetector<Tracap.ISingleImageCapTracker>(trackerIndex, null);
					if (tracker != null) {
						tracker.PositionDetected += RetrievePosition;
					}
				}
			});
		}
		private void StopCapture() {
			Tracap.DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId > -1) {
					var tracker = Tracap.DetectionSystem.Instance.GetDetector<Tracap.ISingleImageCapTracker>(trackerIndex, null);
					if (tracker != null) {
						tracker.PositionDetected -= RetrievePosition;
					}
				}
			});
			if (!Initialized) {
				return;
			}
		}
		private void RetrievePosition(object sender, Tracap.PositionDetectedEventArgs args) {
			Tracap.CapPosition position = args.Position;
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				{
					var imgTopTarget = Container.GetWidget<Gtk.Image>("img_Top");
					var imgTopSource = Container.GetGadget<Gtk.Image>("img_AlignedTop");
					Gdk.Pixbuf rotated = ImageTools.Rotate(imgTopSource.Pixbuf, position.Yaw,
											 ImageTools.RotatedImageSize.Source, ImageTools.RotatedImageAlpha.Source,
											 backgroundColor, imgTopTarget.Pixbuf);

					if (imgTopTarget.Pixbuf != rotated) {
						imgTopTarget.Pixbuf.Dispose();
					}
					imgTopTarget.Pixbuf = rotated;
					imgTopTarget.QueueDraw();
				}
				{
					var imgSideTarget = Container.GetWidget<Gtk.Image>("img_Side");
					var imgSideSource = Container.GetGadget<Gtk.Image>("img_AlignedSide");
					Gdk.Pixbuf rotated = ImageTools.Rotate(imgSideSource.Pixbuf, position.Pitch,
											 ImageTools.RotatedImageSize.Source, ImageTools.RotatedImageAlpha.Source,
											 backgroundColor, imgSideTarget.Pixbuf);
					if (imgSideTarget.Pixbuf != rotated) {
						imgSideTarget.Pixbuf.Dispose();
					}
					imgSideTarget.Pixbuf = rotated;
					imgSideTarget.QueueDraw();
				}
				{
					string sYaw = string.Format("{0:N2}", position.Yaw);
					var txtYaw1 = Container.GetWidget<Gtk.Label>("txt_Yaw");
					var txtYaw2 = Container.GetWidget<Gtk.Label>("txt_YawYaw");
					txtYaw1.Text = sYaw;
					txtYaw2.Text = sYaw;
				}
				{
					var txtPitch = Container.GetWidget<Gtk.Label>("txt_Pitch");
					txtPitch.Text = string.Format("{0:N2}", position.Pitch);
				}
				{
					var txtRoll = Container.GetWidget<Gtk.Label>("txt_Roll");
					txtRoll.Text = string.Format("{0:N2}", position.Roll);

				}
				{
					var txtPosX = Container.GetWidget<Gtk.Label>("txt_PosX");
					txtPosX.Text = string.Format("{0:N2}", position.PosX);
				}
				{
					var txtPosY = Container.GetWidget<Gtk.Label>("txt_PosY");
					txtPosY.Text = string.Format("{0:N2}", position.PosY);
				}
				{
					string sPosZ = string.Format("{0:N2}", position.PosZ);
					var txtPosZ = Container.GetWidget<Gtk.Label>("txt_PosZ");
					var txtPosZZ = Container.GetWidget<Gtk.Label>("txt_PosZZ");
					txtPosZ.Text = sPosZ;
					txtPosZZ.Text = sPosZ;
				}
				/*{
					var imgSideSource = Container.GetGadget<Gtk.Image>("img_AlignedSide");
					Gdk.Pixbuf nRotatedImage = ImageTools.Rotate(
						imgSideSource.Pixbuf, angle, ImageTools.RotatedImageSize.Maximized,
						ImageTools.RotatedImageAlpha.Enabled, new Gdk.Color(), rotatedImage);
				}*/
			});
		}
	}
}
