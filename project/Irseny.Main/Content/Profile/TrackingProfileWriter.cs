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
using Irseny.Core.Tracking;
using Irseny.Core.Util;

namespace Irseny.Main.Content.Profile {
	public class TrackingProfileWriter {
		public TrackingProfileWriter() {
		}
		public bool Write(SetupProfile profile, XmlNode root, XmlDocument target) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (target == null) throw new ArgumentNullException("target");
			if (root == null) throw new ArgumentNullException("root");
			foreach (int i in profile.TrackerIndexes) {
				EquipmentSettings settings = profile.GetTracker(i);
				XmlNode node = WriteCap3Point(i, settings, target);
				if (node != null) {
					root.AppendChild(node);
				}

			}
			return true;
		}
		private XmlNode WriteCap3Point(int index, EquipmentSettings settings, XmlDocument target) {
			XmlElement result = target.CreateElement("Cap3Point");
			result.SetAttribute("Index", index.ToString());
			// stream
			for (int i = 0; i < 2; i++) { // stream
				int stream = settings.GetInteger(TrackerProperty.Stream0 + i, -1);
				if (stream > -1) {
					XmlElement node = target.CreateElement("Stream");
					result.AppendChild(node);
					node.InnerText = stream.ToString();
				}
			}
			{ // model
				// TODO implement
			}
			{ // mixing
				int mixing = settings.GetInteger(TrackerProperty.Smoothing, -1);
				if (mixing > -1) {
					XmlElement node = target.CreateElement("Smoothing");
					result.AppendChild(node);
					node.InnerText = mixing.ToString();
				}
			}
			{
				decimal mixingDecline = settings.GetDecimal(TrackerProperty.SmoothingDropoff, -1.0m);
				if (mixingDecline > -1.0m) {
					XmlElement node = target.CreateElement("SmoothingDropoff");
					result.AppendChild(node);
					node.InnerText = mixingDecline.ToString(JsonString.FormatProvider);
				}
			}
			{
				int bright = settings.GetInteger(TrackerProperty.MinBrightness, -1);
				if (bright > -1) {
					XmlElement node = target.CreateElement("MinBrightness");
					result.AppendChild(node);
					node.InnerText = bright.ToString();
				}
			}
			{ // cluster radius
				int radius = settings.GetInteger(TrackerProperty.MinClusterRadius, -1);
				if (radius > -1) {
					XmlElement node = target.CreateElement("MinClusterRadius");
					result.AppendChild(node);
					node.InnerText = radius.ToString();
				}
			}
			{
				int radius = settings.GetInteger(TrackerProperty.MaxClusterRadius, -1);
				if (radius > -1) {
					XmlElement node = target.CreateElement("MaxClusterRadius");
					result.AppendChild(node);
					node.InnerText = radius.ToString();
				}
			}
			{ // cluster members
				int members = settings.GetInteger(TrackerProperty.MaxClusterMembers, -1);
				if (members > -1) {
					XmlElement node = target.CreateElement("MaxClusterMembers");
					result.AppendChild(node);
					node.InnerText = members.ToString();
				}
			}
			{ // cluster no
				int clusterNo = settings.GetInteger(TrackerProperty.MaxClusterNo, -1);
				if (clusterNo > -1) {
					XmlElement node = target.CreateElement("MaxClusters");
					result.AppendChild(node);
					node.InnerText = clusterNo.ToString();
				}
			}
			{ // point no
				int pointNo = settings.GetInteger(TrackerProperty.MaxPointNo, -1);
				if (pointNo > -1) {
					XmlElement node = target.CreateElement("MaxPoints");
					result.AppendChild(node);
					node.InnerText = pointNo.ToString();
				}
			}
			{ // layer energy
				int energy = settings.GetInteger(TrackerProperty.MinLayerEnergy, -1);
				if (energy > -1) {
					XmlElement node = target.CreateElement("MinLayerEnergy");
					result.AppendChild(node);
					node.InnerText = energy.ToString();
				}
			}
			{ // label no
				int labelNo = settings.GetInteger(TrackerProperty.LabelNo, -1);
				if (labelNo > -1) {
					XmlElement node = target.CreateElement("Labels");
					result.AppendChild(node);
					node.InnerText = labelNo.ToString();
				}
			}
			{ // fast approx threshold
				int threshold = settings.GetInteger(TrackerProperty.FastApproxThreshold, -1);
				if (threshold > -1) {
					XmlElement node = target.CreateElement("FastApproxThreshold");
					result.AppendChild(node);
					node.InnerText = threshold.ToString();
				}
			}
			{ // max queued images
				int imageNo = settings.GetInteger(TrackerProperty.MaxQueuedImages, -1);
				if (imageNo > -1) {
					XmlElement node = target.CreateElement("MaxQueuedImages");
					result.AppendChild(node);
					node.InnerText = imageNo.ToString();
				}
			}
			return result;
		}
	}
}
