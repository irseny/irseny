using System;

namespace Irseny {
	public interface IPointLabelerOptions {
		/// <summary>
		/// Gets the number of labels to assign.
		/// </summary>
		/// <value>The label number.</value>
		int LabelNo { get; }
		/// <summary>
		/// Gets the distance weight of unlabeled points.
		/// This should be more than the maximum point distance.
		/// </summary>
		/// <value>The weight of missing labels.</value>
		int MissingLabelDistance { get; }
		/// <summary>
		/// Gets the threshold used for determining whether the fast solution approximation is good enough.
		/// </summary>
		/// <value>The threshold for fast approximation grading.</value>
		int FastApproximationThreshold { get; }
		/// <summary>
		/// Gets a value indicating whether to visualize assigned labels.
		/// </summary>
		/// <value><c>true</c> if label visualization is enabled; otherwise, <c>false</c>.</value>
		bool ShowLabels { get; }
	}
}

