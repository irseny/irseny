using System;
using System.Collections.Generic;

namespace Mycena {
	internal class SpinButtonFactory : WidgetFactory<Gtk.SpinButton> {
		public SpinButtonFactory() : base() {
			CreationProperties.Add("max_length", EntryFactory.SetMaxLength);
			CreationProperties.Add("invisible_char", EntryFactory.SetInvisibleChar);
			CreationProperties.Add("width_chars", EntryFactory.SetWidth);
			CreationProperties.Add("numeric", SetNumeric);
		}
		protected override Gtk.SpinButton CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			string adjName = properties.GetProperty("adjustment", AdjustmentFactory.DefaultAdjustment);
			Gtk.Adjustment adjustment = AdjustmentFactory.GetAdjustment(adjName, container);
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
		private static bool SetNumeric(Gtk.SpinButton widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
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
