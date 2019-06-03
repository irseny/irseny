using System;
using System.Collections.Generic;
using Irseny.Tracap;
using Irseny.Listing;
using Irseny.Log;

namespace Irseny.Inco.Device {
	public class CapInputTranslation {
		private struct Binding {
			CapAxis sourceAxis;
			VirtualDeviceCapability targetCapability;
			object targetPosKey;
			object targetNegKey;
			object mapping;

			public CapAxis Axis {
				get { return sourceAxis; }
			}
			public VirtualDeviceCapability Capability {
				get { return targetCapability; }
			}
			public object PosKey {
				get { return targetPosKey; }
			}
			public object NegKey {
				get { return targetNegKey; }
			}
			public object Mapping {
				get { return mapping; }
			}
			public Binding(CapAxis axis, VirtualDeviceCapability capability, object posKey, object negKey, object mapping) {
				this.sourceAxis = axis;
				this.targetCapability = capability;
				this.targetPosKey = posKey;
				this.targetNegKey = negKey;
				this.mapping = mapping;
			}
		}

		Dictionary<int, List<Binding>> config = new Dictionary<int, List<Binding>>();
		public CapInputTranslation() {
		}
		public void AddBinding(CapAxis axis, int deviceIndex, VirtualDeviceCapability capability, object posKey, object negKey, object mapping) {
			List<Binding> deviceBindings;
			if (!config.TryGetValue(deviceIndex, out deviceBindings)) {
				deviceBindings = new List<Binding>();
				config.Add(deviceIndex, deviceBindings);
			}
			var binding = new Binding(axis, capability, posKey, negKey, mapping);
			deviceBindings.Add(binding);
		}
		public bool RemoveBinding(CapAxis axis) {
			bool result = false;
			Queue<int> toRemove = new Queue<int>();
			foreach (var pair in config) {
				for (int i = 0; i < pair.Value.Count; i++) {
					if (pair.Value[i].Axis == axis) {
						pair.Value.RemoveAt(i);
						i -= 1;
						result = true;
					}
				}
				if (pair.Value.Count == 0) {
					toRemove.Enqueue(pair.Key);
				}
				foreach (int deviceIndex in toRemove) {
					config.Remove(deviceIndex);
				}
			}
			return result;
		}
		public void PositionChanged(object sender, PositionDetectedEventArgs args) {
			// TODO: protect with locks
			var configCopy = config;
			VirtualDeviceManager.Instance.Invoke(delegate {
				VirtualDeviceManager.Instance.BeginUpdate();
				foreach (var pair in configCopy) {
					int deviceIndex = pair.Key;
					int deviceId = EquipmentMaster.Instance.VirtualDevice.GetEquipment(deviceIndex, -1);
					if (deviceId < 0) {
						LogManager.Instance.Log(LogMessage.CreateWarning(this, "Missing device " + deviceIndex));
						continue;
					}
					IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(deviceId);
					if (device == null) {
						LogManager.Instance.Log(LogMessage.CreateWarning(this, "Missing device with id " + deviceId));
						continue;
					}
					device.BeginUpdate();
					foreach (Binding bind in pair.Value) {
						float state = args.Position.GetAxis(bind.Axis);
						// TODO: apply mapping
						if (!device.SetKeyState(bind.Capability, bind.PosKey, state)) {
							string message = string.Format("Cannot set device {0} key state {1}|{2}", deviceIndex, bind.PosKey, bind.NegKey);
							LogManager.Instance.Log(LogMessage.CreateWarning(this, message));
						}
						// TODO: set device axis

					}
					device.EndUpdate();
				}
				VirtualDeviceManager.Instance.EndUpdate();
			});
		}
	}

}
