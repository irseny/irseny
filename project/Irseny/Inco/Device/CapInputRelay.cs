using System;
using System.Collections.Generic;
using Irseny.Tracking;
using Irseny.Listing;
using Irseny.Log;

namespace Irseny.Inco.Device {
	public class CapInputRelay {
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
		public CapInputRelay() {
		}
		public CapInputRelay(CapInputRelay source) {
			if (source == null) throw new ArgumentNullException("source");
			foreach (var pair in source.config) {
				foreach (var binding in pair.Value) {
					AddBinding(binding.Axis, pair.Key, binding.Capability, binding.PosKey, binding.NegKey, binding.Mapping);
				}
			}
		}
		public bool Empty {
			get { return config.Count == 0; }
		}
		public int GetDeviceIndex(CapAxis axis) {
			foreach (var pair in config) {
				foreach (var binding in pair.Value) {
					if (binding.Axis == axis) {
						return pair.Key;
					}
				}
			}
			return -1;
		}
		public VirtualDeviceCapability GetDeviceCapability(CapAxis axis) {
			foreach (var lst in config.Values) {
				foreach (var binding in lst) {
					if (binding.Axis == axis) {
						return binding.Capability;
					}
				}
			}
			return VirtualDeviceCapability.Axis;
		}
		public Tuple<object, object> GetDeviceKeys(CapAxis axis) {
			foreach (var lst in config.Values) {
				foreach (var binding in lst) {
					if (binding.Axis == axis) {
						return Tuple.Create(binding.PosKey, binding.NegKey);
					}
				}
			}
			return null;
		}
		public object GetMapping(CapAxis axis) {
			foreach (var lst in config.Values) {
				foreach (var binding in lst) {
					if (binding.Axis == axis) {
						return binding.Mapping;
					}
				}
			}
			return null;
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
				// remove binding from all devices
				for (int i = 0; i < pair.Value.Count; i++) {
					if (pair.Value[i].Axis == axis) {
						pair.Value.RemoveAt(i);
						i -= 1;
						result = true;
					}
				}
				// remove empty device entries
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
					}
					device.EndUpdate();
				}
				VirtualDeviceManager.Instance.EndUpdate();
			});
		}

	}

}
