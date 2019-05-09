using System;
using System.Diagnostics;
using Irseny.Content;
using Irseny.Listing;

namespace Irseny.Iface.Main.View.Camera {
	public class CameraFactory : InterfaceFactory {
		public CameraFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CameraDisplay");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.PackStart(ntbMain, true, true, 0);
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated += CameraChanged;
			return true;
		}
		protected override bool DisconnectInternal() {
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated -= CameraChanged;
			while (RemoveCamera()) {
			}
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.Remove(ntbMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void CameraChanged(object sender, Listing.EquipmentUpdateArgs<int> args) {
			Invoke(delegate {
				var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Root");
				int pages = ntbCamera.NPages;
				if (args.Missing) {
					if (args.Index == pages - 1) {
						RemoveCamera();
					} else {
						Debug.WriteLine(this.GetType().Name + ": Camera modified out of order: " + args.Index);
					}

				} else { // can occur multiple times (passive/active) -> debug message
					if (args.Index == pages) {
						AddCamera();
					} else {
						Debug.WriteLine(this.GetType().Name + ": Camera modified out of order: " + args.Index);
					}
				}
			});
		}
		public bool AddCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbCamera.NPages;
			var factory = new Irseny.Iface.Main.View.Camera.WebcamFactory(page);
			ConstructFloor(string.Format("Camera{0}", page), factory);
			var boxInner = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label(string.Format("Cam{0}", page));
			factory.Container.AddWidget(label);
			ntbCamera.AppendPage(boxInner, label);
			ntbCamera.ShowAll();
			return true;
		}
		public bool RemoveCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbCamera.NPages - 1;
			if (page > -1) {
				ntbCamera.RemovePage(page);
				IInterfaceFactory floor = DestructFloor(string.Format("Camera{0}", page));
				floor.Dispose();
				return true;
			}
			return false;
		}
	}
}

