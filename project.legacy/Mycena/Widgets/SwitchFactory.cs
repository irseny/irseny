using System;

namespace Mycena {
	internal class SwitchFactory : WidgetFactory<Gtk.Switch> {
		public SwitchFactory() : base() {
			CreationProperties.Add("active", SetActive);
		}
		protected override Gtk.Switch CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			
			return new Gtk.Switch();
		}
		private static bool SetActive(Gtk.Switch widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool active;
			try {
				active = TextParseTools.ParseBool(properties.GetProperty("active"));

			} catch (FormatException) {
				return false;
			}
			widget.Active = active;
			return true;
		}
	}
}

