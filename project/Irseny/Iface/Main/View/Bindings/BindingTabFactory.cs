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
		CapAxis activeTrackerAxis = CapAxis.Yaw;
		int activeDeviceIndex = -1;
		VirtualDeviceCapability activeDeviceCapability = VirtualDeviceCapability.Key;
		object activeKeyHandle = null;
		string activeKeyDescription = string.Empty;
		object activeAxisMapping = null;

		// available for selection
		List<VirtualDeviceCapability> selectionDeviceCapabilities = new List<VirtualDeviceCapability>();
		List<string> selectionKeyDescriptions = new List<string>();
		List<object> selectionKeyHandles = new List<object>();
		Dictionary<int, string> selectionDeviceNames = new Dictionary<int, string>();

		// current settings for all axes
		Dictionary<CapAxis, int> setupDeviceIndexes = new Dictionary<CapAxis, int>();
		Dictionary<CapAxis, VirtualDeviceCapability> setupDeviceCapabilities = new Dictionary<CapAxis, VirtualDeviceCapability>();
		Dictionary<CapAxis, object> setupKeyHandles = new Dictionary<CapAxis, object>();
		Dictionary<CapAxis, string> setupKeyDescriptions = new Dictionary<CapAxis, string>();
		Dictionary<CapAxis, object> setupAxisMappings = new Dictionary<CapAxis, object>();

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
				ICapTracker tracker = DetectionSystem.Instance.GetDetector<ICapTracker>(trackerId, null);
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
				ICapTracker tracker = DetectionSystem.Instance.GetDetector<ICapTracker>(trackerId, null);
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
			// activate UI
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = true;
			//expRoot.Sensitive = true;
			// restore active and selection fields
			activeTrackerAxis = axis;
			if (!setupDeviceIndexes.TryGetValue(axis, out activeDeviceIndex)) {
				activeDeviceIndex = -1;
			}
			if (!setupKeyHandles.TryGetValue(axis, out activeKeyHandle)) {
				activeKeyHandle = null;
			}
			if (!setupKeyDescriptions.TryGetValue(axis, out activeKeyDescription)) {
				activeKeyDescription = string.Empty;
			}
			// rebuild UI elements
			BuildDeviceSelection();
			BuildCapabilitySelection();
			// restore active selection in UI
			SetActiveDeviceIndex(activeDeviceIndex);
			SetActiveDeviceCapability(activeKeyDescription);
			// TODO: reset state
		}
		public void Hide() {
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = false;
			//expRoot.Sensitive = false;
		}
		public void ApplyBinding() {

		}
		public bool ApplyConfig(CapInputRelay config) {
			if (config == null) throw new ArgumentNullException("config");
			if (!Initialized) {
				return false;
			}
			// TODO: ditch setup in favor of relay
			foreach (var axis in (CapAxis[])Enum.GetValues(typeof(CapAxis))) {
				int deviceIndex = config.GetDeviceIndex(axis);
				if (deviceIndex < 0) {
					setupDeviceIndexes[axis] = -1;
					setupDeviceCapabilities[axis] = VirtualDeviceCapability.Key;
					setupKeyHandles[axis] = null;
					setupKeyDescriptions[axis] = string.Empty;
					setupAxisMappings[axis] = null;
				} else {
					setupDeviceIndexes[axis] = deviceIndex;
					setupDeviceCapabilities[axis] = config.GetDeviceCapability(axis);
					var keys = config.GetDeviceKeys(axis);
					setupKeyHandles[axis] = keys.Item1;
					setupKeyDescriptions[axis] = keys.Item1.ToString();
					setupAxisMappings[axis] = config.GetMapping(axis);
				}
				activeDeviceIndex = -1;
			}
			// copy the given configuration and connect it to the tracker
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					LogManager.Instance.LogError(this, "Tracker " + trackerIndex + " not available");
					return;
				}
				ICapTracker tracker = DetectionSystem.Instance.GetDetector<ICapTracker>(trackerId, null);
				if (tracker == null) {
					LogManager.Instance.LogError(this, "Tracker " + trackerIndex + " not available");
					return;
				}
				// the tracker could have changed 
				// but in case it has not we remove the event handler anyway
				// and exchange it with the new instance
				tracker.PositionDetected -= inputHandler.PositionChanged;
				inputHandler = CapInputRelay.CreateCopy(config);
				tracker.PositionDetected += inputHandler.PositionChanged;
			});
			return true;
		}
		public CapInputRelay GetConfig() {
			return CapInputRelay.CreateCopy(inputHandler);
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
			activeDeviceIndex = GetActiveDeviceIndex();
			if (activeDeviceIndex < 0) {
				ClearCapabilitySelection();
				setupDeviceIndexes[activeTrackerAxis] = -1;
				setupKeyHandles[activeTrackerAxis] = null;
				setupKeyDescriptions[activeTrackerAxis] = string.Empty;
				SyncTranslation(activeTrackerAxis);
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
					selectionKeyDescriptions = descriptions;
					selectionKeyHandles = handles;
					setupDeviceIndexes[activeTrackerAxis] = activeDeviceIndex;
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
			Tuple<VirtualDeviceCapability, object, string> activeKey = GetActiveDeviceCapability();
			activeDeviceCapability = activeKey.Item1;
			activeKeyHandle = activeKey.Item2;
			activeKeyDescription = activeKey.Item3;
			if (activeKey.Item2 == null || activeKey.Item3.Length == 0) {
				ClearCapabilitySelection();
			}
			setupDeviceCapabilities[activeTrackerAxis] = activeKey.Item1;
			setupKeyHandles[activeTrackerAxis] = activeKey.Item2;
			setupKeyDescriptions[activeTrackerAxis] = activeKey.Item3;
			SyncTranslation(activeTrackerAxis);
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
			for (int i = 0; i < selectionKeyDescriptions.Count; i++) {
				string entry = selectionKeyDescriptions[i];
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
			return true;
		}
		/// <summary>
		/// Gets the currently selected device capability.
		/// The information is read from UI elements.
		/// </summary>
		/// <returns>The active device capability information. null if none is selected of if the selection is illegal.</returns>
		private Tuple<VirtualDeviceCapability, object, string> GetActiveDeviceCapability() {
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			string sActive = cbbCap.ActiveText;
			if (sActive == null || sActive.Length == 0) {
				return Tuple.Create<VirtualDeviceCapability, object, string>(VirtualDeviceCapability.Key, null, string.Empty);
			}
			for (int i = 0; i < selectionKeyHandles.Count; i++) {
				if (sActive.Equals(selectionKeyDescriptions[i])) {
					return Tuple.Create(selectionDeviceCapabilities[i], selectionKeyHandles[i], selectionKeyDescriptions[i]);
				}
			}
			return Tuple.Create<VirtualDeviceCapability, object, string>(VirtualDeviceCapability.Key, null, string.Empty);
		}
		/// <summary>
		/// Sets the currently selected device capability visualization.
		/// </summary>
		/// <returns><c>true</c>, if active device capability was set, <c>false</c> otherwise.</returns>
		/// <param name="keyDescription">Key name.</param>
		private bool SetActiveDeviceCapability(string keyDescription) {
			var cbbCap = Container.GetWidget<Gtk.ComboBoxText>("cbb_Capability");
			int keyIndex;
			for (keyIndex = 0; keyIndex < selectionKeyDescriptions.Count; keyIndex++) {
				if (selectionKeyDescriptions[keyIndex].Equals(keyDescription)) {
					break;
				}
			}
			if (keyIndex < selectionKeyDescriptions.Count) {
				cbbCap.Active = keyIndex + 1; // skip None
			} else {
				// TODO: report
				cbbCap.Active = 0; // None
			}
			return true;
		}
		/// <summary>
		/// Updates the translation to the currently selected values.
		/// Updates are only applied to the given axis.
		/// </summary>
		/// <param name="axis">Axis to update.</param>
		private void SyncTranslation(CapAxis axis) {
			int deviceIndex;
			bool clear = false;
			if (!setupDeviceIndexes.TryGetValue(axis, out deviceIndex)) {
				clear = true;
			}
			object keyHandle;
			if (!setupKeyHandles.TryGetValue(axis, out keyHandle)) {
				clear = true;
			}
			VirtualDeviceCapability capability;
			if (!setupDeviceCapabilities.TryGetValue(axis, out capability)) {
				clear = true;
			}
			object mapping = null;
			if (clear) {
				inputHandler.RemoveBinding(axis);
			} else {
				inputHandler.AddBinding(axis, deviceIndex, capability, keyHandle, keyHandle, mapping);
			}
		}
	}
}
