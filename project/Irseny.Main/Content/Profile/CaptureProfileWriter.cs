using System;
using System.Xml;
using System.Collections.Generic;
using Irseny.Core.Sensors.VideoCapture;

namespace Irseny.Main.Content.Profile {
	public class CaptureProfileWriter {
		public CaptureProfileWriter() {
		}
		public XmlNode Write(SetupProfile profile, XmlNode root, XmlDocument target) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (target == null) throw new ArgumentNullException("target");
			if (root == null) throw new ArgumentNullException("root");
			foreach (int i in profile.VideoCaptureIndexes) {
				SensorSettings settings = profile.GetVideoCapture(i);
				XmlNode node = WriteStream(i, settings, target);
				if (node != null) {
					root.AppendChild(node);
				}
			}
			return root;
		}
		private XmlNode WriteStream(int index, SensorSettings settings, XmlDocument target) {
			var result = target.CreateElement("Stream");
			result.SetAttribute("Index", index.ToString());
			{ // camera
				XmlElement node = target.CreateElement("Camera");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(SensorProperty.CameraId, 0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.CameraId).ToString());
			}
			{ // width
				XmlElement node = target.CreateElement("Width");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(SensorProperty.FrameWidth, 640).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.FrameWidth).ToString());
			}
			{ // height
				XmlElement node = target.CreateElement("Height");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(SensorProperty.FrameHeight, 480).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.FrameHeight).ToString());
			}
			{ // fps
				XmlElement node = target.CreateElement("FPS");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(SensorProperty.FrameRate, 30).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.FrameRate).ToString());
			}
			{ // exposure
				XmlElement node = target.CreateElement("Exposure");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(SensorProperty.Exposure, 0.0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.Exposure).ToString());
			}
			{ // brightness
				XmlElement node = target.CreateElement("Brightness");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(SensorProperty.Brightness, 0.0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.Brightness).ToString());
			}
			{ // contrast
				XmlElement node = target.CreateElement("Contrast");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(SensorProperty.Contrast, 0.0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.Contrast).ToString());
			}
			return result;
		}
	}
}
