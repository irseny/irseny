// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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

