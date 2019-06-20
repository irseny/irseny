using System;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Util;
using Irseny.Tracap;
using Irseny.Log;

namespace Irseny.Iface.Main.View.Bindings {
	public class CapBindingsFactory : InterfaceFactory {
		int trackerIndex;
		bool lockBinding = false;
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
			//EquipmentMaster.Instance.HeadTracker.Updated += TrackerStateChanged;
			/*var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			videoOut.GetStock(out videoOutStock, out videoOutSize);*/
			{
				Gtk.ToggleButton[] buttons = GetBindingButtons();
				for (int i = 0; i + 1 < buttons.Length; i += 2) {
					if (buttons[i] != null) {
						buttons[i].Clicked += CreateToggleAxisCallback(buttons[i + 1], buttons);
					}
					if (buttons[i + 1] != null) {
						buttons[i + 1].Clicked += CreateToggleAxisCallback(buttons[i], buttons);
					}
				}
				CapAxis[] axes = new CapAxis[] {
					CapAxis.X, CapAxis.X,
					CapAxis.Y, CapAxis.Y,
					CapAxis.Z, CapAxis.Z,
					CapAxis.Yaw, CapAxis.Yaw,
					CapAxis.Pitch, CapAxis.Pitch,
					CapAxis.Roll, CapAxis.Roll
					};

				for (int i = 0; i < buttons.Length; i++) {
					if (buttons[i] != null) {
						buttons[i].Toggled += CreateOpenTabCallback(axes[i]);
					}
				}
			}

			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					LogManager.Instance.LogError(this, "Tracker " + 0 + " not available");
					return;
				}
				var tracker = DetectionSystem.Instance.GetDetector<ISingleImageCapTracker>(trackerIndex, null);
				if (tracker == null) {
					LogManager.Instance.LogError(this, "Tracker " + 0 + " not available");
					return;
				}
				tracker.PositionDetected += RetrievePosition;
			});
			return true;
		}

		protected override bool DisconnectInternal() {
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					return;
				}
				var tracker = DetectionSystem.Instance.GetDetector<ISingleImageCapTracker>(trackerIndex, null);
				if (tracker == null) {
					return;
				}
				tracker.PositionDetected -= RetrievePosition;
			});
			//EquipmentMaster.Instance.HeadTracker.Updated -= TrackerStateChanged;

			return true;
		}

		protected override bool DestroyInternal() {
			// TODO: fix bug: removing a started tracker will make subsequently added trackers not receive images
			Container.Dispose();
			return true;
		}
		private Gtk.ToggleButton[] GetBindingButtons() {
			Gtk.ToggleButton[] buttons = new Gtk.ToggleButton[(3 + 3)*2];
			string[] names = new string[] {
					"btn_AxisX1",
					"btn_AxisX2",
					"btn_AxisY1",
					"btn_AxisY2",
					"btn_AxisZ1",
					"btn_AxisZ2",
					"btn_Yaw1",
					"btn_Yaw2",
					"btn_Pitch1",
					"btn_Pitch2",
					"btn_Roll1",
					"btn_Roll2",
				};
			for (int i = 0; i < buttons.Length; i++) {
				buttons[i] = Container.GetWidget<Gtk.ToggleButton>(names[i], null);
			}

			return buttons;
		}
		private EventHandler CreateToggleAxisCallback(Gtk.ToggleButton alternative, Gtk.ToggleButton[] all) {
			return (object sender, EventArgs e) => {
				if (lockBinding) {
					return; // do not react to modified buttons
				}
				lockBinding = true;
				var master = (Gtk.ToggleButton)sender;
				bool target = master.Active;
				// make the alternative button have the same state as the master
				if (alternative != null) {
					if (alternative.Active != target) {
						alternative.Click();
					}
				}
				// disable all other buttons
				if (target) {
					foreach (Gtk.ToggleButton button in all) {
						if (button != master && button != alternative && button != null) {
							if (button.Active) {
								button.Click();
							}
						}
					}
				}
				lockBinding = false;
			};
		}
		private EventHandler CreateOpenTabCallback(CapAxis axis) {
			return (object sender, EventArgs e) => {
				if (lockBinding) {
					return; // do not open multiple times
				}
				var floor = GetFloor<BindingTabFactory>("Binding");
				Gtk.ToggleButton button = (Gtk.ToggleButton)sender;
				if (button.Active) {
					floor.RestoreBinding(axis);
				} else {
					floor.Hide();
				}
			};
		}

		private void RetrievePosition(object sender, PositionDetectedEventArgs args) {
			CapPosition position = args.Position;
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
					if (imgSideTarget.Pixbuf != rotated) { // never happens
						imgSideTarget.Pixbuf.Dispose();
					}
					imgSideTarget.Pixbuf = rotated; // TODO: try to avoid assignment
					imgSideTarget.QueueDraw();
				}
				{
					string sYaw = string.Format("{0:N2}", position.Yaw);
					var txtYaw1 = Container.GetWidget<Gtk.Label>("txt_Yaw1");
					var txtYaw2 = Container.GetWidget<Gtk.Label>("txt_Yaw2");
					txtYaw1.Text = sYaw;
					txtYaw2.Text = sYaw;
				}
				{
					var txtPitch = Container.GetWidget<Gtk.Label>("txt_Pitch1");
					txtPitch.Text = string.Format("{0:N2}", position.Pitch);
				}
				{
					var txtRoll = Container.GetWidget<Gtk.Label>("txt_Roll1");
					txtRoll.Text = string.Format("{0:N2}", position.Roll);

				}
				{
					var txtPosX = Container.GetWidget<Gtk.Label>("txt_PosX1");
					txtPosX.Text = string.Format("{0:N2}", position.PosX);
				}
				{
					var txtPosY = Container.GetWidget<Gtk.Label>("txt_PosY1");
					txtPosY.Text = string.Format("{0:N2}", position.PosY);
				}
				{
					string sPosZ = string.Format("{0:N2}", position.PosZ);
					var txtPosZ = Container.GetWidget<Gtk.Label>("txt_PosZ1");
					var txtPosZZ = Container.GetWidget<Gtk.Label>("txt_PosZ2");
					txtPosZ.Text = sPosZ;
					txtPosZZ.Text = sPosZ;
				}
			});
		}
	}
}
