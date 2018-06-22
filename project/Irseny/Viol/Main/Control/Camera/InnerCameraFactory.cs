using System;
namespace Irseny.Viol.Main.Control.Camera {
	public class InnerCameraFactory : InterfaceFactory {
		public InnerCameraFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.Master.Instance.Resources.InterfaceDefinitions.GetEntry("InnerCameraControl"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			// TODO: connect value setting widgets with value visualizers
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
