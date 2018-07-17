using System;

namespace Irseny.Tracap {
	public class Basic3PointOptions : CapTrackerOptions, IKeypointDetectorOptions, IPointLabelerOptions, IBasicPoseEstimatorOptions {
		/// <summary>
		/// Creates a new instance of this class with default values.
		/// </summary>
		public Basic3PointOptions() : base() {
			this.BrightnessThreshold = 32;
			this.MaxPointNo = 1024;
			this.MaxClusterNo = 8;
			this.MaxClusterPointNo = 512;
			this.MinLayerEnergy = 6;
			this.MinClusterRadius = 2;
			this.MaxClusterRadius = 32;
			this.MarkClusters = true;

			this.LabelNo = 3;
			this.MissingLabelDistance = 800; // diagonal of 640x480
			this.FastApproximationThreshold = 200;
			this.ShowLabels = true;


		}
		/// <summary>
		/// Creates a copy of the given instance.
		/// </summary>
		/// <param name="source">Source.</param>
		public Basic3PointOptions(Basic3PointOptions source) : base(source) {
			this.BrightnessThreshold = source.BrightnessThreshold;
			/*this.MaxClusterGap = source.MaxClusterGap;
			this.MinClusterEnergy = source.MinClusterEnergy;
			this.VoidEnergy = source.VoidEnergy;
			this.PointEnergy = source.PointEnergy;
			this.MinLineWidth = source.MinLineWidth;*/
			this.MaxPointNo = source.MaxPointNo;
			this.MaxClusterNo = source.MaxClusterNo;
			this.MaxClusterPointNo = source.MaxClusterPointNo;
			this.MinLayerEnergy = source.MinLayerEnergy;
			this.MinClusterRadius = source.MinClusterRadius;
			this.MaxClusterRadius = source.MaxClusterRadius;
			this.MarkClusters = source.MarkClusters;

			this.LabelNo = source.LabelNo;
			this.MissingLabelDistance = source.MissingLabelDistance;
			this.FastApproximationThreshold = source.FastApproximationThreshold;
			this.ShowLabels = source.ShowLabels;
		}

		/// <summary>
		/// Gets or sets the maximum allowed horizontal and vertical gap between points in a cluster
		/// </summary>
		/// <value>The maximum cluster point gap.</value>
		//public int MaxClusterGap { get; set; }
		/// <summary>
		/// Gets or sets the minimum energy a cluster must satisfy.
		/// </summary>
		/// <value>The max cluster strength.</value>
		//public int MinClusterEnergy { get; set; }
		/// <summary>
		/// Gets or sets the cluster strength reduction when a point fails the threshold test.
		/// </summary>
		/// <value>The void strength.</value>
		//public int VoidEnergy { get; set; }
		/// <summary>
		/// Gets or sets the cluster strength increase when a point passes the threshold test.
		/// </summary>
		/// <value>The point strength.</value>
		//public int PointEnergy { get; set; }
		/// <summary>
		/// Gets or sets the minimum width a detected line must satisfy.
		/// </summary>
		/// <value>The minimum width of the line.</value>
		//public int MinLineWidth { get; set; }
		public int BrightnessThreshold { get; set; }
		public int MinClusterRadius { get; set; }
		public int MaxClusterRadius { get; set; }
		public int MinLayerEnergy { get; set; }
		public int MaxClusterPointNo { get; set; }
		public int MaxPointNo { get; set; }
		public int MaxClusterNo { get; set; }
		public bool MarkClusters { get; set; }

		public int LabelNo { get; set; }
		public int MissingLabelDistance { get; set; }
		public int FastApproximationThreshold { get; set; }
		public bool ShowLabels { get; set; }
	}
}
