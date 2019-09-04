using System;
using System.Diagnostics;

namespace Irseny.Inco.Device {
	public abstract class VirtualDevice : IVirtualDevice {
		static Stopwatch sendWatch;

		static VirtualDevice() {
			sendWatch = new Stopwatch();
			sendWatch.Start();
		}

		int sendRate = 16;
		bool stateChanged = true;
		long lastSend = 0;

		public VirtualDevice(int index) {
			DeviceIndex = index;
		}
		public int DeviceIndex { get; private set; }
		public abstract VirtualDeviceType DeviceType { get; }
		public VirtualDeviceSendPolicy SendPolicy { get; set; }
		public int SendRate {
			get { return sendRate; }
			set {
				if (value < 0) throw new ArgumentException();
				sendRate = value;
			}
		}
		public bool SendRequired {
			get {
				switch (SendPolicy) {
				case VirtualDeviceSendPolicy.FixedRate:
					return (sendWatch.ElapsedMilliseconds - lastSend >= sendRate);
				case VirtualDeviceSendPolicy.AfterModification:
					return stateChanged;
				case VirtualDeviceSendPolicy.Adaptive:
					if (stateChanged) {
						return true;
					}
					return (sendWatch.ElapsedMilliseconds - lastSend >= sendRate);
				default:
					return false;
				}
			}
		}
		public abstract VirtualDeviceCapability[] GetSupportedCapabilities();
		public abstract object[] GetKeyHandles(VirtualDeviceCapability capability);
		public abstract int GetKeyNo(VirtualDeviceCapability capability);
		public virtual void BeginUpdate() {
			// nothing to do
		}
		public abstract bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, KeyState state);
		public abstract KeyState GetKeyState(VirtualDeviceCapability capability, object keyHandle);
		public abstract object[] GetModifiedKeys(VirtualDeviceCapability capability);
		public virtual void EndUpdate() {
			// mark the new data as sendable
			stateChanged = true;
		}
		public virtual void Send() {
			// everything else managed in override
			// TODO: set last update time
			stateChanged = false;
			lastSend = sendWatch.ElapsedMilliseconds;
		}
		public static VirtualDevice CreateFromSettings(VirtualDeviceSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			VirtualDevice result;
			switch (settings.DeviceType) {
			case VirtualDeviceType.Keyboard:
				result = new VirtualKeyboard(settings.DeviceId);
				break;
			case VirtualDeviceType.TrackingInterface:
				result = new FreetrackInterface(settings.DeviceId);
				break;
			default:
				throw new NotImplementedException();
			}
			result.SendPolicy = settings.SendPolicy;
			result.SendRate = settings.SendRate;
			return result;
		}

	}
}
