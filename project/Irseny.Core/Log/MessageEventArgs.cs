﻿using System;
namespace Irseny.Core.Log {
	public class MessageEventArgs : EventArgs {
		public MessageEventArgs(LogEntry message) {
			if (message == null) throw new ArgumentNullException("message");
			Message = message;
		}
		public LogEntry Message { get; private set; }
	}
}