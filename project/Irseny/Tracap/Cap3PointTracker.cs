﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;
using Irseny.Util;

namespace Irseny.Tracap {
	public class Cap3PointTracker : SingleImageCapTracker {
		KeypointDetector pointDetector = null;
		PointLabeler pointLabeler = null;
		BasicPoseEstimator poseEstimator = null;
		SharedRef<Emgu.CV.Mat> imageOut = SharedRef.Create(new Emgu.CV.Mat());
		SharedRefCleaner imageCleaner = new SharedRefCleaner(32);

		public Cap3PointTracker() : base() {

		}
		public override bool Centered {
			get {
				if (poseEstimator == null) {
					return false;
				}
				return poseEstimator.Centered;
			}
		}
		public override bool Center() {
			if (poseEstimator == null) {
				return false;
			}
			return poseEstimator.Center();
		}
		public override bool Start(TrackerSettings settings) {
			if (!base.Start(settings)) {
				return false;
			}
			pointDetector = new KeypointDetector(settings);
			pointLabeler = new PointLabeler(settings);
			poseEstimator = new BasicPoseEstimator(settings);
			return true;
		}

		public override bool Stop() {
			if (!base.Stop()) {
				return false;
			}
			imageCleaner.CleanUpAll(); // might leave some images left
			return true;
		}
		public override void Dispose() {
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
