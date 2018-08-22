using System;
using Irseny.Content;
using Irseny.Inco;
using Irseny.Listing;
using Irseny.Log;

namespace Irseny.Viol.Main.Control.Output {
	public class OuputDeviceSignalBindingFactory : InterfaceFactory {
		public OuputDeviceSignalBindingFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("OutputDeviceSignalBinding");
			Container = factory.CreateWidget("exp_Root");
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
