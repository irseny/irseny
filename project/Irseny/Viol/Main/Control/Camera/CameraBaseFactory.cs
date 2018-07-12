using System;

namespace Irseny.Viol.Main.Control.Camera {
	public class CameraBaseFactory : InterfaceFactory {
		public CameraBaseFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("CameraControlBase"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var boxMain = Container.GetWidget("box_Root");
			var btnAdd = Container.GetWidget<Gtk.Button>("btn_Add");
			btnAdd.Clicked += delegate {
				AddCamera();
			};

			var btnRemove = Container.GetWidget<Gtk.Button>("btn_Remove");
			btnRemove.Clicked += delegate {
				RemoveCamera();
			};

			boxRoot.PackStart(boxMain);
			return true;
		}
		protected override bool DisconnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public bool AddCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			int page = ntbCamera.NPages;
			// create and append
			if (page < 10) {
				// also changed in the inner factory
				Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(page, Listing.EquipmentState.Passive, page);
				var factory = new Camera.CameraFactory(page);
				ConstructFloor(string.Format("Camera{0}", page), factory);
				var boxInner = factory.Container.GetWidget("box_Root");
				var label = new Gtk.Label(string.Format("Cam{0}", page));
				factory.Container.AddWidget(label);
				ntbCamera.AppendPage(boxInner, label);
				ntbCamera.ShowAll();


				return true;
			} else {
				return false;
			}
		}
		public bool RemoveCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			int page = ntbCamera.NPages - 1;
			// remove last
			if (page > -1) {
				ntbCamera.RemovePage(page);
				IInterfaceFactory floor = DestructFloor(string.Format("Camera{0}", page));
				floor.Dispose();
				// also changed in the floor
				Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(page, Listing.EquipmentState.Missing, page);
				return true;
			} else {
				return false;
			}
		}
	}
}

