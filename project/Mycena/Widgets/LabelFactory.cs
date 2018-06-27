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
			// label
			CreationProperties.Add("label", SetText);
			CreationAttributes.Add("underline", SetUnderline);
		}
		protected override Gtk.Label CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.Label();
		}
		private static bool SetText(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container) {
			string text = properties.GetProperty("label");
			widget.Text = text;
			return true;
		}
		private static bool SetUnderline(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container) {
			bool underline;
			try {
				underline = TextParseTools.ParseBool(properties.GetAttribute("underline", false));
			} catch (FormatException) {
				return false;
			}
			widget.UseUnderline = underline;
			return true;
		}
		/*private static bool SetMaxLength(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container) {
			int length;
			try {
				length = TextParseTools.ParseInt(properties.GetProperty("max_length"));
			}
		}*/
	}
}