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
					int index = TextParseTools.ParseInt(node.Attributes["Index"].InnerText, -1);
					if (index < 0) {
						return false;
					}

					TrackerSettings settings = ReadCap3Point(node);
					if (settings == null) {
						return false;
					}
					profile.AddTracker(index, settings);
				}
			}
			return true;
		}
		private TrackerSettings ReadCap3Point(XmlNode root) {
			var result = new TrackerSettings();
			int streamNo = 0;
			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("Stream")) {
					int stream = TextParseTools.ParseInt(node.InnerText, -1);
					if (stream < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.Stream0 + streamNo, stream);
					streamNo += 1;
				} else if (node.Name.Equals("Model")) {
					int model = TextParseTools.ParseInt(node.InnerText, -1);
					if (model < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.Model, model);
				} else if (node.Name.Equals("Smoothing")) {
					int smoothing = TextParseTools.ParseInt(node.InnerText, -1);
					if (smoothing < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.Smoothing, smoothing);
				} else if (node.Name.Equals("SmoothingDropoff")) {
					double dropoff = TextParseTools.ParseDouble(node.InnerText, -1.0);
					if (dropoff == -1.0) {
						return null;
					}
					result.SetDecimal(TrackerProperty.SmoothingDropoff, dropoff);
				} else if (node.Name.Equals("MinBrightness")) {
					int brightness = TextParseTools.ParseInt(node.InnerText, -1);
					if (brightness < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MinBrightness, brightness);
				} else if (node.Name.Equals("MinClusterRadius")) {
					int radius = TextParseTools.ParseInt(node.InnerText, -1);
					if (radius < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MinClusterRadius, radius);
				} else if (node.Name.Equals("MaxClusterRadius")) {
					int radius = TextParseTools.ParseInt(node.InnerText, -1);
					if (radius < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxClusterRadius, radius);
				} else if (node.Name.Equals("MinLayerEnergy")) {
					int energy = TextParseTools.ParseInt(node.InnerText, -1);
					if (energy < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MinLayerEnergy, energy);
				} else if (node.Name.Equals("MaxClusterMembers")) {
					int members = TextParseTools.ParseInt(node.InnerText, -1);
					if (members < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxClusterMembers, members);
				} else if (node.Name.Equals("MaxClusters")) {
					int clusters = TextParseTools.ParseInt(node.InnerText, -1);
					if (clusters < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxClusterNo, clusters);
				} else if (node.Name.Equals("MaxPoints")) {
					int points = TextParseTools.ParseInt(node.InnerText, -1);
					if (points < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.MaxPointNo, points);
				} else if (node.Name.Equals("Labels")) {
					int labelNo = TextParseTools.ParseInt(node.InnerText, -1);
					if (labelNo < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.LabelNo, labelNo);
				} else if (node.Name.Equals("FastApproxThreshold")) {
					int threshold = TextParseTools.ParseInt(node.InnerText, -1);
					if (threshold < 0) {
						return null;
					}
					result.SetInteger(TrackerProperty.FastApproxThreshold, threshold);
				} else if (node.Name.Equals("MaxQueuedImages")) {
					int images = TextParseTools.ParseInt(node.InnerText, -1);
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
