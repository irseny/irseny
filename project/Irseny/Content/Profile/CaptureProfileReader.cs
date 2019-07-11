using System;
using System.Xml;
using Irseny.Util;
using Irseny.Capture.Video;

namespace Irseny.Content.Profile {
	public class CaptureProfileReader {
		public CaptureProfileReader() {
		}
		/// <summary>
		/// Reads all stream settings from the given node.
		/// </summary>
		/// <returns><c>true</c>, if operation was successful, <c>false</c> otherwise.</returns>
		/// <param name="profile">Target profile.</param>
		/// <param name="root">Root node.</param>
		public bool Read(SetupProfile profile, XmlNode root) {
			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("Stream")) {
					if (node.Attributes["Index"] == null) {
						return false;
					}
					int index = TextParseTools.ParseInt(node.Attributes["Index"].InnerText, -1);
					if (index < 0) {
						return false;
					}
					if (!ReadStream(index, profile, node)) {
						return false;
					}
				}
			}
			return true;
		}
		/// <summary>
		/// Reads stream settings from the given node.
		/// </summary>
		/// <returns><c>true</c>, if operation was successful, <c>false</c> otherwise.</returns>
		/// <param name="index">Capture index.</param>
		/// <param name="profile">Target profile.</param>
		/// <param name="root">Root node.</param>
		public bool ReadStream(int index, SetupProfile profile, XmlNode root) {
			var settings = new CaptureSettings();
			// first parse the node
			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("Camera")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = TextParseTools.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					if (!auto) {
						int camera = TextParseTools.ParseInt(node.InnerText, -1);
						if (camera < 0) {
							return false;
						}
						settings.SetInteger(CaptureProperty.CameraId, camera);
					}
				} else if (node.Name.Equals("Width")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = TextParseTools.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					if (!auto) {
						int width = TextParseTools.ParseInt(node.InnerText, 640);
						if (width < 1) {
							return false;
						}
						settings.SetInteger(CaptureProperty.FrameWidth, width);
					}
				} else if (node.Name.Equals("Height")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = TextParseTools.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					if (!auto) {
						int height = TextParseTools.ParseInt(node.InnerText, 480);
						if (height < 1) {
							return false;
						}
						settings.SetInteger(CaptureProperty.FrameHeight, height);
					}
				} else if (node.Name.Equals("FPS")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = TextParseTools.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					if (!auto) {
						int fps = TextParseTools.ParseInt(node.InnerText, 30);
						if (fps < 0) {
							return false;
						}
						settings.SetInteger(CaptureProperty.FrameRate, fps);
					}
				} else if (node.Name.Equals("Exposure")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = TextParseTools.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					double exposure = TextParseTools.ParseDouble(node.InnerText, 0.0);
					if (!auto) {
						settings.SetDecimal(CaptureProperty.Exposure, exposure);
					}
				} else if (node.Name.Equals("Brightness")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = TextParseTools.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					double bright = TextParseTools.ParseDouble(node.InnerText, 0.0);
					if (!auto) {
						settings.SetDecimal(CaptureProperty.Brightness, bright);
					}
				} else if (node.Name.Equals("Contrast")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = TextParseTools.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					double contrast = TextParseTools.ParseDouble(node.InnerText, 0.0);
					if (!auto) {
						settings.SetDecimal(CaptureProperty.Contrast, contrast);
					}
				}
			}
			// add the settings if successful
			profile.AddVideoCapture(index, settings);
			return true;
		}
	}
}
