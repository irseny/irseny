using System;
using System.Xml;
using System.Collections.Generic;
using Irseny.Tracap;

namespace Irseny.Content.Profile {
	public class TrackingProfileWriter {
		public TrackingProfileWriter() {
		}
		public XmlNode Write(SetupProfile profile, XmlDocument target) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (target == null) throw new ArgumentNullException("target");
			var result = target.CreateElement("Tracking");
			foreach (int i in profile.TrackerIndexes) {
				ICapTrackerOptions options = profile.GetTracker(i);
				XmlNode node = null;
				if (options is Cap3PointOptions) {
					node = WriteCap3Point(i, (Cap3PointOptions)options, target);
				} else {
					return null;
				}
				result.AppendChild(node);

			}
			return result;
		}
		private XmlNode WriteCap3Point(int index, Cap3PointOptions options, XmlDocument target) {
			XmlElement result = target.CreateElement("Cap3Point");
			result.SetAttribute("Index", index.ToString());
			for (int i = 0; i < options.StreamNo; i++) {
				XmlElement node = target.CreateElement("Stream");
				result.AppendChild(node);
				node.InnerText = options.GetStreamId(i).ToString();
			}
			{ // smoothing
				XmlElement node = target.CreateElement("Smoothing");
				result.AppendChild(node);
				node.InnerText = options.Smoothing.ToString();
			}
			{ // brightness
				XmlElement node = target.CreateElement("BrightnessThreshold");
				result.AppendChild(node);
				node.InnerText = options.BrightnessThreshold.ToString();
			}
			{ // cluster radius
				XmlElement node = target.CreateElement("MinClusterRadius");
				result.AppendChild(node);
				node.InnerText = options.MinClusterRadius.ToString();
				node = target.CreateElement("MaxClusterRadius");
				result.AppendChild(node);
				node.InnerText = options.MaxClusterRadius.ToString();
			}
			{ // cluster members
				XmlElement node = target.CreateElement("MaxClusterMembers");
				result.AppendChild(node);
				node.InnerText = options.MaxClusterMembers.ToString();
			}

			{ // cluster no
				XmlElement node = target.CreateElement("MaxClusterNo");
				result.AppendChild(node);
				node.InnerText = options.MaxClusterNo.ToString();
			}
			{ // point no
				XmlElement node = target.CreateElement("MaxPointNo");
				result.AppendChild(node);
				node.InnerText = options.MaxPointNo.ToString();
			}
			{ // layer energy
				XmlElement node = target.CreateElement("MinLayerEnergy");
				result.AppendChild(node);
				node.InnerText = options.MinLayerEnergy.ToString();
			}
			{ // fast approx threshold
				XmlElement node = target.CreateElement("FastApproximationThreshold");
				result.AppendChild(node);
				node.InnerText = options.FastApproximationThreshold.ToString();
			}
			return result;
		}
	}
}
