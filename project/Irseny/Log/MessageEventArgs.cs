using System;
namespace Irseny.Log {
	public class MessageEventArgs : EventArgs {
		public MessageEventArgs(LogMessage message) {
			if (message == null) throw new ArgumentNullException("message");
			Message = message;
		}
		public LogMessage Message { get; private set; }
	}
}
