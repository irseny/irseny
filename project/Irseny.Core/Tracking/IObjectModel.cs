using System;

namespace Irseny.Core.Tracking {
	public interface IObjectModel {
		int PointNo { get; }
		Tuple<int, int, int> GetPoint(int index);
	}
}
