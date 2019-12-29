using System;
using System.IO;
using System.Collections.Generic;

namespace Mycena {
	public class InterfaceStock : IInterfaceStock {
		StockAccessConfig accessConfig;
		Dictionary<string, Gdk.Pixbuf> pixbufs;

		public InterfaceStock(StockAccessConfig accessConfig) {
			if (accessConfig == null) throw new ArgumentNullException("accessConfig");
			this.accessConfig = accessConfig;
			this.pixbufs = new Dictionary<string, Gdk.Pixbuf>(32);
		}

		public static IInterfaceStock CreateEmpty() {
			return new InterfaceStock(new StockAccessConfig());
		}
		public void RegisterPixbuf(string path, Gdk.Pixbuf pixbuf) {
			if (path == null) throw new ArgumentNullException("path");
			if (pixbuf == null) throw new ArgumentNullException("pixbuf");
			string name = TrimPath(path);
			try {
				pixbufs.Add(name, pixbuf);
			} catch (ArgumentException) {
				throw new ArgumentException("path: Pixbuf with this path does already exist");
			}
		}
		public void RegisterPixbuf(string path) {
			if (path == null) throw new ArgumentNullException("path");
			Gdk.Pixbuf pixbuf;
			try {
				pixbuf = new Gdk.Pixbuf(path);
			} catch (FileNotFoundException e) {
				throw new ArgumentException("path", e);
			} catch (FileLoadException e) {
				throw new ArgumentException("path", e);
			}
			try {
				RegisterPixbuf(path, pixbuf);
			} catch (ArgumentException e) {
				pixbuf.Dispose();
				throw e;
			}
		}
		public Gdk.Pixbuf GetPixbuf(string path, Gdk.Pixbuf defaultValue) {
			if (path == null) throw new ArgumentNullException("path");
			string name = TrimPath(path);
			Gdk.Pixbuf result;
			if (pixbufs.TryGetValue(name, out result)) {
				return result;
			} else {
				return defaultValue;
			}
		}
		public Gdk.Pixbuf GetPixbuf(string path) {
			if (path == null) throw new ArgumentNullException("path");
			string name = TrimPath(path);
			Gdk.Pixbuf result;
			if (pixbufs.TryGetValue(name, out result)) {
				return result;
			} else {
				throw new KeyNotFoundException(string.Format("Pixbuf named '{0}' does not exist", name));
			}
		}
		private string TrimPath(string path) {
			string directory = Path.GetDirectoryName(path);
			string file = Path.GetFileName(path);
			if (accessConfig.CutFileExtension) {
				file = Path.GetFileNameWithoutExtension(file);
			}
			if (accessConfig.CutDirectoryPath) {
				directory = string.Empty;
			}
			if (directory.Length > 0) {
				return Path.Combine(directory, file);
			} else {
				return file;
			}
		}
		public void Dispose() {
			foreach (Gdk.Pixbuf pixbuf in pixbufs.Values) {
				pixbuf.Dispose();
			}
			pixbufs.Clear();
		}
	}
}
