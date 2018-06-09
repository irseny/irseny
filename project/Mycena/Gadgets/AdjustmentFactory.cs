using System;

namespace Mycena {
	internal class AdjustmentFactory : GadgetFactory<Gtk.Adjustment> {
		public AdjustmentFactory() : base() {
			
		}
		protected override Gtk.Adjustment CreateGadget(ConfigProperties properties, IInterfaceNode container) {
			try {
				double value = TextParseTools.ParseDouble(properties.GetProperty("value", "0"));
				double minValue = TextParseTools.ParseDouble(properties.GetProperty("lower", "0"));
				double maxValue = TextParseTools.ParseDouble(properties.GetProperty("upper", "100"));
				double stepInc = TextParseTools.ParseDouble(properties.GetProperty("step_increment", "1"));
				double pageInc = TextParseTools.ParseDouble(properties.GetProperty("page_increment", "10"));
				double pageSize = TextParseTools.ParseDouble(properties.GetProperty("page_size", "0"));
				return new Gtk.Adjustment(value, minValue, maxValue, stepInc, pageInc, pageSize);
			} catch (FormatException) {
				return null;
			}

		}
	}
}

