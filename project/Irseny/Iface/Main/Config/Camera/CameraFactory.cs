using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Content.Profile;
using Irseny.Util;
using Irseny.Capture.Video;

namespace Irseny.Iface.Main.Config.Camera {
	public class CameraFactory : InterfaceFactory {
		const string TitlePrefix = "Cam";
		const string FloorPrefix = "Camera";

		public CameraFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CameraConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var boxMain = Container.GetWidget("box_Root");
			var btnAdd = Container.GetWidget<Gtk.Button>("btn_Add");
			btnAdd.Clicked += delegate {
				AddFreeWebcam();
			};

			var btnRemove = Container.GetWidget<Gtk.Button>("btn_Remove");
			btnRemove.Clicked += delegate {
				RemoveSelectedWebcam();
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
		public CaptureSettings GetWebcamSettings(int iCamera) {
			foreach (IInterfaceFactory floor in Floors) {
				var factory = (WebcamFactory)floor;
				if (factory.StreamIndex == iCamera) {
					return factory.GetSettings();
				}
			}
			return null;
		}
		private bool AddFreeWebcam() {
			if (!Initialized) {
				return false;
			}
			// get non free indexes
			var claimed = new List<int>();
			foreach (IInterfaceFactory floor in Floors) {
				var factory = (WebcamFactory)floor;
				claimed.Add(factory.StreamIndex);
			}
			// find the first free index, capped at 16
			int iFree;
			for (iFree = 0; iFree < 16; iFree++) {
				bool available = true;
				foreach (int iClaimed in claimed) {
					if (iClaimed == iFree) {
						available = false;
						break;
					}
				}
				if (available) {
					break;
				}
			}
			// add a new camera
			if (iFree >= 16) {
				return false;
			}
			return AddWebcam(iFree, new CaptureSettings());
		}
		public bool AddWebcam(int iCamera, CaptureSettings settings) {
			if (!Initialized) {
				return false;
			}
			// create floor and append it to notebook
			var factory = new WebcamFactory(iCamera);
			if (!ConstructFloor(FloorPrefix + iCamera, factory)) {
				return false;
			}
			if (!factory.ApplySettings(settings)) {
				return false;
			}
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			int iPage = ntbCamera.NPages;
			var page = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label(TitlePrefix + iCamera);
			factory.Container.AddWidget(label);
			ntbCamera.AppendPage(page, label);
			ntbCamera.ShowAll();
			ntbCamera.CurrentPage = iPage;
			return true;
		}
		private bool RemoveSelectedWebcam() {
			if (!Initialized) {
				return false;
			}
			// read index from selected page and call removing method
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			int iPage = ntbCamera.CurrentPage;
			if (iPage < 0 || iPage >= ntbCamera.NPages) {
				return false;
			}
			Gtk.Widget page = ntbCamera.GetNthPage(iPage);
			string title = ntbCamera.GetTabLabelText(page);
			int iTitle = TextParseTools.ParseInt(title.Substring(TitlePrefix.Length), -1);
			if (iTitle < 0) {
				return false;
			}
			return RemoveWebcam(iTitle);
		}
		private bool RemoveWebcam(int iCamera) {
			if (!Initialized) {
				return false;
			}
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			// search for page, label
			Gtk.Widget label = null;
			int pageNo = ntbCamera.NPages;
			int iPage;
			for (iPage = 0; iPage < pageNo; iPage++) {
				Gtk.Widget page = ntbCamera.GetNthPage(iPage);
				string title = ntbCamera.GetTabLabelText(page);
				int iTitle = TextParseTools.ParseInt(title.Substring(TitlePrefix.Length), -1);
				if (iTitle == iCamera) {
					label = ntbCamera.GetTabLabel(page);
					break;
				}
			}
			if (iPage >= pageNo) {
				// page not found
				return false;
			}
			// search for floor
			string floorName = string.Empty;
			foreach (string name in FloorNames) {
				int iFloor = TextParseTools.ParseInt(name.Substring(FloorPrefix.Length), -1);
				if (iFloor == iCamera) {
					floorName = name;
				}
			}
			if (floorName.Length < 1) {
				// floor not found
				return false;
			}
			// destruct
			ntbCamera.RemovePage(iPage);
			label.Dispose();
			IInterfaceFactory factory = DestructFloor(floorName);
			factory.Dispose();
			return true;

			/*int page = ntbCamera.NPages - 1;
			// remove last
			if (page > -1) {
				ntbCamera.RemovePage(page);
				IInterfaceFactory floor = DestructFloor(string.Format("Camera{0}", page));
				floor.Dispose();
				// also changed in the floor

				return true;
			} else {
				return false;
			}*/
		}
		public void RemoveWebcams() {
			if (!Initialized) {
				return;
			}
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Camera");
			while (ntbCamera.NPages > 0) {
				Gtk.Widget page = ntbCamera.GetNthPage(0);
				Gtk.Widget label = ntbCamera.GetTabLabel(page);
				ntbCamera.RemovePage(0);
				// TODO: check wheter disposing works this way
				label.Dispose();
			}
			var toDestruct = new Queue<string>(FloorNames);
			while (toDestruct.Count > 0) {
				string name = toDestruct.Dequeue();
				IInterfaceFactory factory = DestructFloor(name);
				factory.Dispose();
			}
		}

	}
}

