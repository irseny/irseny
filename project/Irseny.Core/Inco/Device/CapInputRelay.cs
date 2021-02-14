// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Irseny.Core.Tracking;
using Irseny.Core.Tracking.HeadTracking;
using Irseny.Core.Listing;
using Irseny.Core.Log;

namespace Irseny.Core.Inco.Device {
	public class CapInputRelay {
		private struct Binding {
			HeadAxis sourceAxis;
			VirtualDeviceCapability targetCapability;
			object targetPosKey;
			object targetNegKey;
			object mapping;

			public HeadAxis Axis {
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
			public Binding(HeadAxis axis, VirtualDeviceCapability capability, object posKey, object negKey, object mapping) {
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
		public int GetDeviceIndex(HeadAxis axis) {
			foreach (var pair in config) {
				foreach (var binding in pair.Value) {
					if (binding.Axis == axis) {
						return pair.Key;
					}
				}
			}
			return -1;
		}
		public VirtualDeviceCapability GetDeviceCapability(HeadAxis axis) {
			foreach (var lst in config.Values) {
				foreach (var binding in lst) {
					if (binding.Axis == axis) {
						return binding.Capability;
					}
				}
			}
			return VirtualDeviceCapability.Axis;
		}
		public Tuple<object, object> GetDeviceKeys(HeadAxis axis) {
			foreach (var lst in config.Values) {
				foreach (var binding in lst) {
					if (binding.Axis == axis) {
						return Tuple.Create(binding.PosKey, binding.NegKey);
					}
				}
			}
			return null;
		}
		public object GetMapping(HeadAxis axis) {
			foreach (var lst in config.Values) {
				foreach (var binding in lst) {
					if (binding.Axis == axis) {
						return binding.Mapping;
					}
				}
			}
			return null;
		}
		public void AddBinding(HeadAxis axis, int deviceIndex, VirtualDeviceCapability capability, object posKey, object negKey, object mapping) {
			List<Binding> deviceBindings;
			if (!config.TryGetValue(deviceIndex, out deviceBindings)) {
				deviceBindings = new List<Binding>();
				config.Add(deviceIndex, deviceBindings);
			}
			var binding = new Binding(axis, capability, posKey, negKey, mapping);
			deviceBindings.Add(binding);
		}
		public bool RemoveBinding(HeadAxis axis) {
			// remove the axis from all devices
			bool result = false;
			var removeFrom = new Queue<List<Binding>>(config.Values);
			while (removeFrom.Count > 0) {
				List<Binding> bindings = removeFrom.Dequeue();
				for (int iBind = 0; iBind < bindings.Count; iBind++) {
					if (bindings[iBind].Axis == axis) {
						bindings.RemoveAt(iBind);
						iBind -= 1;
						result = true;
					}
				}
			}
			// cleanup, remove empty device config
			var toRemove = new Queue<int>();
			foreach (var pair in config) {
				if (pair.Value.Count == 0) {
					toRemove.Enqueue(pair.Key);
				}
			}
			while (toRemove.Count > 0) {
				config.Remove(toRemove.Dequeue());
			}
			return result;
		}
		public void PositionChanged(object sender, PositionDetectedEventArgs args) {
			// TODO: protect with locks
			var configCopy = config;
			VirtualDeviceManager.Instance.Invoke(delegate {
				VirtualDeviceManager.Instance.BeginUpdate();
				var toRemove = new Queue<int>();
				foreach (var pair in configCopy) {
					List<Binding> bindings = pair.Value;
					int deviceIndex = pair.Key;
					int deviceId = EquipmentMaster.Instance.VirtualDevice.GetEquipment(deviceIndex, -1);
					if (deviceId < 0) {
						LogManager.Instance.Log(LogEntry.CreateWarning(this, "Missing device " + deviceIndex));
						continue;
					}
					IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(deviceId);
					if (device == null) {
						LogManager.Instance.Log(LogEntry.CreateWarning(this, "Missing device with id " + deviceId));
						continue;
					}
					device.BeginUpdate();
					foreach (Binding bind in pair.Value) {
						KeyState state = args.Position.GetAxis(bind.Axis);
						// TODO: apply mapping
						if (!device.SetKeyState(bind.Capability, bind.PosKey, state)) {
							string message = string.Format("Cannot set device {0} key state {1}|{2}", deviceIndex, bind.PosKey, bind.NegKey);
							LogManager.Instance.Log(LogEntry.CreateWarning(this, message));
						}
					}
					device.EndUpdate();
				}
				VirtualDeviceManager.Instance.EndUpdate();
			});
		}

	}

}
