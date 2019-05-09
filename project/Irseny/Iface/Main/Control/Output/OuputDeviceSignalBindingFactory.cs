using System;
using Irseny.Content;
using Irseny.Inco.Device;
using Irseny.Listing;
using Irseny.Log;

namespace Irseny.Iface.Main.Control.Output {
	public class OuputDeviceSignalBindingFactory : InterfaceFactory {
		readonly int deviceIndex;
		readonly string deviceName;
		public OuputDeviceSignalBindingFactory(int deviceIndex, string deviceName) : base() {
			this.deviceIndex = deviceIndex;
			this.deviceName = deviceName;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("OutputDeviceSignalBinding");
			Container = factory.CreateWidget("exp_Root");
			var lblTargetDevice = Container.GetWidget<Gtk.Label>("lbl_TargetDevice");
			lblTargetDevice.Text = deviceName;
			return true;
		}
		protected override bool ConnectInternal() {
			EquipmentMaster.Instance.OutputDevice.Updated += DevicesUpdated;

			var cbbTargetCapability = Container.GetWidget<Gtk.ComboBoxText>("cbb_TargetCapability");
			cbbTargetCapability.Changed += TargetCapabilityUpdated;
			FindSourceCandidates();
			FindTargetCapabilities();
			var cbbTargetKey = Container.GetWidget<Gtk.ComboBoxText>("cbb_TargetKey");
			return true;
		}
		protected override bool DisconnectInternal() {

			EquipmentMaster.Instance.OutputDevice.Updated -= DevicesUpdated;
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void FindSourceCandidates() {

		}
		private void FindSourceCapabilities() {

		}
		private void FindSourceKeys() {

		}
		private void FindTargetCapabilities() {
			// active target device
			int activeDeviceIndex;
			if (!GetActiveTargetDevice(out activeDeviceIndex)) {
				return;
			}
			// query capabilities
			VirtualDeviceManager.Instance.Invoke(delegate {
				VirtualDeviceCapability[] capabilities = GetDeviceCapability(activeDeviceIndex);
				this.Invoke(delegate {
					// fill combobox with exposed capabilities
					var cbbCapability = Container.GetWidget<Gtk.ComboBoxText>("cbb_TargetCapability");

					/*cbbCapability.Clear();
					var store = new Gtk.ListStore(typeof(string));
					foreach (var cap in capabilities) {
						store.AppendValues(cap.ToString());
					}
					cbbCapability.Model = store;
					var renderer = new Gtk.CellRendererText();
					cbbCapability.PackStart(renderer, true);
					cbbCapability.AddAttribute(renderer, "text", 0);
					cbbCapability.Active = 0;
					cbbCapability.QueueDraw();*/


					var store = (Gtk.ListStore)cbbCapability.Model;
					store.Clear();
					foreach (var cap in capabilities) {
						store.AppendValues(cap.ToString());
					}
					cbbCapability.Active = -1;
					cbbCapability.QueueDraw();


					/*cbbCapability.Clear(); // does not clear
					foreach (var cap in capabilities) {
						cbbCapability.AppendText(cap.ToString()); // does not show up
					}
					cbbCapability.Active = 0;
					cbbCapability.QueueDraw();*/
				});
			});
		}

		private void FindTargetKeys() {
			// active target device
			int activeDeviceIndex;
			if (!GetActiveTargetDevice(out activeDeviceIndex)) {
				return;
			}
			// selected capability
			VirtualDeviceCapability capability;
			if (!GetActiveTargetCapability(out capability)) {
				return;
			}
			// query keys for the active capability
			VirtualDeviceManager.Instance.Invoke(delegate {
				string[] keys = GetDeviceKeys(activeDeviceIndex, capability);
				this.Invoke(delegate {
					// fill combobox with key descriptions
					var cbbKey = Container.GetWidget<Gtk.ComboBoxText>("cbb_TargetKey");
					var store = (Gtk.ListStore)cbbKey.Model;
					store.Clear();
					foreach (string key in keys) {
						store.AppendValues(key);
					}
					cbbKey.Active = -1;
					cbbKey.QueueDraw();
				});
			});
		}

		private void DevicesUpdated(object sender, EquipmentUpdateArgs<int> args) {
			if (args.Active) {
				// update capabilities of target device
				int activeDeviceIndex;
				if (GetActiveTargetDevice(out activeDeviceIndex)) {
					if (activeDeviceIndex == args.Index) {
						this.Invoke(delegate {
							FindTargetCapabilities();
						});
					}
				}
				// update source device candidates
			}
		}
		private void TargetCapabilityUpdated(object sender, EventArgs args) {
			// update keys of target device
			FindTargetKeys();
		}
		private void TargetKeyUpdated(object sender, EventArgs args) {
			// nothing to do yet
			// update underlying device
		}
		private static VirtualDeviceCapability[] GetDeviceCapability(int deviceIndex) {
			IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(deviceIndex);
			if (device != null) {
				return device.GetSupportedCapabilities();
			} else {
				return new VirtualDeviceCapability[0];
			}
		}
		private static string[] GetDeviceKeys(int deviceIndex, VirtualDeviceCapability capability) {
			IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(deviceIndex);
			if (device != null) {
				return device.GetKeyDescriptions(capability);
			} else {
				return new string[0];
			}
		}
		private bool GetActiveTargetDevice(out int index) {
			index = deviceIndex;
			return true;
		}
		private bool GetActiveTargetCapability(out VirtualDeviceCapability capability) {
			var cbbCapability = Container.GetWidget<Gtk.ComboBoxText>("cbb_TargetCapability");

			string text = cbbCapability.ActiveText;
			if (text == null) {
				capability = default(VirtualDeviceCapability);
				return false;
			}
			text = text.ToLower();
			if (text.Equals("key")) {
				capability = VirtualDeviceCapability.Key;
				return true;
			} else if (text.Equals("button")) {
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
		private bool GetActiveTargetKey(out string key) {
			var cbbKey = Container.GetWidget<Gtk.ComboBoxText>("cbb_TargetKey");
			string text = cbbKey.ActiveText;
			key = text;
			return text.Length > 0;
		}
		private bool GetActiveSourceIndex(out int index) {
			var cbbSource = Container.GetWidget<Gtk.ComboBoxText>("cbb_SourceDevice");
			// TODO: translate from device description to device index
			index = -1;
			return false;
		}
	}
}
