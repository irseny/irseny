using System;

namespace Irseny.Listing {
	public class EquipmentMaster {
		static object instanceSync = new object();
		static EquipmentMaster instance = null;

		public EquipmentMaster() {
			VideoCaptureStream = new EquipmentManager<int>();
			//VideoSource = new EquipmentManager<int>();
			HeadTracker = new EquipmentManager<int>();
			OutputDevice = new EquipmentManager<int>();
			InputDevice = new EquipmentManager<int>();
		}
		public static EquipmentMaster Instance {
			get {
				lock (instanceSync) {
					return instance;
				}
			}
		}
		/// <summary>
		/// Gets the video capture stream manager.
		/// </summary>
		/// <value>The video capture stream manager.</value>
		public EquipmentManager<int> VideoCaptureStream { get; private set; }
		//public EquipmentManager<int> VideoSource { get; private set; }
		/// <summary>
		/// Gets the pose tracker manager.
		/// </summary>
		/// <value>The head tracker manager.</value>
		public EquipmentManager<int> HeadTracker { get; private set; }
		/// <summary>
		/// Gets the output device manager.
		/// </summary>
		/// <value>The output device manager.</value>
		public EquipmentManager<int> OutputDevice { get; private set; }
		/// <summary>
		/// Gets the input device manager.
		/// </summary>
		/// <value>The input device manager.</value>
		public EquipmentManager<int> InputDevice { get; private set; }
		/// <summary>
		/// Makes the given instance current.
		/// </summary>
		/// <param name="instance">Instance to make current.</param>
		public static void MakeInstance(EquipmentMaster instance) {
			lock (instanceSync) {
				EquipmentMaster.instance = instance;
			}
		}
	}
}

