using System;

namespace Mycena {
	internal class ViewportFactory : WidgetFactory<Gtk.Viewport> {
		public ViewportFactory() : base() {
			CreationProperties.Add("hpolicy", SetHScrollPolicy);
			CreationProperties.Add("vpolicy", SetVScrollPolicy);
			CreationProperties.Add("hadjustment", SetHAdjustment);
			CreationProperties.Add("vadjustment", SetVAdjustment);
		}
		protected override Gtk.Viewport CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {			
			return new Gtk.Viewport();
		}
		protected override bool PackWidget(Gtk.Viewport container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			container.Add(child);
			return true;
		}
		private static bool SetHScrollPolicy(Gtk.Viewport widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.ScrollablePolicy policy;
			try {
				policy = TextParseTools.ParseScrollablePolicy(properties.GetProperty("hpolicy", Gtk.ScrollablePolicy.Minimum));
			} catch (FormatException) {
				return false;
			}
			widget.HscrollPolicy = policy;
			return true;
		}
		private static bool SetVScrollPolicy(Gtk.Viewport widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.ScrollablePolicy policy;
			try {
				policy = TextParseTools.ParseScrollablePolicy(properties.GetProperty("vpolicy", Gtk.ScrollablePolicy.Minimum));
			} catch (FormatException) {
				return false;
			}
			widget.VscrollPolicy = policy;
			return true;
		}
		private static bool SetHAdjustment(Gtk.Viewport widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {			
			string adjustmentName;
			if (properties.TryGetProperty("hadjustment", out adjustmentName)) {
				Gtk.Adjustment adjustment = AdjustmentFactory.GetAdjustment(adjustmentName, container);
				widget.Vadjustment = adjustment;
				return true;
			}
			return false;
		}
		private static bool SetVAdjustment(Gtk.Viewport widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {			
			string adjustmentName;
			if (properties.TryGetProperty("vadjustment", out adjustmentName)) {
				Gtk.Adjustment adjustment = AdjustmentFactory.GetAdjustment(adjustmentName, container);
				widget.Hadjustment = adjustment;
				return true;
			}
			return false;
		}
	}
}

