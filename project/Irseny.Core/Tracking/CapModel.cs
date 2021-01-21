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

namespace Irseny.Core.Tracking {
	public class CapModel : IObjectModel {

		public int VisorHeight { get; set; }
		public int VisorLength { get; set; }
		public int VisorWidth { get; set; }

		public CapModel() {
			VisorHeight = 8;
			VisorLength = 10;
			VisorWidth = 12;
		}
		public int PointNo {
			get { return 3; }
		}
		public Tuple<int, int, int> GetPoint(int index) {
			switch(index) {
			case 0:
				return Tuple.Create(0, 0, 0);
			case 1:
				return Tuple.Create(-VisorWidth/2, VisorHeight, VisorLength);
			case 2:
				return Tuple.Create(VisorWidth/2, VisorHeight, VisorLength);
			default:
				throw new ArgumentOutOfRangeException("index");
			}
		}
	}
}
