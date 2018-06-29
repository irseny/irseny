using System;

namespace Irseny.Listing {
	public class EquipmentMaster {
		static object instanceSync = new object();
		static EquipmentMaster instance = null;

		public EquipmentMaster() {
			VideoCaptureStream = new EquipmentManager<int>();
			VideoSource = new EquipmentManager<int>();
		}
		public static EquipmentMaster Instance {
			get {
				lock (instanceSync) {
					return instance;
				}
			}
		}
		public EquipmentManager<int> VideoCaptureStream { get; private set; }
		public EquipmentManager<int> VideoSource { get; private set; }
		public static void MakeInstance(EquipmentMaster instance) {			
			lock (instanceSync) {
				EquipmentMaster.instance = instance;
			}
		}
	}
}

