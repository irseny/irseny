﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Irseny.Log {
	public class LogManager {
		static LogManager instance = null;
		static Thread instanceThread = null;
		static object instanceSync = new object();

		volatile bool running = false;
		ManualResetEvent pendingSignal = new ManualResetEvent(false);
		object pendingSync = new object();
		Queue<LogMessage> pendingMessages = new Queue<LogMessage>();
		object availEventSync = new object();
		event EventHandler<MessageEventArgs> messageAvailable;
		object openEventSync = new object();
		event EventHandler opened;
		object closeEventSync = new object();
		event EventHandler closed;

		public LogManager() {
		}
		public event EventHandler Opened {
			add {
				lock (openEventSync) {
					opened += value;
				}
			}
			remove {
				lock (openEventSync) {
					opened -= value;
				}
			}
		}
		public event EventHandler<MessageEventArgs> MessageAvailable {
			add {
				lock (availEventSync) {
					messageAvailable += value;
				}
			}
			remove {
				lock (availEventSync) {
					messageAvailable -= value;
				}
			}
		}
		public event EventHandler Closed {
			add {
				lock (closeEventSync) {
					closed += value;
				}
			}
			remove {
				lock (closeEventSync) {
					closed -= value;
				}
			}
		}
		public static LogManager Instance {
			get {
				lock (instanceSync) {
					return instance;
				}
			}
		}


		private void ProcessPending() {
			Queue<LogMessage> toProcess;
			lock (pendingSync) {
				toProcess = pendingMessages;
				pendingMessages = new Queue<LogMessage>();
			}
			EventHandler<MessageEventArgs> handler;
			lock (availEventSync) {
				handler = messageAvailable;
			}
			while (toProcess.Count > 0) {
				var args = new MessageEventArgs(toProcess.Dequeue());
				if (handler != null) {
					handler(this, args);
				}
			}
		}
		private void Run() {
			running = true;
			EventHandler openHandler;
			lock (openEventSync) {
				openHandler = opened;
			}
			if (openHandler != null) {
				openHandler(this, new EventArgs());
			}
			while (running) {
				pendingSignal.WaitOne();
				pendingSignal.Reset();
				ProcessPending();
			}
			EventHandler closeHandler;
			lock (closeEventSync) {
				closeHandler = closed;
			}
			if (closeHandler != null) {
				closeHandler(this, new EventArgs());
			}
		}
		private void SignalStop() {
			running = false;
			pendingSignal.Set();
		}
		public void Log(LogMessage message) {
			if (message == null) throw new ArgumentNullException("message");
			lock (pendingSync) {
				pendingMessages.Enqueue(message);
			}
			pendingSignal.Set();
		}
		public void LogMsg(object source, string text) {
			if (text == null) throw new ArgumentNullException("text");
			Log(LogMessage.CreateMessage(source, text));
		}
		public void LogWarning(object source, string text) {
			if (text == null) throw new ArgumentNullException("text");
			Log(LogMessage.CreateWarning(source, text));
		}
		public void LogError(object source, string text) {
			if (text == null) throw new ArgumentNullException("text");
			Log(LogMessage.CreateError(source, text));
		}
		public static void MakeInstance(LogManager instance) {
			lock (instanceSync) {
				if (LogManager.instance != null) {
					LogManager.instance.SignalStop();
					if (instanceThread.IsAlive) {
						instanceThread.Join();
					}
					instanceThread = null;
				}
				LogManager.instance = instance;
				if (instance != null) {
					instanceThread = new Thread(instance.Run);
					instanceThread.Start();
				}
			}

		}
	}


}
