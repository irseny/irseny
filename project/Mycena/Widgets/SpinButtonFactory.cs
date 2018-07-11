using System;
using System.Collections.Generic;

namespace Mycena {
	internal class SpinButtonFactory : WidgetFactory<Gtk.SpinButton> {
		public SpinButtonFactory() : base() {
			CreationProperties.Add("max_length", EntryFactory.SetMaxLength);
			CreationProperties.Add("invisible_char", EntryFactory.SetInvisibleChar);
			CreationProperties.Add("width_chars", EntryFactory.SetWidthChars);
			//CreationProperties.Add("adjustment", SetAdjustment);
			CreationProperties.Add("numeric", SetNumeric);
		}
		protected override Gtk.SpinButton CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.Adjustment adjustment = AdjustmentFactory.GetAdjustment(properties, container);
			double climb;
			uint digits;
			try {
				climb = TextParseTools.ParseDouble(properties.GetProperty("climb_rate", 1));
				digits = TextParseTools.ParseUInt(properties.GetProperty("digits", 0));
			} catch (FormatException) {
				return null;
			}
			return new Gtk.SpinButton(adjustment, climb, digits);
		}
		private static bool SetNumeric(Gtk.SpinButton widget, ConfigProperties properties, IInterfaceNode container) {
			bool numeric;
			try {
				numeric = TextParseTools.ParseBool(properties.GetProperty("numeric"));
			} catch (FormatException) {
				return false;
			}
			widget.Numeric = numeric;

			return true;
		}
	}
}
