using System;
namespace Irseny.Viol.Main.Control.Tracking {
	public class TrackingFactory : InterfaceFactory {
		readonly int index;
		public TrackingFactory(int index) : base() {
			this.index = index;
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("TrackingControl"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			return true;
		}
		protected override bool DisconnectInternal() {
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
	}
}
