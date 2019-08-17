﻿using System;
using System.Collections.Generic;
using System.Threading;
using Irseny.Log;

namespace Irseny.Capture.Video {
	public class CaptureSystem {
		static object instanceSync = new object();
		static Thread instanceThread = null;
		static volatile CaptureSystem instance = null;

		object invokeSync = new object();
		volatile bool running = false;
		Queue<EventHandler> toInvoke = new Queue<EventHandler>();
		AutoResetEvent invokeSignal = new AutoResetEvent(false);
		object streamEventSync = new object();
		event EventHandler<StreamEventArgs> streamCreated;
		event EventHandler<StreamEventArgs> streamDestroyed;
		object streamSync = new object();
		IList<CaptureStream> streams = new List<CaptureStream>(4);

		public event EventHandler<StreamEventArgs> StreamCreated {
			add {
				lock (streamEventSync) {
					streamCreated += value;
				}
			}
			remove {
				lock (streamEventSync) {
					streamCreated -= value;
				}
			}
		}
		public event EventHandler<StreamEventArgs> StreamDestroyed {
			add {
				lock (streamEventSync) {
					streamDestroyed += value;
				}
			}
			remove {
				lock (streamEventSync) {
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
					instanceThread.Join(2048);
					if (instanceThread.IsAlive) {
						LogManager.Instance.LogWarning(instance, "Capture thread does not terminate. Aborting.");
						instanceThread.Abort();
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
				invokeSignal.WaitOne();
				InvokePending();
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
			lock (streamEventSync) {
				handler = streamCreated;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		private void OnStreamDestroyed(StreamEventArgs args) {
			EventHandler<StreamEventArgs> handler;
			lock (streamEventSync) {
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
					if (streams[id] != null) {
						streams[id].Stop();
					}
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

