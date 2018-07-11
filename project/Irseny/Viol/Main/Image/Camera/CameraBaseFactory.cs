using System;
using System.Diagnostics;

namespace Irseny.Viol.Main.Image.Camera {
	public class CameraBaseFactory : InterfaceFactory {
		public CameraBaseFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("CameraImageBase"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.PackStart(boxMain);
			Listing.EquipmentMaster.Instance.VideoSource.Updated += CameraChanged;
			return true;
		}
		protected override bool DisconnectInternal() {
			Listing.EquipmentMaster.Instance.VideoSource.Updated -= CameraChanged;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void CameraChanged(object sender, Listing.EquipmentUpdateArgs<int> args) {
			Invoke(delegate {
				var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
				int pages = ntbCamera.NPages;
				if (args.Available) {
					if (args.Index == pages) {
						AddCamera();
					} else {
						Debug.WriteLine("changed camera source index out of order: " + args.Index);
					}
				} else {
					if (args.Index == pages - 1) {
						RemoveCamera();
					} else {
						Debug.WriteLine("changed camera source index out of order: " + args.Index);
					}
				}
			});
		}
		public bool AddCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			int page = ntbCamera.NPages;
			var factory = new Camera.CameraFactory(page);
			ConstructFloor(string.Format("Camera{0}", page), factory);
			var boxInner = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label(string.Format("Cam{0}", page));
			factory.Container.AddWidget(label);
			ntbCamera.AppendPage(boxInner, label);
			ntbCamera.ShowAll();

			return true;
		}
		public bool RemoveCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			int page = ntbCamera.NPages - 1;
			if (page > -1) {
				ntbCamera.RemovePage(page);
				IInterfaceFactory floor = DestructFloor(string.Format("Camera{0}", page));
				floor.Dispose();
				return true;
			} else {
				return false;
			}
		}
	}
}

