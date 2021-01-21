using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;
using Irseny.Core.Util;
using Irseny.Core.Listing;

namespace Irseny.Core.Tracking {
	public class Cap3PointTracker : SingleImageCapTracker {
		KeypointDetector pointDetector;
		PointLabeler pointLabeler;
		P3PoseEstimator poseEstimator;
		EquipmentSettings settings;
		SharedRef<Emgu.CV.Mat> imageOut;
		SharedRefCleaner imageCleaner;
		readonly object trackerSync;


		public override bool Running {
			get { 
				lock (trackerSync) {
					return pointDetector != null && poseEstimator != null && pointLabeler != null; 
				}
			}
		}
		public override EquipmentSettings GetSettings() {
			lock (trackerSync) {
				settings.SetInteger(TrackerProperty.Tracking, Running ? 1 : 0);
				return new EquipmentSettings(settings);
			}
		}
		public Cap3PointTracker(EquipmentSettings settings) : base() {
			if (settings == null) throw new ArgumentNullException("settings");
			this.settings = new EquipmentSettings(settings);
			this.pointDetector = null;
			this.pointLabeler = null;
			this.poseEstimator = null;

			this.imageOut = SharedRef.Create(new Emgu.CV.Mat());
			this.imageCleaner = new SharedRefCleaner(32);
			this.trackerSync = new object();
		}

		public override bool Center() {
			lock (trackerSync) {
				if (poseEstimator == null) {
					return false;
				}
				return poseEstimator.Center();
			}
		}
		public override bool ApplySettings(EquipmentSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			lock (trackerSync) {
				this.settings = new EquipmentSettings(settings);
				if (Running) {
					var model = new CapModel();
					pointDetector = new KeypointDetector(this.settings);
					pointLabeler = new PointLabeler(this.settings);
					poseEstimator = new P3PoseEstimator(this.settings, model);
				}
			}
			return true;
		}
		public override bool Start() {
			var model = new CapModel();
			pointDetector = new KeypointDetector(settings);
			pointLabeler = new PointLabeler(settings);
			poseEstimator = new P3PoseEstimator(settings, model);
			return true;
		}

		public override bool Stop() {
			imageCleaner.CleanUpAll(); // might leave some images left
			pointDetector = null;
			pointLabeler = null;
			poseEstimator = null;
			return true;
		}
		public override void Dispose() {
			Stop();
			imageCleaner.DisposeAll(); // should not matter if some images are disposed on non detection threads
			base.Dispose();
		}
		protected override bool Step(SharedRef<Emgu.CV.Mat> imageIn) {
			if (!Running) {
				return false;
			}
			SetupStep(imageIn);
			// keypoint detection
			Point2i[] keypoints;
			int keypointNo = pointDetector.Detect(imageIn.Reference, imageOut.Reference, out keypoints);
			// keypoint labeling
			int[] labels;
			int labelNo = pointLabeler.Label(keypoints, keypointNo, imageOut.Reference, out labels);
			// TODO: pose detection
			var position = poseEstimator.Estimate(keypoints, labels, labelNo);
			// spread results
			OnInputProcessed(new ImageProcessedEventArgs(imageOut));
			OnPositionDetected(new PositionDetectedEventArgs(position));
			return true;
		}
		private void SetupStep(SharedRef<Emgu.CV.Mat> imageIn) {
			Emgu.CV.Mat imgIn = imageIn.Reference;
			Emgu.CV.Mat imgOut = imageOut.Reference;
			if (imgIn.Width != imgOut.Width || imgIn.Height != imgOut.Height) {
				imageCleaner.AddReference(imageOut);
				imageCleaner.CleanUpAll();
				imageOut = SharedRef.Create(new Emgu.CV.Mat(imgIn.Height, imgIn.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1));
			}
		}
	}


}
