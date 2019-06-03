using System;
namespace Irseny.Inco.Device {
	public abstract class VirtualDevice : IVirtualDevice {
		int sendInterval = 16;
		bool stateChanged = true;
		long lastSend = 0;

		public VirtualDevice(int index) {
			DeviceIndex = index;
		}
		public int DeviceIndex { get; private set; }
		public abstract VirtualDeviceType DeviceType { get; }
		public VirtualDeviceSendPolicy SendPolicy { get; set; }
		public int SendInterval {
			get { return sendInterval; }
			set {
				if (value < 0) throw new ArgumentException();
				sendInterval = value;
			}
		}
		public bool SendRequired {
			get {
				switch (SendPolicy) {
				case VirtualDeviceSendPolicy.FixedRate:
					return true; // TODO: implement
				case VirtualDeviceSendPolicy.AfterModication:
					return stateChanged;
				case VirtualDeviceSendPolicy.Adaptive:
					if (stateChanged) {
						return true;
					}
					return true;
				default:
					return false;
				}
			}
		}
		public abstract VirtualDeviceCapability[] GetSupportedCapabilities();
		public abstract string[] GetKeyDescriptions(VirtualDeviceCapability capability);
		public abstract object[] GetKeyHandles(VirtualDeviceCapability capability);
		public abstract int GetKeyNo(VirtualDeviceCapability capability);
		public void BeginUpdate() {
			// nothing to do
		}
		public abstract bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, float state);
		public void EndUpdate() {
			// mark the new data as sendable
			stateChanged = true;
		}
		public virtual void Send() {
			// everything else managed in override
			// TODO: set last update time
			stateChanged = false;
		}


	}
}
