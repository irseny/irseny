using System;
using System.Collections.Generic;

namespace Irseny.Iface {
	public static class InvokeHack {

		static Queue<EventHandler> pendingEvents = new Queue<EventHandler>();
		static readonly object invokeLock = new object();

		public static void Invoke(EventHandler handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			lock (invokeLock) {
				pendingEvents.Enqueue(handler);
			}
		}
		public static void Process() {
			Queue<EventHandler> toInvoke;
			lock (invokeLock) {
				toInvoke = new Queue<EventHandler>(pendingEvents);
				pendingEvents.Clear();
			}

			object sender = null;
			var args = new EventArgs();
			while (toInvoke.Count > 0) {
				EventHandler handler = toInvoke.Dequeue();
				handler(sender, args);
			}
		}
	}
}
