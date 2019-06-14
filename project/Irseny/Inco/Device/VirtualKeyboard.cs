using System;
using System.Collections.Generic;
namespace Irseny.Inco.Device {
	public class VirtualKeyboard : VirtualDevice {
		static readonly object[] keyHandles = new object[] {
			"Q", "W", "E", "R", "T", "Z", "U", "I", "O", "P"

		};
		static readonly string[] keyDescriptions = new string[] {
			"Q", "W", "E", "R", "T", "Z", "U", "I", "O", "P"

		};
		Dictionary<object, bool> keyState = new Dictionary<object, bool>(256);
		List<object> modifiedKeys = new List<object>(32);

		public VirtualKeyboard(int index) : base(index) {
			SendPolicy = VirtualDeviceSendPolicy.FixedRate;
			keyState = new Dictionary<object, bool>();
			foreach (object handle in keyHandles) {
				keyState.Add(handle, false);
			}
		}
		public override VirtualDeviceType DeviceType {
			get { return VirtualDeviceType.Keyboard; }
		}

		public override VirtualDeviceCapability[] GetSupportedCapabilities() {
			return new VirtualDeviceCapability[] { VirtualDeviceCapability.Key };
		}
		public override int GetKeyNo(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return keyHandles.Length;
			default:
				return 0;
			}
		}
		public override string[] GetKeyDescriptions(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return (string[])keyDescriptions.Clone();
			default:
				return new string[0];
			}
		}
		public override object[] GetKeyHandles(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return (object[])keyHandles.Clone();
			default:
				return new object[0];
			}
		}
		public override void BeginUpdate() {
			base.BeginUpdate();
			modifiedKeys.Clear();
		}
		public override bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, float state) {
			// TODO: implement
			switch (capability) {
			case VirtualDeviceCapability.Key:
				if (!keyState.ContainsKey(keyHandle)) {
					return false;
				}
				keyState[keyHandle] = (state > 0f);
				modifiedKeys.Add(keyHandle);
				return true;
			default:
				return false;
			}
		}
		public override float GetKeyState(VirtualDeviceCapability capability, object keyHandle) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				if (!keyState.ContainsKey(keyHandle)) {
					return 0f;
				}
				if (keyState[keyHandle]) {
					return 1f;
				} else {
					return 0f;
				}
			default:
				return 0f;
			}
		}
		public override object[] GetModifiedKeys(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return modifiedKeys.ToArray();
			default:
				return new object[0];
			}
		}
	}
}
