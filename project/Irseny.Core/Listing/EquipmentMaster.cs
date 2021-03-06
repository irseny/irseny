﻿// This file is part of Irseny.
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

namespace Irseny.Core.Listing {
	public class EquipmentMaster {
		static EquipmentMaster instance = null;

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

		public EquipmentManager<int> HeadModel { get; private set; }
		/// <summary>
		/// Gets the output device manager.
		/// </summary>
		/// <value>The output device manager.</value>
		public EquipmentManager<int> VirtualDevice { get; private set; }
		/// <summary>
		/// Gets the input device manager.
		/// </summary>
		/// <value>The input device manager.</value>
		public EquipmentManager<int> InputDevice { get; private set; }
		/// <summary>
		/// Gets the surface property manager
		/// </summary>
		/// <value>The surface property manager.</value>
		public EquipmentManager<int> Surface { get; private set; }



		public EquipmentMaster() {
			VideoCaptureStream = new EquipmentManager<int>();
			//VideoSource = new EquipmentManager<int>();
			HeadTracker = new EquipmentManager<int>();
			HeadModel = new EquipmentManager<int>();
			VirtualDevice = new EquipmentManager<int>();
			InputDevice = new EquipmentManager<int>();
			Surface = new EquipmentManager<int>();

		}
		public static EquipmentMaster Instance {
			get {
				return instance;
			}
		}
		/// <summary>
		/// Makes the given instance current.
		/// </summary>
		/// <param name="instance">Instance to make current.</param>
		public static void MakeInstance(EquipmentMaster instance) {
			EquipmentMaster.instance = instance;
		}
	}
}

