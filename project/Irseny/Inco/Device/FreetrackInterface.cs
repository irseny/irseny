using System;
using System.Collections.Generic;

namespace Irseny.Inco.Device {
	class FreetrackInterface : VirtualDevice {
		Dictionary<string, float> keyState = new Dictionary<string, float>(16);
		object[] keyHandles = new object[] {
			"X", "Y", "Z", "Yaw", "Pitch", "Roll"
		};
		List<object> modifiedKeys = new List<object>();
		public FreetrackInterface(int index) : base(index) {
			SendPolicy = VirtualDeviceSendPolicy.FixedRate;
			foreach (object handle in keyHandles) {
				keyState.Add(handle.ToString(), 0.0f);
			}
		}
		public override VirtualDeviceType DeviceType {
			get { return VirtualDeviceType.TrackingInterface; }
		}
		public override VirtualDeviceCapability[] GetSupportedCapabilities() {
			return new VirtualDeviceCapability[] {
				VirtualDeviceCapability.Axis
			};
		}
		public override object[] GetKeyHandles(VirtualDeviceCapability capability) {
			return (object[])keyHandles.Clone();
		}
		public override int GetKeyNo(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				return keyHandles.Length;
			default:
				return 0;
			}
		}
		public override float GetKeyState(VirtualDeviceCapability capability, object keyHandle) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				float result;
				if (keyState.TryGetValue(keyHandle.ToString(), out result)) {
					return result;
				}
				return 0.0f;
			default:
				return 0.0f;
			}
		}
		public override bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, float state) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				if (!keyState.ContainsKey(keyHandle.ToString())) {
					return false;
				}
				keyState[keyHandle.ToString()] = state;
				modifiedKeys.Add(keyHandle);
				return true;
			default:
				return false;
			}
		}
		public override object[] GetModifiedKeys(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Axis:
				return modifiedKeys.ToArray();
			default:
				return new object[0];
			}
		}
		public override void BeginUpdate() {
			base.BeginUpdate();
			modifiedKeys.Clear();
		}
	}
}
