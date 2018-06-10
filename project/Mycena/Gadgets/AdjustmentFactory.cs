using System;

namespace Mycena {
	internal class AdjustmentFactory : GadgetFactory<Gtk.Adjustment> {
		public AdjustmentFactory() : base() {

		}
		protected override Gtk.Adjustment CreateGadget(ConfigProperties properties, IInterfaceNode container) {
			double value, lower, upper, stepInc, pageInc, pageSize;
			try {
				value = TextParseTools.ParseDouble(properties.GetProperty("value", 0));
				lower = TextParseTools.ParseDouble(properties.GetProperty("lower", 0));
				upper = TextParseTools.ParseDouble(properties.GetProperty("upper", 100));
				stepInc = TextParseTools.ParseDouble(properties.GetProperty("step_increment", 1));
				pageInc = TextParseTools.ParseDouble(properties.GetProperty("page_increment", 10));
				pageSize = TextParseTools.ParseDouble(properties.GetProperty("page_size", 0));
			} catch (FormatException) {
				return null;
			}
			return new Gtk.Adjustment(value, lower, upper, stepInc, pageInc, pageSize);

		}
		public static Gtk.Adjustment GetAdjustment(ConfigProperties properties, IInterfaceNode container) {
			string adjName;
			if (!properties.TryGetProperty("adjustment", out adjName)) {
				adjName = "adj_Default";
			}
			Gtk.Adjustment result;
			if (!container.TryGetGadget(adjName, out result)) {
				result = new Gtk.Adjustment(0, 0, 10, 1, 10, 0);
				container.RegisterGadget(adjName, result);
			}
			return result;
		}
		public static Gtk.Adjustment GetAdjustment(string adjustment, IInterfaceNode container) {
			Gtk.Adjustment result;
			if (!container.TryGetGadget(adjustment, out result)) {
				result = new Gtk.Adjustment(0, 0, 10, 1, 10, 0);
				container.RegisterGadget(adjustment, result);
			}
			return result;
		}
	}
}

