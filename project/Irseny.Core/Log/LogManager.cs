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
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Irseny.Core.Log {
	public class LogManager {
		static LogManager instance = null;
		static Thread instanceThread = null;
		static object instanceSync = new object();

		volatile bool running = false;
		ManualResetEvent pendingSignal = new ManualResetEvent(false);
		object pendingSync = new object();
		Queue<LogEntry> pendingMessages = new Queue<LogEntry>();
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
			Queue<LogEntry> toProcess;
			lock (pendingSync) {
				toProcess = pendingMessages;
				pendingMessages = new Queue<LogEntry>();
			}
			EventHandler<MessageEventArgs> handler;
			lock (availEventSync) {
				handler = messageAvailable;
			}
			while (toProcess.Count > 0) {
				LogEntry msg = toProcess.Dequeue();
				Debug.WriteLine(msg.ToDebugString());
				var args = new MessageEventArgs(msg);
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
		public void Log(LogEntry message) {
			if (message == null) throw new ArgumentNullException("message");
			lock (pendingSync) {
				pendingMessages.Enqueue(message);
			}
			pendingSignal.Set();
		}
		public void LogMessage(object source, string description, [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0) {
			Log(LogEntry.CreateMessage(source, description, sourceFile, sourceLine));
		}
		public void LogWarning(object source, string description, [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0) {
			Log(LogEntry.CreateWarning(source, description, sourceFile, sourceLine));
		}
		public void LogError(object source, string description, [CallerFilePath] string sourceFile = "", [CallerLineNumber] int sourceLine = 0) {
			Log(LogEntry.CreateError(source, description, sourceFile, sourceLine));
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
