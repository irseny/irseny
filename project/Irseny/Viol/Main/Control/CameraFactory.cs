using System;

namespace Irseny.Viol.Main.Control {
	public class CameraFactory : InterfaceFactory {
		public CameraFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.Master.Instance.Resources.InterfaceDefinitions.GetEntry("CameraControl"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var boxMain = Container.GetWidget("box_Root");
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
	}
}

