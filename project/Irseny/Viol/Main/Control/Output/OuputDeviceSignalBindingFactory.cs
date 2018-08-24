using System;
using Irseny.Content;
using Irseny.Inco.Device;
using Irseny.Listing;
using Irseny.Log;

namespace Irseny.Viol.Main.Control.Output {
	public class OuputDeviceSignalBindingFactory : InterfaceFactory {
		readonly int outputDeviceId;
		public OuputDeviceSignalBindingFactory(int outputDeviceId) : base() {
			this.outputDeviceId = outputDeviceId;
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
		private void FindSourceCandidates() {

		}
		private void FindSelectedSourceCapabilities() {

		}
		private void FindSelectedSourceKeys() {

		}
		private void FindTargetCapabilities() {

		}
		private void FindTargetKeys() {
			int activeDeviceId = outputDeviceId;
			VirtualDeviceCapability capability;
			if (!GetActiveTargetCapability(out capability)) {
				return;
			}
			VirtualDeviceManager.Instance.Invoke(delegate {

			});
		}
		private bool GetActiveTargetCapability(out VirtualDeviceCapability capability) {
			var cbbCapability = Container.GetWidget<Gtk.ComboBoxText>("cbb_TargetCapability");
			string text = cbbCapability.ActiveText.ToLower();
			if (text.Equals("button")) {
				capability = VirtualDeviceCapability.Button;
				return true;
			} else if (text.Equals("axis")) {
				capability = VirtualDeviceCapability.Axis;
				return true;
			} else {
				capability = default(VirtualDeviceCapability);
				return false;
			}
		}
		private bool GetActiveSourceId() {
			return false;
		}
	}
}
