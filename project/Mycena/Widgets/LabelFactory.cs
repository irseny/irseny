﻿using System;

namespace Mycena {
	internal class LabelFactory : WidgetFactory<Gtk.Label> {
		public LabelFactory() : base() {
			// entry
			/*CreationProperties.Add("max_length", EntryFactory.SetMaxLength);
			CreationProperties.Add("width_chars", EntryFactory.SetWidthChars);
			CreationProperties.Add("invisible_char", EntryFactory.SetInvisibleChar);*/
			// misc
			CreationProperties.Add("xalign", MiscFactory.SetAligment);
			CreationProperties.Add("yalign", MiscFactory.SetAligment);
			CreationProperties.Add("xpad", MiscFactory.SetPadding);
			CreationProperties.Add("ypad", MiscFactory.SetPadding);
			CreationProperties.Add("margin_left", MiscFactory.SetLeftMargin);
			CreationProperties.Add("margin_right", MiscFactory.SetRightMargin);
			CreationProperties.Add("margin_top", MiscFactory.SetTopMargin);
			CreationProperties.Add("margin_bottom", MiscFactory.SetBottomMargin);
			// label
			CreationProperties.Add("label", SetText);
			CreationAttributes.Add("underline", SetUnderline);
			CreationProperties.Add("angle", SetAngle);
			CreationProperties.Add("width_chars", SetChars);
			CreationProperties.Add("max_width_chars", SetChars);
			CreationProperties.Add("margin", MiscFactory.SetMargin);

		}
		protected override Gtk.Label CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Label();
		}
		private static bool SetText(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			string text = properties.GetProperty("label");
			widget.Text = text;
			return true;
		}
		private static bool SetUnderline(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool underline;
			try {
				underline = TextParseTools.ParseBool(properties.GetAttribute("underline", false));
			} catch (FormatException) {
				return false;
			}
			widget.UseUnderline = underline;
			return true;
		}
		private static bool SetAngle(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			float angle;
			try {
				angle = TextParseTools.ParseFloat(properties.GetProperty("angle", 0));
			} catch (FormatException) {
				return false;
			}
			widget.Angle = angle;
			return true;
		}
		private static bool SetChars(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			int widthChars, maxWidthChars;
			try {
				widthChars = TextParseTools.ParseInt(properties.GetProperty("width_chars", -1));
				maxWidthChars = TextParseTools.ParseInt(properties.GetProperty("max_width_chars", -1));
			} catch (FormatException) {
				return false;
			}
			widget.MaxWidthChars = maxWidthChars;
			widget.WidthChars = widthChars;
			return true;
		}

	}
}
