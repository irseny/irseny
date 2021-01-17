using System;
using System.Xml;
using Irseny.Core.Util;
using Irseny.Core.Sensors;

namespace Irseny.Main.Content.Profile {
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
				if (node.Name.Equals("Webcam")) {
					string sIndex;
					if (node.Attributes["Index"] != null) {
						sIndex = node.Attributes["Index"].InnerText;
					} else {
						sIndex = "-1";
					}
					int index = -1;
					if (sIndex != null) {
						index = JsonString.ParseInt(node.Attributes["Index"].InnerText, -1);
					} 
					string name;
					if (node.Attributes["Name"] != null) {
						name = node.Attributes["Name"].InnerText;
					} else {
						name = "Webcam";
					}
					if (!ReadWebcam(name, index, profile, node)) {
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
		public bool ReadWebcam(string name, int index, SetupProfile profile, XmlNode root) {
			var settings = new SensorSettings(typeof(SensorProperty));
			// TODO read name from data
			settings.SetText(SensorProperty.Name, name);
			settings.SetText(SensorProperty.Type, "Webcam");
			// first parse the node
			foreach (XmlNode node in root.ChildNodes) {
				
				if (node.Name.Equals("Camera")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = JsonString.ParseBool(node.Attributes["Auto"].InnerText, true);
					}

					if (!auto) {
						int camera = JsonString.ParseInt(node.InnerText, -1);
						if (camera < 0) {
							return false;
						}
						settings.SetInteger(SensorProperty.CameraId, camera);
					}
				} else if (node.Name.Equals("Width")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = JsonString.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					if (!auto) {
						int width = JsonString.ParseInt(node.InnerText, 640);
						if (width < 1) {
							return false;
						}
						settings.SetInteger(SensorProperty.FrameWidth, width);
					}
				} else if (node.Name.Equals("Height")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = JsonString.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					if (!auto) {
						int height = JsonString.ParseInt(node.InnerText, 480);
						if (height < 1) {
							return false;
						}
						settings.SetInteger(SensorProperty.FrameHeight, height);
					}
				} else if (node.Name.Equals("FPS")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = JsonString.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					if (!auto) {
						int fps = JsonString.ParseInt(node.InnerText, 30);
						if (fps < 0) {
							return false;
						}
						settings.SetInteger(SensorProperty.FrameRate, fps);
					}
				} else if (node.Name.Equals("Exposure")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = JsonString.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					decimal exposure = JsonString.ParseDecimal(node.InnerText, 0.0m);
					if (!auto) {
						settings.SetDecimal(SensorProperty.Exposure, exposure);
					}
				} else if (node.Name.Equals("Brightness")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = JsonString.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					decimal bright = JsonString.ParseDecimal(node.InnerText, 0.0m);
					if (!auto) {
						settings.SetDecimal(SensorProperty.Brightness, bright);
					}
				} else if (node.Name.Equals("Contrast")) {
					bool auto = true;
					if (node.Attributes["Auto"] != null) {
						auto = JsonString.ParseBool(node.Attributes["Auto"].InnerText, true);
					}
					decimal contrast = JsonString.ParseDecimal(node.InnerText, 0.0m);
					if (!auto) {
						settings.SetDecimal(SensorProperty.Contrast, contrast);
					}
				}
			}
			// add the settings if successful
			profile.AddVideoCapture(index, settings);
			return true;
		}
	}
}
