using System;
using Irseny.Log;
using Irseny.Content;
using Irseny.Inco.Device;
using Irseny.Listing;
using Irseny.Util;

namespace Irseny.Iface.Main.Config.Devices {
	public class FreetrackConfigFactory : InterfaceFactory, IClassifiedDeviceConfigFactory {
		readonly int freetrackIndex;
		readonly int deviceIndex;
		VirtualDeviceSettings settings;

		public FreetrackConfigFactory(VirtualDeviceSettings settings, int deviceIndex) : base() {
			if (settings == null) throw new ArgumentNullException("settings");
			if (deviceIndex < 0) throw new ArgumentOutOfRangeException("deviceIndex");
			if (freetrackIndex < 0) throw new ArgumentOutOfRangeException("keyboardIndex");
			this.freetrackIndex = settings.ClassifiedDeviceIndex;
			this.deviceIndex = deviceIndex;
			this.settings = settings;
		}
		public int CommonDeviceIndex {
			get { return deviceIndex; }
		}
		public int ClassifiedDeviceIndex {
			get { return freetrackIndex; }
		}
		public VirtualDeviceType DeviceType {
			get { return VirtualDeviceType.TrackingInterface; }
		}
		protected override bool CreateInternal() {

			// TODO: create device and add to equipment
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("KeyboardConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			{
				var btnApply = Container.GetWidget<Gtk.Button>("btn_Apply");
				btnApply.Clicked += delegate {
					settings = GetSettings();
					ApplySettingsToModel();
				};
			}
			Invoke(delegate {
				ApplySettingsToView();
			});
			VirtualDeviceManager.Instance.Invoke(delegate {
				var device = VirtualDevice.CreateFromSettings(settings);
				int deviceId = VirtualDeviceManager.Instance.ConnectDevice(device);
				if (deviceId < 0) {
					LogManager.Instance.Log(LogEntry.CreateError(this, "Failed to connect keyboard " + freetrackIndex));
					return;
				}
				EquipmentMaster.Instance.VirtualDevice.Update(deviceIndex, EquipmentState.Active, deviceId);
			});
			return true;
		}
		protected override bool DisconnectInternal() {
			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = EquipmentMaster.Instance.VirtualDevice.GetEquipment(deviceIndex, -1);
				if (deviceId < 0) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to disconnect keyboard " + freetrackIndex));
					return;
				}
				EquipmentMaster.Instance.VirtualDevice.Update(deviceIndex, EquipmentState.Missing, -1);
				if (!VirtualDeviceManager.Instance.DisconnectDevice(deviceId)) {
					LogManager.Instance.Log(LogEntry.CreateWarning(this, "Failed to disconnect keyboard " + freetrackIndex));
					return;
				}
			});
			return true;
		}
		protected override bool DestroyInternal() {


			Container.Dispose();
			return true;
		}
		public VirtualDeviceSettings GetSettings() {
			var result = new VirtualDeviceSettings() {
				DeviceType = VirtualDeviceType.TrackingInterface,
				DeviceId = deviceIndex,
				ClassifiedDeviceIndex = freetrackIndex
			};

			if (!Initialized) {
				return result;
			}
			{ // send rate
				var txtRate = Container.GetWidget<Gtk.SpinButton>("txt_SendRate");
				result.SendRate = (int)txtRate.Adjustment.Value;
			}
			{ // send policy
				var cbbPolicy = Container.GetWidget<Gtk.ComboBoxText>("cbb_SendPolicy");
				int policyId = TextParseTools.ParseInt(cbbPolicy.ActiveId, 0);
				result.SendPolicy = (VirtualDeviceSendPolicy)policyId;
			}

			return result;
		}
		private void ApplySettingsToView() {
			if (!Initialized) {
				return;
			}
			{ // send rate
				var txtRate = Container.GetWidget<Gtk.SpinButton>("txt_SendRate");
				txtRate.Adjustment.Value = settings.SendRate;
			}
			{ // send policy
				var cbbPolicy = Container.GetWidget<Gtk.ComboBoxText>("cbb_SendPolicy");
				cbbPolicy.ActiveId = ((int)settings.SendPolicy).ToString();
			}
		}
		private void ApplySettingsToModel() {
			IVirtualDevice device = VirtualDevice.CreateFromSettings(settings);
			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = EquipmentMaster.Instance.VirtualDevice.GetEquipment(deviceIndex, -1);
				if (deviceId < 0) {
					LogManager.Instance.LogError(this, "Missing keyboard " + freetrackIndex);
					return;
				}
				if (!VirtualDeviceManager.Instance.ReconnectDevice(deviceId, device)) {
					LogManager.Instance.LogError(this, "Failed to apply settings to keyboard " + freetrackIndex);
					return;
				}
			});
		}
	}
}

