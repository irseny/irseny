using System;
using System.Collections.Generic;
using System.Threading;

namespace Irseny.Capture.Video {
	public class CaptureSystem {
		static object instanceSync = new object();
		static Thread instanceThread = null;
		static volatile CaptureSystem instance = null;

		object invokeSync = new object();
		volatile bool running = false;
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();
		ManualResetEvent invokeSignal = new ManualResetEvent(false);
		object createEventSync = new object();
		object destroyEventSync = new object();
		event EventHandler<StreamEventArgs> streamCreated;
		event EventHandler<StreamEventArgs> streamDestroyed;
		object streamSync = new object();
		IList<CaptureStream> streams = new List<CaptureStream>(4);

		public event EventHandler<StreamEventArgs> StreamCreated {
			add {
				lock (createEventSync) {
					streamCreated += value;
				}
			}
			remove {
				lock (createEventSync) {
					streamCreated -= value;
				}
			}
		}
		public event EventHandler<StreamEventArgs> StreamDestroyed {
			add {
				lock (destroyEventSync) {
					streamDestroyed += value;
				}
			}
			remove {
				lock (destroyEventSync) {
					streamDestroyed -= value;
				}
			}
		}
		public static CaptureSystem Instance {
			get {

				lock (instanceSync) {
					return instance;
				}
			}
		}

		public static void MakeInstance(CaptureSystem instance) {
			lock (instanceSync) {
				if (CaptureSystem.instance != null) {
					CaptureSystem.instance.SignalStop();
					if (instanceThread.IsAlive) {
						instanceThread.Join();
					}
					instanceThread = null;
				}
				CaptureSystem.instance = instance;
				if (instance != null) {
					instanceThread = new Thread(instance.Run);
					instanceThread.Start();
				}
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
		private void SignalStop() {
			running = false;
			invokeSignal.Set(); // wake up the operation invoking thread for exit
		}
		private void Run() {
			running = true;
			while (running) {
				invokeSignal.WaitOne(); // TODO: evaluate whether a timeout is required
				invokeSignal.Reset(); // may reset even if there are operations pending
				InvokePending(); // these are processed here, the signal can be set again in the meantime
			}
			// clean up
			lock (streamSync) {
				for (int id = streams.Count - 1; id >= 0; id--) { // streams removed due to descending id
					DestroyStream(id);
				}

			}
		}
		private void OnStreamCreated(StreamEventArgs args) {
			EventHandler<StreamEventArgs> handler;
			lock (createEventSync) {
				handler = streamCreated;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		private void OnStreamDestroyed(StreamEventArgs args) {
			EventHandler<StreamEventArgs> handler;
			lock (destroyEventSync) {
				handler = streamDestroyed;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		public int CreateStream() {
			CaptureStream stream;
			int index;
			lock (streamSync) {
				// find unused stream id
				for (index = 0; index < streams.Count; index++) {
					if (streams[index] == null) {
						break;
					}
				}
				// create stream
				stream = new CaptureStream(index);
				if (index < streams.Count) {
					streams[index] = stream;
				} else {
					streams.Add(stream);
				}
			}
			OnStreamCreated(new StreamEventArgs(stream, index));
			return index;
		}
		public CaptureStream GetStream(int id) {
			lock (streamSync) {
				if (id >= 0 && id < streams.Count) {
					return streams[id];
				}
			}
			return null;
		}
		public bool DestroyStream(int id) {
			lock (streamSync) {
				if (id >= 0 && id < streams.Count) {
					streams[id].Stop();
					if (id == streams.Count - 1) {
						streams.RemoveAt(id);
					} else {
						streams[id] = null;
					}
					return true;
				}
			}
			return false;
		}

		public void Invoke(EventHandler handler) {
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
				invokeSignal.Set();
			}
		}

	}
}

