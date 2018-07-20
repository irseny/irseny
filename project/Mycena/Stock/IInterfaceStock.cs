using System;
namespace Mycena {
	public interface IInterfaceStock : IDisposable {
		/// <summary>
		/// Registers the given pixbuf under the given path.
		/// </summary>
		/// <param name="path">Unique identifer.</param>
		/// <param name="pixbuf">Pixbuf to register.</param>
		void RegisterPixbuf(string path, Gdk.Pixbuf pixbuf);
		/// <summary>
		/// Registers the pixbuf specified by the given path.
		/// Load the pixbuf before registration.
		/// </summary>
		/// <param name="path">Image file path.</param>
		void RegisterPixbuf(string path);
		/// <summary>
		/// Gets a previously registered pixbuf. 
		/// Returns <paramref name="defaultValue"/> if the operation was not successful.
		/// </summary>
		/// <returns>The previously registered pixbuf.</returns>
		/// <param name="path">Pixbuf identifier.</param>
		/// <param name="defaultValue">Default return value.</param>
		Gdk.Pixbuf GetPixbuf(string path, Gdk.Pixbuf defaultValue);
		/// <summary>
		/// Gets a previously registered pixbuf.
		/// </summary>
		/// <returns>The previously registered pixbuf.</returns>
		/// <param name="path">Pixbuf identifier.</param>
		Gdk.Pixbuf GetPixbuf(string path);
	}
}
