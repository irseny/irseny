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
