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
using System.Xml;
using System.Collections.Generic;
using Irseny.Core.Sensors;
using Irseny.Core.Util;

namespace Irseny.Main.Content.Profile {
	public class CaptureProfileWriter {
		public CaptureProfileWriter() {
		}
		public XmlNode Write(SetupProfile profile, XmlNode root, XmlDocument target) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (target == null) throw new ArgumentNullException("target");
			if (root == null) throw new ArgumentNullException("root");
			foreach (int i in profile.VideoCaptureIndexes) {
				EquipmentSettings settings = profile.GetVideoCapture(i);
				XmlNode node = WriteStream(i, settings, target);
				if (node != null) {
					root.AppendChild(node);
				}
			}
			return root;
		}
		private XmlNode WriteStream(int index, EquipmentSettings settings, XmlDocument target) {
			var result = target.CreateElement("Webcam");
			result.SetAttribute("Index", index.ToString(JsonString.FormatProvider));
			result.SetAttribute("Name", settings.GetText(SensorProperty.Name, "Webcam"));
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
				node.InnerText = settings.GetDecimal(SensorProperty.Exposure, 0.0m).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.Exposure).ToString());
			}
			{ // brightness
				XmlElement node = target.CreateElement("Brightness");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(SensorProperty.Brightness, 0.0m).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.Brightness).ToString());
			}
			{ // contrast
				XmlElement node = target.CreateElement("Contrast");
				result.AppendChild(node);
				node.InnerText = settings.GetDecimal(SensorProperty.Contrast, 0.0m).ToString();
				node.SetAttribute("Auto", settings.IsAuto(SensorProperty.Contrast).ToString());
			}
			return result;
		}
	}
}
