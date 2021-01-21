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
	// TODO change to struct
	public class HttpMessage {
		public HttpHeader Header {  get; private set; }
		public byte[] Content { get; private set; }
		bool HasContent {
			get { return Content != null && Content.Length > 0; }
		}

		public HttpMessage(HttpHeader header, byte[] content) {
			if (header == null) throw new ArgumentNullException("header");
			if (content == null) throw new ArgumentNullException("content");
			Header = header;
			Content = content;
		}
		public HttpMessage(HttpHeader header) {
			if (header == null) throw new ArgumentNullException("header");
			Header = header;
			Content = new byte[0];
		}

	}
}
