using System;
using System.Collections.Generic;
using System.Threading;

namespace Irseny.Capture.Video {
	public class CaptureSystem {
		static object instanceSync;
		static Thread instanceThread;
		static volatile CaptureSystem instance;

		object invokeSync = new object();
		bool running = false;
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();


		static CaptureSystem() {
			instanceSync = new object();
			MakeInstance(new CaptureSystem());
		}
		public CaptureSystem() {

		}

		public static CaptureSystem Instance {
			get { return Instance; }
		}

		public static void MakeInstance(CaptureSystem instance) {
			if (instance == null) throw new ArgumentNullException("instance");
			lock (instanceSync) {
				if (Instance != null) {
					Instance.SignalStop();
					instanceThread.Join();
				}
				CaptureSystem.instance = instance;
				instanceThread = new Thread(instance.Run);
			}

		}
		private void InvokePending() {
			Queue<EventHandler> pending;
			lock (invokeSync) {
				pending = toInvoke;
				toInvoke = new Queue<EventHandler>();
			}
			foreach (EventHandler handler in pending) {
				handler(this, new EventArgs());
			}

		}
		public void Invoke(EventHandler handler) {
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
			}
		}

		private void SignalStop() {
			running = false;
		}
		private void Run() {
			running = true;
			while (running) {
				InvokePending();
				Thread.Sleep(30);
			}
		}


	}
}

