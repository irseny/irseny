using System;

namespace Irseny.Tracap {
	public interface IKeypointDetectorOptions : ICapTrackerOptions {
		/// <summary>
		/// Gets the brightness for point-void classification.
		/// </summary>
		/// <value>The brightness threshold.</value>
		int BrightnessThreshold { get; }
		/// <summary>
		/// Gets the maximum number of points to detect.
		/// </summary>
		/// <value>The max point no.</value>
		int MaxPointNo { get; }
		/// <summary>
		/// Gets the maximum number of clusters to detect.
		/// </summary>
		/// <value>The maximum cluster number.</value>
		int MaxClusterNo { get; }
		/// <summary>
		/// Gets the maximum number of points a cluster may have.
		/// </summary>
		/// <value>The maximum cluster point number.</value>
		int MaxClusterMembers { get; }
		/// <summary>
		/// Gets the minimum energy a cluster must have.
		/// </summary>
		/// <value>The minimum cluster energy.</value>
		//int MinClusterEnergy { get; }
		/// <summary>
		/// Gets the minimum radius a cluster must have.
		/// </summary>
		/// <value>The minimum cluster radius.</value>
		int MinClusterRadius { get; }
		/// <summary>
		/// Gets the maximum radius a cluster is allowed to have.
		/// </summary>
		/// <value>The maximum cluster radius.</value>
		int MaxClusterRadius { get; }
		/// <summary>
		/// Gets the minimum energy a cluster layer must have.
		/// </summary>
		/// <value>The minimum layer energy.</value>
		int MinLayerEnergy { get; }
		/// <summary>
		/// Gets a value indicating whether to visually mark cluster locations.
		/// </summary>
		/// <value><c>true</c> if cluster marking is enabled; otherwise, <c>false</c>.</value>
		bool MarkClusters { get; }
	}
}

