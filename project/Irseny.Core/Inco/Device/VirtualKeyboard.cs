using System;
using System.Collections.Generic;
namespace Irseny.Core.Inco.Device {
	public class VirtualKeyboard : VirtualDevice {
		static readonly object[] keyHandles = new object[] {
			"Q", "W", "E", "R", "T", "Z", "U", "I", "O", "P"
		};
		Dictionary<string, KeyState> keyState = new Dictionary<string, KeyState>(256);
		List<object> modifiedKeys = new List<object>(32);

		public VirtualKeyboard(int index) : base(index) {
			SendPolicy = VirtualDeviceSendPolicy.FixedRate;
			foreach (object handle in keyHandles) {
				keyState.Add(handle.ToString(), new KeyState(false, false));
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
			// TODO: determine whether clearing here produces correct results
			// TODO: maybe move to Send()
			modifiedKeys.Clear();
		}
		public override bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, KeyState state) {
			// TODO: implement
			string keyName = keyHandle.ToString();
			switch (capability) {
			case VirtualDeviceCapability.Key:
				if (!keyState.ContainsKey(keyName)) {
					return false;
				}
				keyState[keyName] = state;
				modifiedKeys.Add(keyHandle);
				return true;
			default:
				return false;
			}
		}
		public override KeyState GetKeyState(VirtualDeviceCapability capability, object keyHandle) {
			string keyName = keyHandle.ToString();
			switch (capability) {
			case VirtualDeviceCapability.Key:
				KeyState result;
				if (!keyState.TryGetValue(keyName, out result)) {
					return result;
				}
				return new KeyState(false, false);
			default:
				return new KeyState(false, false);
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
