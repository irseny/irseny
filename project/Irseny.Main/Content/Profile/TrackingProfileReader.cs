﻿// This file is part of Irseny.
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
using System.Xml;
using System.Collections.Generic;
using Irseny.Core.Util;
using Irseny.Core.Tracking;

namespace Irseny.Main.Content.Profile {
	public class TrackingProfileReader {
		public TrackingProfileReader() {
		}
		public bool Read(SetupProfile profile, XmlNode root) {
			foreach (XmlNode node in root) {
				if (node.Name.Equals("Cap3Point")) {
					if (node.Attributes["Index"] == null) {
						return false;
					}
					int index = JsonString.ParseInt(node.Attributes["Index"].InnerText, -1);
					if (index < 0) {
						return false;
					}

					EquipmentSettings settings = ReadCap3Point(node);
					if (settings == null) {
						return false;
					}
					profile.AddTracker(index, settings);
				}
			}
			return true;
		}
		private EquipmentSettings ReadCap3Point(XmlNode root) {
			var result = new EquipmentSettings(typeof(TrackerProperty));
			int streamNo = 0;
			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("Stream")) {
					int stream = JsonString.ParseInt(node.InnerText, -1);
					if (stream < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.Stream0 + streamNo, stream);
					streamNo += 1;
				} else if (node.Name.Equals("Model")) {
					int model = JsonString.ParseInt(node.InnerText, -1);
					if (model < 0) {
						return null;
					}
					// TODO replace with another system
					//result.SetInteger(TrackerProperty.Model, model);

				} else if (node.Name.Equals("Smoothing")) {
					int smoothing = JsonString.ParseInt(node.InnerText, -1);
					if (smoothing < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.Smoothing, smoothing);
				} else if (node.Name.Equals("SmoothingDropoff")) {
					decimal dropoff = JsonString.ParseDecimal(node.InnerText, -1.0m);
					if (dropoff == -1.0m) {
						return null;
					}
					result.SetDecimal(TrackerProperty.SmoothingDropoff, dropoff);
				} else if (node.Name.Equals("MinBrightness")) {
					int brightness = JsonString.ParseInt(node.InnerText, -1);
					if (brightness < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MinBrightness, brightness);
				} else if (node.Name.Equals("MinClusterRadius")) {
					int radius = JsonString.ParseInt(node.InnerText, -1);
					if (radius < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MinClusterRadius, radius);
				} else if (node.Name.Equals("MaxClusterRadius")) {
					int radius = JsonString.ParseInt(node.InnerText, -1);
					if (radius < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxClusterRadius, radius);
				} else if (node.Name.Equals("MinLayerEnergy")) {
					int energy = JsonString.ParseInt(node.InnerText, -1);
					if (energy < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MinLayerEnergy, energy);
				} else if (node.Name.Equals("MaxClusterMembers")) {
					int members = JsonString.ParseInt(node.InnerText, -1);
					if (members < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxClusterMembers, members);
				} else if (node.Name.Equals("MaxClusters")) {
					int clusters = JsonString.ParseInt(node.InnerText, -1);
					if (clusters < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxClusterNo, clusters);
				} else if (node.Name.Equals("MaxPoints")) {
					int points = JsonString.ParseInt(node.InnerText, -1);
					if (points < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxPointNo, points);
				} else if (node.Name.Equals("Labels")) {
					int labelNo = JsonString.ParseInt(node.InnerText, -1);
					if (labelNo < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.LabelNo, labelNo);
				} else if (node.Name.Equals("FastApproxThreshold")) {
					int threshold = JsonString.ParseInt(node.InnerText, -1);
					if (threshold < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.FastApproxThreshold, threshold);
				} else if (node.Name.Equals("MaxQueuedImages")) {
					int images = JsonString.ParseInt(node.InnerText, -1);
					if (images < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxQueuedImages, images);
				}

			}
			return result;
		}
	}

}
