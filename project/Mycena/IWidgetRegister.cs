using System;

namespace Mycena {
	internal interface IWidgetRegister {
		/// <summary>
		/// Stores the given widget.
		/// </summary>
		/// <param name="name">Unique widget name.</param>
		/// <param name="widget">Widget to store.</param>
		void RegisterWidget(string name, Gtk.Widget widget);
	}
}

