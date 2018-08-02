using System;
using Irseny.Content;

namespace Irseny.Viol.Main.Control.Camera {
	public class CameraFactory : InterfaceFactory {
		public CameraFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CameraControl");
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

			boxRoot.PackStart(boxMain, true, true, 0);
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

				var factory = new Camera.WebcamFactory(page);
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

				return true;
			} else {
				return false;
			}
		}
	}
}

