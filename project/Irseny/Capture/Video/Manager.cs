using System;

namespace Irseny.Capture.Video {
	public class Manager {


		public Manager() {
		}

		public void Invoke(EventHandler handler) {
			handler(this, new EventArgs());
		}

	}
}

