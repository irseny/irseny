using System;

namespace Irseny.Viol.Main {
	public class LogFactory : InterfaceFactory {
		public LogFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile("../../resources/gtk/Log.glade");
			Container = factory.CreateWidget("pnl_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var hallRoot = Hall.Container.GetWidget<Gtk.Paned>("spl_ImageLog");
			var pnlRoot = Container.GetWidget("pnl_Root");
			hallRoot.Pack2(pnlRoot, true, true);
			return true;
		}
		protected override bool DisconnectInternal() {
			var hallRoot = Hall.Container.GetWidget<Gtk.Paned>("spl_ImageLog");
			hallRoot.Pack2(null, false, false); // null widget generates criticall assertion fail
			return true;
		}
		protected override bool DestroyInternal() {
			// nothing to do
			return true;
		}
	}
}

