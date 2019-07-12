using System;
using System.Xml;
using System.Collections.Generic;
using Irseny.Util;
using Irseny.Tracap;

namespace Irseny.Content.Profile {
	public class TrackerProfileReader {
		public TrackerProfileReader() {
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

					TrackerSettings settings = ReadCap3Point(root);
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

			return result;
		}
	}

}
