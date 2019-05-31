using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Tracap;
using Irseny.Inco.Device;
using Irseny.Listing;
using Irseny.Log;

namespace Irseny.Iface.Main.View.Bindings {
	public class BindingTabFactory : InterfaceFactory {
		int trackerIndex;
		// current selection
		CapAxis activeTrackerAxis = CapAxis.Yaw;
		int activeDeviceIndex = -1;
		int activeDeviceKeyIndex = -1;
		object activeAxisTranslation = null;

		// available for selection
		List<VirtualDeviceCapability> selectionDeviceCapabilities = new List<VirtualDeviceCapability>();
		List<string> selectionDeviceKeyDescriptions = new List<string>();
		List<object> selectionDeviceKeyHandles = new List<object>();
		Dictionary<int, string> selectionDevices = new Dictionary<int, string>();

		// current settings for all axes
		Dictionary<CapAxis, int> setupDeviceIndex = new Dictionary<CapAxis, int>();
		Dictionary<CapAxis, VirtualDeviceCapability> setupDeviceCapability = new Dictionary<CapAxis, VirtualDeviceCapability>();
		Dictionary<CapAxis, int> setupDeviceKeyIndex = new Dictionary<CapAxis, int>();
		Dictionary<CapAxis, object> setupAxisTranslation = new Dictionary<CapAxis, object>();

		// locking
		bool lockSelection = false;



		public BindingTabFactory(int trackerIndex) : base() {
			this.trackerIndex = trackerIndex;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("BindingTab");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			{ // connect to hall
				var boxParent = Hall.Container.GetWidget<Gtk.Box>("box_Binding");
				var boxRoot = Container.GetWidget("box_Root");
				boxParent.PackStart(boxRoot, true, true, 0);
			}
			var drwGraph = Container.GetWidget<Gtk.DrawingArea>("drw_Graph");
			{ // target selection
				var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
				var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
				cbbDevice.Changed += DeviceSelected;
				cbbCap.Changed += CapabilitySelected;
			}
			{ // initial selction
				BuildDeviceSelection();
				BuildCapabilitySelection();
			}
			// receive device updates
			EquipmentMaster.Instance.VirtualDevice.Updated += DeviceUpdated;
			Invoke(delegate {
				// delayed update after initialization finished
				EquipmentMaster.Instance.VirtualDevice.SendEquipment(DeviceUpdated);
			});

			return true;
		}
		protected override bool DisconnectInternal() {
			{  // disconnect from hall
				EquipmentMaster.Instance.VirtualDevice.Updated -= DeviceUpdated;
				var boxParent = Hall.Container.GetWidget<Gtk.Box>("box_Binding");
				var boxRoot = Container.GetWidget("box_Root");
				boxParent.Remove(boxRoot);
			}
			{ // target selection
				var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
				var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
				cbbDevice.Changed -= DeviceSelected;
				cbbCap.Changed -= CapabilitySelected;
			}
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public void RestoreBinding(CapAxis axis) {
			// activate UI
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = true;
			expRoot.Sensitive = true;
			// restore active and selection fields
			activeTrackerAxis = axis;
			if (!setupDeviceIndex.TryGetValue(axis, out activeDeviceIndex)) {
				activeDeviceIndex = -1;
			}
			// TODO: work with key instead of index
			if (!setupDeviceKeyIndex.TryGetValue(axis, out activeDeviceKeyIndex)) {
				activeDeviceKeyIndex = -1;
			}
			// rebuild UI elements
			BuildDeviceSelection();
			BuildCapabilitySelection();
			// restore active selection in UI
			SetActiveDeviceIndex(activeDeviceIndex);
			SetActiveDeviceCapability(activeDeviceKeyIndex);
			// TODO: reset state
		}
		public void Hide() {
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = false;
			expRoot.Sensitive = false;
		}
		public void ApplyBinding() {

		}

		/// <summary>
		/// Updates the displayed and selectable device information.
		/// Queries new device information first.
		/// The actual update happens delayed.
		/// May be called from any thread.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		private void DeviceUpdated(object sender, EquipmentUpdateArgs<int> args) {
			int deviceId = args.Equipment;
			int deviceIndex = args.Index;
			if (args.Active) {
				// query device info
				VirtualDeviceManager.Instance.Invoke(delegate {
					IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(deviceId);
					if (device == null) {
						LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to query device info: " + deviceId));
						return;
					}
					int deviceKey = device.DeviceIndex;
					VirtualDeviceType deviceType = device.DeviceType;

					// add entry
					Invoke(delegate {
						if (!Initialized) {
							return;
						}
						if (selectionDevices.ContainsKey(deviceIndex)) {
							LogManager.Instance.Log(LogMessage.CreateWarning(this, "Device already added: " + deviceIndex));
							return;
						}
						string reference = deviceType.ToString() + deviceKey.ToString();
						selectionDevices.Add(deviceIndex, reference);
						BuildDeviceSelection();
						DeviceSelected(sender, args);
					});
				});
			} else {
				// remove entry
				Invoke(delegate {
					if (!Initialized) {
						return;
					}
					if (!selectionDevices.ContainsKey(deviceIndex)) {
						LogManager.Instance.Log(LogMessage.CreateWarning(this, "Cannot remove unknown device: " + deviceIndex));
						return;
					}
					selectionDevices.Remove(deviceIndex);
					BuildDeviceSelection();
					DeviceSelected(sender, args);
				});
			}
		}
		/// <summary>
		/// Updates the capability selection.
		/// Device capabilities are queried from the device. The actual update happens delayed.
		/// Must be called from the UI thread.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		private void DeviceSelected(object sender, EventArgs args) {
			if (!Initialized) {
				return;
			}
			if (lockSelection) {
				return;
			}
			activeDeviceIndex = GetActiveDeviceIndex();
			if (activeDeviceIndex < 0) {
				ClearCapabilitySelection();
				return;
			}
			// TODO: update saved config
			int deviceId = EquipmentMaster.Instance.VirtualDevice.GetEquipment(activeDeviceIndex, -1);
			if (deviceId < 0) {
				LogManager.Instance.Log(LogMessage.CreateWarning(this, "Cannot query capabilities of an unregistered device: " + activeDeviceIndex));
				ClearCapabilitySelection();
				return;
			}
			VirtualDeviceManager.Instance.Invoke(delegate {
				IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(deviceId);
				if (device == null) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Cannot query capabilities of an unavailable device: " + deviceId));
					return;
				}
				var capabilities = new List<VirtualDeviceCapability>();
				var descriptions = new List<string>();
				var handles = new List<object>();
				VirtualDeviceCapability[] tmpCaps = device.GetSupportedCapabilities();
				foreach (VirtualDeviceCapability cap in tmpCaps) {
					string[] tmpDescriptions = device.GetKeyDescriptions(cap);
					object[] tmpHandles = device.GetKeyHandles(cap);
					for (int i = 0; i < tmpHandles.Length; i++) {
						capabilities.Add(cap);
						descriptions.Add(tmpDescriptions[i]);
						handles.Add(tmpHandles[i]);
					}
				}
				this.Invoke(delegate {
					selectionDeviceCapabilities = capabilities;
					selectionDeviceKeyDescriptions = descriptions;
					selectionDeviceKeyHandles = handles;
					setupDeviceIndex[activeTrackerAxis] = activeDeviceIndex;
					BuildCapabilitySelection();
				});
			});
			// TODO: apply binding
		}
		private void CapabilitySelected(object sender, EventArgs args) {
			// TODO: apply binding
			if (!Initialized) {
				return;
			}
			if (lockSelection) {
				return;
			}
			activeDeviceKeyIndex = GetActiveDeviceCapability();
			if (activeDeviceKeyIndex < 0) {
				ClearCapabilitySelection();
			}
			setupDeviceKeyIndex[activeTrackerAxis] = activeDeviceKeyIndex;
			// TODO: update saved config
		}
		/// <summary>
		/// Rebuilds the selectable devices UI element.
		/// Uses the information currently available.
		/// </summary>
		private void BuildDeviceSelection() {
			// disable change events
			lockSelection = true;
			var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
			var store = (Gtk.ListStore)cbbDevice.Model;
			string activeEntry = cbbDevice.ActiveText;
			// rebuild the list
			store.Clear();
			store.AppendValues("None");
			int iEntry = 1;
			int iActiveEntry = 0;
			foreach (string r in selectionDevices.Values) {
				if (r.Equals(activeEntry)) {
					iActiveEntry = iEntry;
				}
				store.AppendValues(r);
				iEntry += 1;
			}
			// restore the previous selection
			cbbDevice.Active = iActiveEntry;
			if (iActiveEntry == 0) {
				ClearCapabilitySelection();
			}
			cbbDevice.QueueDraw();
			// enable change events
			lockSelection = false;
		}
		/// <summary>
		/// Clears the device selection so that no valid values can be selected.
		/// </summary>
		private void ClearDeviceSelection() {
			// disable events
			lockSelection = true;
			// clear list
			var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
			var store = (Gtk.ListStore)cbbDevice.Model;
			store.Clear();
			store.AppendValues("None");
			cbbDevice.Active = 0;
			cbbDevice.QueueDraw();
			// enable events
			lockSelection = false;
		}
		/// <summary>
		/// Clears the capability selection so that no valid values can be selected.
		/// </summary>
		private void ClearCapabilitySelection() {
			// disable events
			lockSelection = true;
			// clear list
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			var store = (Gtk.ListStore)cbbCap.Model;
			store.Clear();
			store.AppendValues("None");
			cbbCap.Active = 0;
			cbbCap.QueueDraw();
			// enable events
			lockSelection = false;
		}
		/// <summary>
		/// Rebuilds the selectable capability UI element.
		/// Uses the capability information currently available.
		/// </summary>
		private void BuildCapabilitySelection() {
			// disable events
			lockSelection = true;
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			var store = (Gtk.ListStore)cbbCap.Model;
			string activeEntry = cbbCap.ActiveText;
			// rebuild list
			store.Clear();
			store.AppendValues("None");
			int iActiveEntry = 0;
			for (int i = 0; i < selectionDeviceKeyDescriptions.Count; i++) {
				string entry = selectionDeviceKeyDescriptions[i];
				if (entry.Equals(activeEntry)) {
					// select the currently created entry
					// entry 0 is None
					iActiveEntry = i + 1;
				}
				store.AppendValues(entry);
			}
			// restore previous selection
			cbbCap.Active = iActiveEntry;
			cbbCap.QueueDraw();
			// enable events
			lockSelection = false;
		}
		/// <summary>
		/// Gets the index of the currently selected device.
		/// The information is read from UI elements.
		/// </summary>
		/// <returns>The active device index. -1 if none is selected or if the selection is illegal</returns>
		private int GetActiveDeviceIndex() {
			var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
			string sActive = cbbDevice.ActiveText;
			if (sActive == null || sActive.Length == 0) {
				return -1;
			}
			foreach (var pair in selectionDevices) {
				if (sActive.Equals(pair.Value)) {
					return pair.Key;
				}
			}
			return -1;
		}
		private bool SetActiveDeviceIndex(int deviceIndex) {
			var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
			cbbDevice.Active = deviceIndex + 1; // skip None
			return true;
		}
		/// <summary>
		/// Gets the currently selected device capability.
		/// The information is read from UI elements.
		/// </summary>
		/// <returns>The active device capability information. null if none is selected of if the selection is illegal.</returns>
		private int GetActiveDeviceCapability() {
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			string sActive = cbbCap.ActiveText;
			if (sActive == null || sActive.Length == 0) {
				return -1;
			}
			for (int i = 0; i < selectionDeviceKeyHandles.Count; i++) {
				if (sActive.Equals(selectionDeviceKeyDescriptions[i])) {
					return i;
				}
			}
			return -1;
		}
		private bool SetActiveDeviceCapability(int keyIndex) {
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			cbbCap.Active = keyIndex + 1; // skip None
			return true;
		}
	}
}
