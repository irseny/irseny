using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Tracking;
using Irseny.Inco.Device;
using Irseny.Listing;
using Irseny.Log;

namespace Irseny.Iface.Main.View.Bindings {
	public class BindingTabFactory : InterfaceFactory {
		int trackerIndex;
		CapInputRelay inputHandler = new CapInputRelay();
		// current selection
		bool axisSelected = false;
		CapAxis activeTrackerAxis = CapAxis.Yaw;
		int activeDeviceIndex = -1;
		VirtualDeviceCapability activeDeviceCapability = VirtualDeviceCapability.Key;
		object activeKeyHandle = null;
		object activeAxisMapping = null;

		// available for selection
		List<VirtualDeviceCapability> selectionDeviceCapabilities = new List<VirtualDeviceCapability>();
		//List<string> selectionKeyDescriptions = new List<string>();
		List<object> selectionKeyHandles = new List<object>();
		Dictionary<int, string> selectionDeviceNames = new Dictionary<int, string>();

		// current settings for all axes
		/*Dictionary<CapAxis, int> setupDeviceIndexes = new Dictionary<CapAxis, int>();
		Dictionary<CapAxis, VirtualDeviceCapability> setupDeviceCapabilities = new Dictionary<CapAxis, VirtualDeviceCapability>();
		Dictionary<CapAxis, object> setupKeyHandles = new Dictionary<CapAxis, object>();
		Dictionary<CapAxis, string> setupKeyDescriptions = new Dictionary<CapAxis, string>();
		Dictionary<CapAxis, object> setupAxisMappings = new Dictionary<CapAxis, object>();*/

		// locking
		bool lockSelection = false;



		public BindingTabFactory(int trackerIndex) : base() {
			this.trackerIndex = trackerIndex;
		}
		public int TrackerIndex {
			get { return trackerIndex; }
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("BindingTab");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			axisSelected = false;
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
			{ // receive device updates
				EquipmentMaster.Instance.VirtualDevice.Updated += DeviceUpdated;
			}
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					LogManager.Instance.LogError(this, "Tracker " + trackerIndex + " not available");
					return;
				}
				IPoseTracker tracker = DetectionSystem.Instance.GetTracker(trackerId);
				if (tracker == null) {
					LogManager.Instance.LogError(this, "Tracker " + trackerIndex + " not available");
					return;
				}
				tracker.PositionDetected += inputHandler.PositionChanged;
			});
			Invoke(delegate {
				// delayed update after initialization finished
				EquipmentMaster.Instance.VirtualDevice.SendEquipment(DeviceUpdated);
			});

			return true;
		}
		protected override bool DisconnectInternal() {
			axisSelected = false;
			{ // stop updates
				EquipmentMaster.Instance.VirtualDevice.Updated -= DeviceUpdated;
			}
			{  // disconnect from hall

				var boxParent = Hall.Container.GetWidget<Gtk.Box>("box_Binding");
				var boxRoot = Container.GetWidget("box_Root");
				boxParent.Remove(boxRoot);// TODO: fix program termination due to access violation
			}
			{ // target selection
				var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
				var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
				cbbDevice.Changed -= DeviceSelected;
				cbbCap.Changed -= CapabilitySelected;
			}
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					return;
				}
				IPoseTracker tracker = DetectionSystem.Instance.GetTracker(trackerId);
				if (tracker == null) {
					return;
				}
				tracker.PositionDetected -= inputHandler.PositionChanged;
			});
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public void RestoreBinding(CapAxis axis) {
			if (!Initialized) {
				return;
			}
			// activate UI
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = true;
			expRoot.Sensitive = true;
			// restore active and selection fields
			axisSelected = true;
			activeTrackerAxis = axis;
			activeDeviceIndex = inputHandler.GetDeviceIndex(axis);
			if (activeDeviceIndex < 0) {
				activeKeyHandle = null;
			} else {
				Tuple<object, object> keys = inputHandler.GetDeviceKeys(axis);
				activeKeyHandle = keys.Item1;
			}
			// rebuild UI elements
			BuildDeviceSelection();
			BuildCapabilitySelection();
			// restore active selection in UI
			SetActiveDeviceIndex(activeDeviceIndex);
			SetActiveDeviceCapability(activeKeyHandle);
			// TODO: find out why the capability is not restored correctly on the first restoration
		}
		public void Hide() {
			if (!Initialized) {
				return;
			}
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = false;
			expRoot.Sensitive = false;
			axisSelected = false;
			activeDeviceIndex = -1;
			activeKeyHandle = null;
			activeAxisMapping = null;
		}
		public void ApplyBinding() {

		}
		public bool ApplySettings(CapInputRelay settings) {
			if (settings == null) throw new ArgumentNullException("config");
			if (!Initialized) {
				return false;
			}
			// reset the view
			Hide();
			// for all axes remove existing bindings and apply the given ones
			foreach (var axis in (CapAxis[])Enum.GetValues(typeof(CapAxis))) {
				inputHandler.RemoveBinding(axis);
				int deviceIndex = settings.GetDeviceIndex(axis);
				if (deviceIndex >= 0) {
					object mapping = settings.GetMapping(axis);
					VirtualDeviceCapability capability = settings.GetDeviceCapability(axis);
					Tuple<object, object> keys = settings.GetDeviceKeys(axis);
					inputHandler.AddBinding(axis, deviceIndex, capability, keys.Item1, keys.Item2, mapping);
				}
			}
			return true;
		}
		public CapInputRelay GetSettings() {
			return new CapInputRelay(inputHandler);
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
						if (selectionDeviceNames.ContainsKey(deviceIndex)) {
							LogManager.Instance.Log(LogMessage.CreateWarning(this, "Device already added: " + deviceIndex));
							return;
						}
						string reference = deviceType.ToString() + deviceKey.ToString();
						selectionDeviceNames.Add(deviceIndex, reference);
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
					if (!selectionDeviceNames.ContainsKey(deviceIndex)) {
						LogManager.Instance.Log(LogMessage.CreateWarning(this, "Cannot remove unknown device: " + deviceIndex));
						return;
					}
					selectionDeviceNames.Remove(deviceIndex);
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
			if (!axisSelected) {
				return;
			}
			activeDeviceIndex = GetActiveDeviceIndex();
			// TODO: remove the current binding if the device index changed
			if (activeDeviceIndex < 0) {
				ClearCapabilitySelection();
				inputHandler.RemoveBinding(activeTrackerAxis);
				// otherwise the bindings are updated further down the line
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
				var handles = new List<object>();
				VirtualDeviceCapability[] tmpCaps = device.GetSupportedCapabilities();
				foreach (VirtualDeviceCapability cap in tmpCaps) {
					object[] tmpHandles = device.GetKeyHandles(cap);
					for (int i = 0; i < tmpHandles.Length; i++) {
						capabilities.Add(cap);
						handles.Add(tmpHandles[i]);
					}
				}
				this.Invoke(delegate {
					selectionDeviceCapabilities = capabilities;
					selectionKeyHandles = handles;
					BuildCapabilitySelection();
					// TODO: trigger capability selection here if it does not work already to update the input handler
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
			if (!axisSelected) {
				return;
			}
			Tuple<VirtualDeviceCapability, object> key = GetActiveDeviceCapability();
			activeDeviceCapability = key.Item1;
			activeKeyHandle = key.Item2;
			inputHandler.RemoveBinding(activeTrackerAxis);
			if (activeKeyHandle == null) {
				ClearCapabilitySelection();
			} else if (activeDeviceIndex >= 0) {
				inputHandler.AddBinding(
					activeTrackerAxis, activeDeviceIndex, activeDeviceCapability,
					activeKeyHandle, activeKeyHandle, activeAxisMapping);
			}
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
			foreach (string r in selectionDeviceNames.Values) {
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
			for (int i = 0; i < selectionKeyHandles.Count; i++) {
				object entry = selectionKeyHandles[i].ToString();
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
			foreach (var pair in selectionDeviceNames) {
				if (sActive.Equals(pair.Value)) {
					return pair.Key;
				}
			}
			return -1;
		}
		private bool SetActiveDeviceIndex(int deviceIndex) {
			var cbbDevice = Container.GetWidget<Gtk.ComboBoxText>("cbb_Target");
			cbbDevice.Active = deviceIndex + 1; // skip None
			cbbDevice.QueueDraw();
			return true;
		}
		/// <summary>
		/// Gets the currently selected device capability.
		/// The information is read from UI elements.
		/// </summary>
		/// <returns>The active device capability information. null if none is selected of if the selection is illegal.</returns>
		private Tuple<VirtualDeviceCapability, object> GetActiveDeviceCapability() {
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			string sActive = cbbCap.ActiveText;
			if (sActive == null || sActive.Length == 0) {
				return Tuple.Create<VirtualDeviceCapability, object>(VirtualDeviceCapability.Key, null);
			}
			for (int i = 0; i < selectionKeyHandles.Count; i++) {
				string entry = selectionKeyHandles[i].ToString();
				if (sActive.Equals(entry)) {
					return Tuple.Create(selectionDeviceCapabilities[i], selectionKeyHandles[i]);
				}
			}
			return Tuple.Create<VirtualDeviceCapability, object>(VirtualDeviceCapability.Key, null);
		}
		/// <summary>
		/// Sets the currently selected device capability visualization.
		/// </summary>
		/// <returns><c>true</c>, if active device capability was set, <c>false</c> otherwise.</returns>
		/// <param name="keyHandle">Key.</param>
		private bool SetActiveDeviceCapability(object keyHandle) {
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			string keyDescription = keyHandle != null ? keyHandle.ToString() : string.Empty;
			int iKey;
			for (iKey = 0; iKey < selectionKeyHandles.Count; iKey++) {
				string entry = selectionKeyHandles[iKey].ToString();
				if (keyDescription.Equals(entry)) {
					break;
				}
			}
			if (iKey < selectionKeyHandles.Count) {
				cbbCap.Active = iKey + 1; // skip None
			} else {
				// TODO: report
				cbbCap.Active = 0; // None
			}
			cbbCap.QueueDraw();
			return true;
		}
	}
}
