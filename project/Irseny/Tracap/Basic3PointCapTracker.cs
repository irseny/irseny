﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;

namespace Irseny.Tracap {
	public class Basic3PointCapTracker : SingleImageCapTracker {
		Basic3PointOptions options;
		KeypointDetector pointDetector;
		PointLabeler pointLabeler;
		BasicPoseEstimator poseEstimator;
		Util.SharedRef<Emgu.CV.Mat> imageOut = Util.SharedRef.Create(new Emgu.CV.Mat());
		Util.SharedRefCleaner imageCleaner = new Util.SharedRefCleaner(32);

		public Basic3PointCapTracker(Basic3PointOptions options) : base(options) {
			this.options = new Basic3PointOptions(options);
			this.pointDetector = new KeypointDetector(this.options);
			this.pointLabeler = new PointLabeler(this.options);
			this.poseEstimator = new BasicPoseEstimator(this.options);
		}

		public override bool Start() {
			Running = true;
			pointDetector = new KeypointDetector(options);
			pointLabeler = new PointLabeler(options);
			poseEstimator = new BasicPoseEstimator(options);
			return true;
		}

		public override bool Stop() {
			Running = false;
			imageCleaner.CleanUpAll(); // might leave some images left
			return true;
		}
		public override void Dispose() {
			imageCleaner.DisposeAll(); // should not matter if some images are disposed on non detection threads
			base.Dispose();
		}
		protected override bool Step(Util.SharedRef<Emgu.CV.Mat> imageIn) {			
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
		private void SetupStep(Util.SharedRef<Emgu.CV.Mat> imageIn) {
			Emgu.CV.Mat imgIn = imageIn.Reference;
			Emgu.CV.Mat imgOut = imageOut.Reference;
			if (imgIn.Width != imgOut.Width || imgIn.Height != imgOut.Height) {
				imageCleaner.AddReference(imageOut);
				imageCleaner.CleanUpAll(); 
				imageOut = Util.SharedRef.Create(new Emgu.CV.Mat(imgIn.Height, imgIn.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1));
			}
		}

	}


}
