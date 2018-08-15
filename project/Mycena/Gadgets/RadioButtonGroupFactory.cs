using System;
namespace Mycena {
	internal class RadioButtonGroupFactory : GadgetFactory<Gtk.RadioButton> {
		public RadioButtonGroupFactory() : base() {
		}
		protected override Gtk.RadioButton CreateGadget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.TextTagTable table = TextTagTableFactory.GetTable(properties, container);
			string label = properties.GetProperty("label", string.Empty);
			return new Gtk.RadioButton(label);
		}
	}
}
