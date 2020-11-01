using System;

namespace Irseny.Main.Webface {
	public class WebSocketMessage {
		string text = null;
		byte[] binary = null;


		public WebSocketMessage(byte[] binary) {
			if (binary == null) throw new ArgumentNullException("binary");
			this.binary = binary;
		}
		public WebSocketMessage(string text) {
			if (text == null) throw new ArgumentNullException("text");
			this.text = text;
		}

		public bool IsText {
			get { return text != null; }
		}
		public bool IsBinary {
			get { return binary != null; }
		}
		public string Text {
			get {
				if (text == null) throw new InvalidOperationException("Not a text message");
				return text;
			}
		}
		public byte[] Binary {
			get {
				if (binary == null) throw new InvalidOperationException("Not a binary message");
				return binary;
			}
		}
	}
}

