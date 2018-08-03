using System;

namespace Mycena {
	internal class ScaleFactory : WidgetFactory<Gtk.Scale> {
		public ScaleFactory() : base() {
			CreationProperties.Add("round_digits", SetRoundDigits);
			CreationProperties.Add("draw_value", SetDrawValue);

		}
		protected override Gtk.Scale CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.Orientation orientation;
			try {
				orientation = TextParseTools.ParseOrientation(properties.GetProperty("orientation", Gtk.Orientation.Horizontal));
			} catch (FormatException) {
				return null;
			}
			string adjName = properties.GetProperty("adjustment", AdjustmentFactory.DefaultAdjustment);
			Gtk.Adjustment adjustment = AdjustmentFactory.GetAdjustment(adjName, container);
			return new Gtk.Scale(orientation, adjustment);
		}
		private static bool SetRoundDigits(Gtk.Scale widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			int digits;
			try {
				digits = TextParseTools.ParseInt(properties.GetProperty("round_digits"));
			} catch (FormatException) {
				return false;
			}
			widget.Digits = digits;
			return true;
		}
		private static bool SetDrawValue(Gtk.Scale widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool draw;
			try {
				draw = TextParseTools.ParseBool(properties.GetProperty("draw_value"));
			} catch (FormatException) {
				return false;
			}
			widget.DrawValue = draw;
			return true;
		}
	}
}
