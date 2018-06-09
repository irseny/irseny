using System;

namespace Mycena {
	internal class ImageGadgetFactory : GadgetFactory<Gtk.Image> {
		public ImageGadgetFactory() : base() {
			
		}
		protected override Gtk.Image CreateGadget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.Image result = ImageWidgetFactory.CreateImage(properties);
			if (result != null) {
				ApplyProperties(result, properties, container);
			}
			return result;
		}
	}
}

