using System;

namespace Mycena {
	internal class GridFactory : WidgetFactory<Gtk.Grid> {
		public GridFactory() : base() {
			CreationProperties.Add("row_homogeneous", SetHomogenous);
			CreationProperties.Add("column_homogeneous", SetHomogenous);
		}
		protected override Gtk.Grid CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Grid();
		}
		protected override bool PackWidget(Gtk.Grid container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			int left, top, width, height;
			try {
				left = TextParseTools.ParseInt(properties.GetProperty("left_attach", 0));
				top = TextParseTools.ParseInt(properties.GetProperty("top_attach", 0));
				width = TextParseTools.ParseInt(properties.GetProperty("width", 1));
				height = TextParseTools.ParseInt(properties.GetProperty("height", 1));
			} catch (FormatException) {
				return false;
			}
			container.Attach(child, left, top, width, height);
			return true;
		}
		private static bool SetHomogenous(Gtk.Grid widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool rowHomogenous, columnHomogenous;
			try {
				rowHomogenous = TextParseTools.ParseBool(properties.GetProperty("row_homogeneous", false));
				columnHomogenous = TextParseTools.ParseBool(properties.GetProperty("column_homogeneous", false));
			} catch (FormatException) {
				return false;
			}
			widget.RowHomogeneous = rowHomogenous;
			widget.ColumnHomogeneous = columnHomogenous;
			return true;
		}
	}
}
