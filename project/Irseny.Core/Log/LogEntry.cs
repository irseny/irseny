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
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;

namespace Irseny.Core.Log {
	public class LogEntry {
		string sourceFile;
		int sourceLine;
		string sourceSystem;
		string description;
		MessageType messageType;


		private LogEntry(MessageType messageType, object source, string description, string sourceFile, int sourceLine) {
			this.messageType = messageType;
			if (source == null) {
				this.sourceSystem = string.Empty;
			} else {
				if (source is string) {
					this.sourceSystem = source.ToString();
				} else {
					this.sourceSystem = source.GetType().FullName;
				}
			}
			this.description = description;
			this.sourceFile = Path.GetFileNameWithoutExtension(sourceFile);
			this.sourceLine = sourceLine;
		}

		public MessageType MessageType {
			get { return messageType; }
		}


		public static LogEntry CreateMessage(object source, string description, [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0) {
			return new LogEntry(MessageType.Signal, source, description, sourceFile, sourceLine);
		}
		public static LogEntry CreateWarning(object source, string description, [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0) {
			return new LogEntry(MessageType.Warning, source, description, sourceFile, sourceLine);
		}
		public static LogEntry CreateError(object source, string description, [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0) {
			return new LogEntry(MessageType.Error, source, description, sourceFile, sourceLine);
		}

		public override string ToString() {
			return ToLongString();
		}
		public string ToDebugString() {
			var result = new StringBuilder(120);
			result.Append(sourceFile);
			result.Append(" (");
			result.Append(sourceLine);
			result.Append(") ");
			AppendMessage(result);
			return result.ToString();
		}
		public string ToLongString() {
			var result = new StringBuilder(120);
			result.Append(sourceSystem);
			result.Append(": ");
			AppendMessage(result);
			return result.ToString();
		}
		public string ToDescription() {
			var result = new StringBuilder(80);
			AppendMessage(result);
			return result.ToString();
		}
		private void AppendMessage(StringBuilder result) {
			switch (MessageType) {
			case MessageType.Signal:
				result.Append("Message: ");
				break;
			case MessageType.Warning:
				result.Append("Warning: ");
				break;
			case MessageType.Error:
				result.Append("Error: ");
				break;
			}
			result.Append(description);
		}
	}
}