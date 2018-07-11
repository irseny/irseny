using System;

namespace Mycena {
	internal class TableFactory : WidgetFactory<Gtk.Table> {
		public TableFactory() : base() {

		}
		protected override Gtk.Table CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			uint rows, columns;
			bool homogenous;
			try {
				rows = TextParseTools.ParseUInt(properties.GetProperty("n_rows", 0));
				columns = TextParseTools.ParseUInt(properties.GetProperty("n_columns", 0));
				homogenous = TextParseTools.ParseBool(properties.GetProperty("homogenous", false));
			} catch (FormatException) {
				return null;
			}
			return new Gtk.Table(rows, columns, homogenous);
		}
		protected override bool PackWidget(Gtk.Table container, Gtk.Widget child, ConfigProperties properties) {
			uint left, right, top, bottom;
			uint xpad, ypad;
			Gtk.AttachOptions xopt, yopt;
			try {
				left = TextParseTools.ParseUInt(properties.GetProperty("left_attach", 0));
				right = TextParseTools.ParseUInt(properties.GetProperty("right_attach", 1));
				top = TextParseTools.ParseUInt(properties.GetProperty("top_attach", 0));
				bottom = TextParseTools.ParseUInt(properties.GetProperty("bottom_attach", 1));
				xpad = TextParseTools.ParseUInt(properties.GetProperty("x_padding", 0));
				ypad = TextParseTools.ParseUInt(properties.GetProperty("y_padding", 0));
				xopt = TextParseTools.ParseAttachOptions(properties.GetProperty("x_options", "GTK_EXPAND|GTK_FILL"));
				yopt = TextParseTools.ParseAttachOptions(properties.GetProperty("y_options", "GTK_EXPAND|GTK_FILL"));
			} catch (FormatException) {
				return false;
			}
			container.Attach(child, left, right, top, bottom, xopt, yopt, xpad, ypad);
			return true;
		}
	}
}