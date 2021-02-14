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
using Irseny.Core.Util;

namespace Irseny.Core.Tracking.HeadTracking {
	public abstract class HeadTracker : IPoseTracker {
		readonly object inputEventSync = new object();
		readonly object detectedEventSync = new object();
		readonly object executionEventSync = new object();
		event EventHandler<PositionDetectedEventArgs> positionDetected;
		event EventHandler inputAvailable;
		event EventHandler started;
		event EventHandler stopped;
		public abstract bool Running { get; }
		public event EventHandler<PositionDetectedEventArgs> PositionDetected {
			add {
				lock (detectedEventSync) {
					positionDetected += value;
				}
			}
			remove {
				lock (detectedEventSync) {
					positionDetected -= value;
				}
			}
		}
		public event EventHandler InputAvailable {
			add {
				lock (inputEventSync) {
					inputAvailable += value;
				}
			}
			remove {
				lock (inputEventSync) {
					inputAvailable -= value;
				}
			}
		}
		public event EventHandler Started {
			add {
				lock (executionEventSync) {
					started += value;
				}
			}
			remove {
				lock (executionEventSync) {
					started -= value;
				}
			}
		}
		public event EventHandler Stopped {
			add {
				lock (executionEventSync) {
					stopped += value;
				}
			}
			remove {
				lock (executionEventSync) {
					stopped -= value;
				}
			}
		}
		protected void OnStarted(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler handler;
			lock (executionEventSync) {
				handler = started;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected void OnStopped(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler handler;
			lock (executionEventSync) {
				handler = stopped;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected virtual void OnInputAvailable(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler handler;
			lock (inputEventSync) {
				handler = inputAvailable;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected virtual void OnPositionDetected(PositionDetectedEventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler<PositionDetectedEventArgs> handler;
			lock (detectedEventSync) {
				handler = positionDetected;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		public virtual void Dispose() {
			lock (detectedEventSync) {
				positionDetected = null;
			}
			lock (inputEventSync) {
				inputAvailable = null;
			}
			lock (executionEventSync) {
				started = null;
				stopped = null;
			}
		}
		public abstract bool Center();
		public abstract bool ApplySettings(EquipmentSettings settings);
		public abstract bool Start();
		public abstract bool Step();
		public abstract bool Stop();

	}
}
