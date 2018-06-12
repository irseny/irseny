using System;
namespace Mycena {
	internal abstract class ScaleFactory : WidgetFactory<Gtk.Scale> {
		public ScaleFactory() : base() {
			CreationProperties.Add("round_digits", SetRoundDigits);
			CreationProperties.Add("draw_value", SetDrawValue);

		}
		private static bool SetRoundDigits(Gtk.Scale widget, ConfigProperties properties, IInterfaceNode container) {
			int digits;
			try {
				digits = TextParseTools.ParseInt(properties.GetProperty("round_digits"));
			} catch (FormatException) {
				return false;
			}
			widget.Digits = digits;
			return true;
		}
		private static bool SetDrawValue(Gtk.Scale widget, ConfigProperties properties, IInterfaceNode container) {
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
	internal class HorizontalScaleFactory : ScaleFactory {
		public HorizontalScaleFactory() : base() {

		}
		protected override Gtk.Scale CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.Adjustment adjustment = AdjustmentFactory.GetAdjustment(properties, container);
			return new Gtk.HScale(adjustment);
		}
	}
	internal class VerticalScaleFactory : ScaleFactory {
		public VerticalScaleFactory() : base() {

		}
		protected override Gtk.Scale CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.Adjustment adjustment = AdjustmentFactory.GetAdjustment(properties, container);
			return new Gtk.VScale(adjustment);
		}
	}


}
