using System;
using System.Xml;
using System.Collections.Generic;
using Irseny.Capture.Video;

namespace Irseny.Content.Profile {
	public class CaptureProfileWriter {
		public CaptureProfileWriter() {
		}
		public XmlNode Write(SetupProfile profile, XmlDocument target) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (target == null) throw new ArgumentNullException("target");
			var result = target.CreateElement("VideoCapture");
			var indexes = new List<int>(profile.VideoCaptureIndexes);
			foreach (int i in indexes) {
				CaptureSettings settings = profile.GetVideoCapture(i);
				XmlNode node = WriteStream(i, settings, target);
				result.AppendChild(node);
			}
			return result;
		}
		private XmlNode WriteStream(int index, CaptureSettings settings, XmlDocument target) {
			var result = target.CreateElement("Stream");
			result.SetAttribute("Index", index.ToString());
			{ // camera
				XmlElement node = target.CreateElement("Camera");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(CaptureProperty.CameraId, 0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(CaptureProperty.CameraId).ToString());
			}
			{ // width
				XmlElement node = target.CreateElement("Width");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(CaptureProperty.FrameWidth, 640).ToString();
				node.SetAttribute("Auto", settings.IsAuto(CaptureProperty.FrameWidth).ToString());
			}
			{ // height
				XmlElement node = target.CreateElement("Height");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(CaptureProperty.FrameHeight, 480).ToString();
				node.SetAttribute("Auto", settings.IsAuto(CaptureProperty.FrameHeight).ToString());
			}
			{ // fps
				XmlElement node = target.CreateElement("FPS");
				result.AppendChild(node);
				node.InnerText = settings.GetInteger(CaptureProperty.FrameRate, 30).ToString();
				node.SetAttribute("Auto", settings.IsAuto(CaptureProperty.FrameRate).ToString());
			}
			{ // exposure
				XmlElement node = target.CreateElement("Exposure");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(CaptureProperty.Exposure, 0.0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(CaptureProperty.Exposure).ToString());
			}
			{ // brightness
				XmlElement node = target.CreateElement("Brightness");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(CaptureProperty.Brightness, 0.0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(CaptureProperty.Brightness).ToString());
			}
			{ // contrast
				XmlElement node = target.CreateElement("Contrast");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(CaptureProperty.Contrast, 0.0).ToString();
				node.SetAttribute("Auto", settings.IsAuto(CaptureProperty.Contrast).ToString());
			}
			return result;
		}
	}
}
