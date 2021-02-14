// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Size2i = System.Drawing.Size;
using Point2i = System.Drawing.Point;
using Point2f = System.Drawing.PointF;
using Irseny.Core.Util;
using Irseny.Core.Listing;
using Irseny.Core.Shared;

namespace Irseny.Core.Tracking {
	public class Cap3PointTracker : SingleImageCapTracker {
		KeypointDetector pointDetector;
		PointLabeler pointLabeler;
		P3PoseEstimator poseEstimator;
		EquipmentSettings settings;
		SharedRef<IRasterImageBase> imageOut;
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
			IRasterImageBase frame = new TrackerVideoFrame(1, 1, new byte[1]);
			this.imageOut = SharedRef.Create(frame);
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
		protected override bool Step(SharedRef<IRasterImageBase> imageIn) {
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
		private void SetupStep(SharedRef<IRasterImageBase> imageIn) {
			IRasterImageBase imgIn = imageIn.Reference;
			IRasterImageBase imgOut = imageOut.Reference;
			if (imgIn.Width != imgOut.Width || imgIn.Height != imgOut.Height) {
				imageCleaner.AddReference(imageOut);
				imageCleaner.CleanUpAll();
				IRasterImageBase frame = new TrackerVideoFrame(imgIn.Height, imgIn.Width, new byte[imgIn.Height*imgIn.Width]);
				imageOut = SharedRef.Create(frame);
			}
		}
	}


}
