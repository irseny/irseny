﻿using System;
using System.Collections.Generic;

namespace Mycena {
	internal class FrameFactory : WidgetFactory<Gtk.Frame> {
		public FrameFactory() : base() {
			CreationProperties.Add("label_xalign", SetLabelAlignmentX);
			CreationProperties.Add("label_yalign", SetLabelAlignmentY);
			CreationProperties.Add("shadow_type", SetShadowType);
		}
		protected override Gtk.Frame CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Frame();
		}
		/*protected override bool PackWidget(Gtk.Frame container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			container.Child = child;
			return true;
		}*/
		protected override bool PackWidgets(Gtk.Frame container, IList<Tuple<Gtk.Widget, ConfigProperties>> children, IInterfaceStock stock) {
			foreach (var pair in children) {
				string childType = pair.Item2.GetAttribute("child_type", string.Empty);
				if (childType.Equals("label")) {
					container.LabelWidget = pair.Item1;
				} else {
					container.Child = pair.Item1;
				}
			}
			return true;
		}
		private static bool SetLabelAlignmentX(Gtk.Frame widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			float align;
			try {
				align = TextParseTools.ParseFloat(properties.GetProperty("label_xalign"));
			} catch (FormatException) {
				return false;
			}
			widget.LabelXalign = align;
			return true;
		}
		private static bool SetLabelAlignmentY(Gtk.Frame widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			float align;
			try {
				align = TextParseTools.ParseFloat(properties.GetProperty("label_yalign"));
			} catch (FormatException) {
				return false;
			}
			widget.LabelYalign = align;
			return true;
		}
		private static bool SetShadowType(Gtk.Frame widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.ShadowType shadow;
			try {
				shadow = TextParseTools.ParseShadowType(properties.GetProperty("shadow_type"));
			} catch (FormatException) {
				return false;
			}
			widget.ShadowType = shadow;
			return true;
		}
	}
}