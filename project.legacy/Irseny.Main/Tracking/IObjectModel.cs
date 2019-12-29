using System;

namespace Irseny.Tracking {
	public interface IObjectModel {
		int PointNo { get; }
		Tuple<int, int, int> GetPoint(int index);
	}
}
