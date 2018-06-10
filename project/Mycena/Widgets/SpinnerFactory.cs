using System;

namespace Mycena {
	/*internal class SpinnerFactory : WidgetFactory<Gtk.spin> {
		public SpinnerFactory() : base() {
			CreationProperties.Add("climb_rate", SetClimb);
			CreationProperties.Add("digits", SetDigits);
		}
		protected override Gtk.SpinButton CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.SpinButton(0, 1, 1);
		}
		private static bool SetAdjustment(Gtk.SpinButton widget, ConfigProperties properties, IInterfaceNode container) {
			Gtk.Adjustment adjustment = container.GetGadget<Gtk.Adjustment>(properties.GetProperty("adjustment"), null);
			if (adjustment == null) {
				return false;
			} else {
				widget.Adjustment = adjustment;
				return true;
			}
			widget.
		}
		private static bool SetClimb(Gtk.SpinButton widget, ConfigProperties properties, IInterfaceNode container) {
			double climb;
			try {
				climb = TextParseTools.ParseDouble(properties.GetProperty("climb_rate"));
			} catch (FormatException) {
				return false;
			}
			widget.ClimbRate = climb;
			return true;
		}
		private static bool SetDigits(Gtk.SpinButton widget, ConfigProperties properties, IInterfaceNode container) {
			uint digits;
			try {
				digits = TextParseTools.ParseUInt(properties.GetProperty("digits"));
			} catch (FormatException) {
				return false;
			}
			widget.Digits = digits;
			return true;
		}
	}*/
}