using System;

namespace Irseny.Tracap {
	public class Cap3PointOptions : CapTrackerOptions, IKeypointDetectorOptions, IPointLabelerOptions, IBasicPoseEstimatorOptions {
		/// <summary>
		/// Creates a new instance of this class with default values.
		/// </summary>
		public Cap3PointOptions() : base() {
			this.BrightnessThreshold = 32;
			this.MaxPointNo = 1024;
			this.MaxClusterNo = 8;
			this.MaxClusterMembers = 512;
			this.MinLayerEnergy = 6;
			this.MinClusterRadius = 2;
			this.MaxClusterRadius = 32;
			this.MarkClusters = true;

			this.LabelNo = 3;
			this.MissingLabelDistance = 800; // diagonal of 640x480
			this.FastApproximationThreshold = 200;
			this.ShowLabels = true;

			this.PointFrameLocationWeight = 0.04f;
		}
		/// <summary>
		/// Creates a copy of the given instance.
		/// </summary>
		/// <param name="source">Source.</param>
		public Cap3PointOptions(Cap3PointOptions source) : base(source) {
			this.BrightnessThreshold = source.BrightnessThreshold;
			this.MaxPointNo = source.MaxPointNo;
			this.MaxClusterNo = source.MaxClusterNo;
			this.MaxClusterMembers = source.MaxClusterMembers;
			this.MinLayerEnergy = source.MinLayerEnergy;
			this.MinClusterRadius = source.MinClusterRadius;
			this.MaxClusterRadius = source.MaxClusterRadius;
			this.MarkClusters = source.MarkClusters;

			this.LabelNo = source.LabelNo;
			this.MissingLabelDistance = source.MissingLabelDistance;
			this.FastApproximationThreshold = source.FastApproximationThreshold;
			this.ShowLabels = source.ShowLabels;

			this.PointFrameLocationWeight = source.PointFrameLocationWeight;
		}
		public int BrightnessThreshold { get; set; }
		public int MinClusterRadius { get; set; }
		public int MaxClusterRadius { get; set; }
		public int MinLayerEnergy { get; set; }
		public int MaxClusterMembers { get; set; }
		public int MaxPointNo { get; set; }
		public int MaxClusterNo { get; set; }
		public bool MarkClusters { get; set; }

		public int LabelNo { get; set; }
		public int MissingLabelDistance { get; set; }
		public int FastApproximationThreshold { get; set; }
		public bool ShowLabels { get; set; }

		public float PointFrameLocationWeight { get; set; }
	}
}
