using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ScrolledWindowFactory : WidgetFactory<Gtk.ScrolledWindow> {
		public ScrolledWindowFactory() : base() {
			CreationProperties.Add("hscrollbar_policy", SetPolicy);
			CreationProperties.Add("vscrollbar_policy", SetPolicy);
		}

		protected override Gtk.ScrolledWindow CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.ScrolledWindow();
		}
		protected override bool PackWidget(Gtk.ScrolledWindow container, Gtk.Widget child, ConfigProperties properties) {
			container.AddWithViewport(child);
			return true;
		}
		private static bool SetPolicy(Gtk.ScrolledWindow widget, ConfigProperties properties, IInterfaceNode container) {
			Gtk.PolicyType horizontalPolicy, verticalPolicy;
			widget.GetPolicy(out horizontalPolicy, out verticalPolicy);

			try {
				string sPolicy;
				if (properties.TryGetProperty("hscrollbar_policy", out sPolicy)) {
					horizontalPolicy = TextParseTools.ParsePolicyType(sPolicy);
				}
				if (properties.TryGetProperty("vscrollbar_policy", out sPolicy)) {
					verticalPolicy = TextParseTools.ParsePolicyType(sPolicy);
				}
			} catch (FormatException) {
				return false;
			}
			widget.SetPolicy(horizontalPolicy, verticalPolicy);
			return true;
		}



	}
}

