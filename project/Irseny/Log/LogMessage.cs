using System;
using System.Text;

namespace Irseny.Log {
	public class LogMessage {

		private LogMessage(MessageType messageType, object source, string text) {
			if (text == null) throw new ArgumentNullException("text");
			MessageType = messageType;
			Source = source;
			InnerText = text;
			Timestamp = DateTime.UtcNow;
		}

		public string InnerText { get; private set; }
		public object Source { get; private set; }
		public DateTime Timestamp { get; private set; }
		public MessageType MessageType { get; private set; }

		public static LogMessage CreateMessage(object source, string text) {
			return new LogMessage(MessageType.Signal, source, text);
		}
		public static LogMessage CreateMessage(object source, string format, params object[] args) {
			return new LogMessage(MessageType.Signal, source, string.Format(format, args));
		}
		public static LogMessage CreateWarning(object source, string text) {
			return new LogMessage(MessageType.Warning, source, text);
		}
		public static LogMessage CreateWarning(object source, string format, params object[] args) {
			return new LogMessage(MessageType.Warning, source, string.Format(format, args));
		}
		public static LogMessage CreateError(object source, string text) {
			return new LogMessage(MessageType.Error, source, text);
		}
		public static LogMessage CreateError(object source, string format, params object[] args) {
			return new LogMessage(MessageType.Error, source, string.Format(format, args));
		}

		public override string ToString() {
			return ToLongString();
		}
		public string ToLongString() {
			var result = new StringBuilder(120);
			result.Append('|');
			result.Append(Timestamp.ToString());
			result.Append('|');
			if (Source == null) {

			} else if (Source is string) {
				result.Append(Source);
			} else {
				result.Append(Source.GetType().FullName);
			}
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
			result.Append(InnerText);
		}
	}
}