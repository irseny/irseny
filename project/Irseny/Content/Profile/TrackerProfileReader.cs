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

					ICapTrackerOptions options = ReadCap3Point(root);
					if (options == null) {
						return false;
					}
					profile.AddTracker(index, options);
				}
			}
			return true;
		}
		private ICapTrackerOptions ReadCap3Point(XmlNode root) {
			var result = new Cap3PointOptions();

			return result;
		}
	}

}
