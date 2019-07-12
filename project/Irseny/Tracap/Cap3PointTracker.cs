﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;
using Irseny.Util;

namespace Irseny.Tracap {
	public class Cap3PointTracker : SingleImageCapTracker {
		TrackerSettings settings;
		KeypointDetector pointDetector;
		PointLabeler pointLabeler;
		BasicPoseEstimator poseEstimator;
		SharedRef<Emgu.CV.Mat> imageOut = SharedRef.Create(new Emgu.CV.Mat());
		SharedRefCleaner imageCleaner = new SharedRefCleaner(32);

		public Cap3PointTracker(TrackerSettings settings) : base(settings) {
			this.settings = new TrackerSettings(settings);
			this.pointDetector = new KeypointDetector(this.settings);
			this.pointLabeler = new PointLabeler(this.settings);
			this.poseEstimator = new BasicPoseEstimator(this.settings);
		}
		public override bool Centered {
			get {
				return poseEstimator.Centered;
			}
		}
		public override bool Center() {
			return poseEstimator.Center();
		}
		public override bool Start() {
			Running = true;
			pointDetector = new KeypointDetector(settings);
			pointLabeler = new PointLabeler(settings);
			poseEstimator = new BasicPoseEstimator(settings);
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
		protected override bool Step(SharedRef<Emgu.CV.Mat> imageIn) {
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