using System;
namespace Irseny.Tracap {
	public class Basic3PointOptions : CapTrackerOptions {
		/// <summary>
		/// Creates a new instance of this class with default values.
		/// </summary>
		public Basic3PointOptions() : base() {
			this.MaxClusterGap = 2;
			this.MinClusterEnergy = 16;
			this.VoidEnergy = 0;
			this.PointEnergy = 0;
			this.MinLineWidth = 4;
			this.MinClusterRadius = 4;
			this.MaxClusterRadius = 64;
			this.MinStrideEnergy = 4;
			this.MaxClusterPoints = 256;

		}
		/// <summary>
		/// Creates a copy of the given instance.
		/// </summary>
		/// <param name="source">Source.</param>
		public Basic3PointOptions(Basic3PointOptions source) : base(source) {
			this.MaxClusterGap = source.MaxClusterGap;
			this.MinClusterEnergy = source.MinClusterEnergy;
			this.VoidEnergy = source.VoidEnergy;
			this.PointEnergy = source.PointEnergy;
			this.MinLineWidth = source.MinLineWidth;
			this.MinClusterRadius = source.MinClusterRadius;
			this.MaxClusterRadius = source.MaxClusterRadius;
			this.MinStrideEnergy = source.MinStrideEnergy;
			this.MaxClusterPoints = source.MaxClusterPoints;
		}
		/// <summary>
		/// Gets or sets the maximum allowed horizontal and vertical gap between points in a cluster
		/// </summary>
		/// <value>The maximum cluster point gap.</value>
		public int MaxClusterGap { get; set; }
		/// <summary>
		/// Gets or sets the minimum energy a cluster must satisfy.
		/// </summary>
		/// <value>The max cluster strength.</value>
		public int MinClusterEnergy { get; set; }
		/// <summary>
		/// Gets or sets the cluster strength reduction when a point fails the threshold test.
		/// </summary>
		/// <value>The void strength.</value>
		public int VoidEnergy { get; set; }
		/// <summary>
		/// Gets or sets the cluster strength increase when a point passes the threshold test.
		/// </summary>
		/// <value>The point strength.</value>
		public int PointEnergy { get; set; }
		/// <summary>
		/// Gets or sets the minimum width a detected line must satisfy.
		/// </summary>
		/// <value>The minimum width of the line.</value>
		public int MinLineWidth { get; set; }
		/// <summary>
		/// Gets or sets the minimum radius a cluster must have to pass the cluster size test.
		/// </summary>
		/// <value>The minimum cluster radius.</value>
		public int MinClusterRadius { get; set; }
		/// <summary>
		/// Gets or sets the maximum radius a cluster may have to pass the cluster size test.
		/// </summary>
		/// <value>The max cluster radius.</value>
		public int MaxClusterRadius { get; set; }
		/// <summary>
		/// Gets or sets the minimum energy a cluster line must have
		/// </summary>
		/// <value>The minimum stride energy.</value>
		public int MinStrideEnergy { get; set; }
		/// <summary>
		/// Gets or sets the maximum number of points to associate with a cluster.
		/// </summary>
		/// <value>The max cluster points.</value>
		public int MaxClusterPoints { get; set; }

	}
}
