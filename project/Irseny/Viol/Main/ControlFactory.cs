using System;

namespace Irseny.Viol.Main {
	public class ControlFactory : InterfaceFactory {
		public ControlFactory() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.Master.Instance.Resources.InterfaceDefinitions.GetEntry("Control"));
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Control");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.PackStart(ntbMain);
			return true;
		}
		protected override bool DisconnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Control");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.Remove(ntbMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
	}
}

