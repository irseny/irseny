using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ScrolledWindowFactory : WidgetFactory<Gtk.ScrolledWindow> {
		public ScrolledWindowFactory() : base() {
			CreationProperties.Add("hscrollbar_policy", SetPolicy);
			CreationProperties.Add("vscrollbar_policy", SetPolicy);
			CreationProperties.Add("propagate_natural_width", SetHExpand);
			CreationProperties.Add("propagate_natural_height", SetVExpand);
			CreationProperties.Add("shadow_type", SetShadow);
		}

		protected override Gtk.ScrolledWindow CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.ScrolledWindow();
		}
		protected override bool PackWidget(Gtk.ScrolledWindow container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			if (child is Gtk.Viewport) {
				container.Add(child);
			} else {
				container.AddWithViewport(child);
			}
			return true;
		}
		private static bool SetHExpand(Gtk.ScrolledWindow widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool expand = false;
			try {
				string sExpand;
				if (properties.TryGetProperty("propagate_natural_width", out sExpand)) {
					expand = TextParseTools.ParseBool(sExpand);
				}
			} catch (FormatException) {
				return false;
			}
			widget.Hexpand = expand;
			return true;
		}
		private static bool SetVExpand(Gtk.ScrolledWindow widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool expand = false;
			try {
				string sExpand;
				if (properties.TryGetProperty("propagate_natural_height", out sExpand)) {
					expand = TextParseTools.ParseBool(sExpand);
				}
			} catch (FormatException) {
				return false;
			}
			widget.Vexpand = expand;
			return true;
		}
		private static bool SetPolicy(Gtk.ScrolledWindow widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
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
		private static bool SetShadow(Gtk.ScrolledWindow widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.ShadowType shadow;
			try {
				shadow = TextParseTools.ParseShadowType(properties.GetProperty("shadow_type", Gtk.ShadowType.In));
			} catch (FormatException) {
				return false;
			}
			widget.ShadowType = shadow;
			return true;
		}
	}
}

