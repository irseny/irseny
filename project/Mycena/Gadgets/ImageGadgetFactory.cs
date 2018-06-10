﻿using System;

namespace Mycena {
	internal class ImageGadgetFactory : GadgetFactory<Gtk.Image> {
		public ImageGadgetFactory() : base() {
			CreationProperties.Add("stock", ImageWidgetFactory.SetStock);
			CreationProperties.Add("icon_name", ImageWidgetFactory.SetIcon);
		}
		protected override Gtk.Image CreateGadget(ConfigProperties properties, IInterfaceNode container) {
			var result = new Gtk.Image();
			ApplyProperties(result, properties, container);
			return result;
		}
	}
}

