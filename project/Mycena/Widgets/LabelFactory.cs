using System;
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
			CreationProperties.Add("xpadding", MiscFactory.SetPadding);
			CreationProperties.Add("ypadding", MiscFactory.SetPadding);
			// label
			CreationProperties.Add("text", SetText);
		}
		protected override Gtk.Label CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.Label();
		}
		private static bool SetText(Gtk.Label widget, ConfigProperties properties, IInterfaceNode container) {
			string text = properties.GetProperty("text");
			widget.Text = text;
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
