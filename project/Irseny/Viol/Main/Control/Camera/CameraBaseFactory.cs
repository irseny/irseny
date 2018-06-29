﻿using System;

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
			/*var btnAdd = Container.GetWidget<Gtk.Button>("btn_Add");
			btnAdd.Clicked = null;
			var btnRemove = Container.GetWidget<Gtk.Button>("btn_Remove");
			btnRemove.Clicked = null;*/
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
				var factory = new Camera.CameraFactory(page);
				ConstructFloor(string.Format("Camera{0}", page), factory);
				var boxInner = factory.Container.GetWidget("box_Root");
				var label = new Gtk.Label(string.Format("Cam{0}", page));
				factory.Container.AddWidget(label);
				ntbCamera.AppendPage(boxInner, label);
				ntbCamera.ShowAll();
				// update video sources
				Listing.EquipmentMaster.Instance.VideoSource.Update(page, true, page);
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
				Listing.EquipmentMaster.Instance.VideoSource.Update(page, false, page);
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

