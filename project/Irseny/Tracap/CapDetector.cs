using System;
using System.Threading;
using System.Collections.Generic;

namespace Irseny.Tracap {
	public abstract class CapDetector : IHeadDetector {
		
		protected object outputEventSync = new object();
		protected object inputSync = new object();
		object runSync = new object();
		ManualResetEvent stepSignal = new ManualResetEvent(false);
		volatile bool running = false;
		Thread thread = null;
		Queue<Emgu.CV.Mat> pendingInput = new Queue<Emgu.CV.Mat>();
		protected event EventHandler<ImageEventArgs> inputProcessed;
		protected event EventHandler<EventArgs> positionDetected;

		public CapDetector() {
		}
		public event EventHandler<ImageEventArgs> InputProcessed {
			add {
				lock (outputEventSync) {
					inputProcessed += value;
				}
			}
			remove {
				lock (outputEventSync) {
					inputProcessed -= value;
				}
			}
		}
		public event EventHandler<EventArgs> PositionDetected {
			add {
				lock (outputEventSync) {
					positionDetected += value;
				} 
			}
			remove {
				lock (outputEventSync) {
					positionDetected -= value;
				}
			}
		}
		protected abstract void Step(Emgu.CV.Mat image);

		private void Run() {
			running = true;
			while (running) {
				stepSignal.WaitOne();
				stepSignal.Reset();
				ProcessPending();
			}
		}
		private void ProcessPending() {
			Queue<Emgu.CV.Mat> input;
			lock (inputSync) {
				input = pendingInput;
				pendingInput = new Queue<Emgu.CV.Mat>();
			}
			foreach (Emgu.CV.Mat image in input) {
				Step(image);
			}
		}
		public void Dispose() {
			// TODO: clear resources
		}

		public bool Start() {
			lock (runSync) {
				if (!running) {
					running = true;
					thread = new Thread(Run);
					thread.Start();
					return true;
				}
			}
			return false;
		}
		public bool Stop() {
			lock (runSync) {
				if (running) {
					running = false;
					stepSignal.Set();
					thread.Join();
					return true;
				}
			}
			return false;
		}
		public void QueueInput(Emgu.CV.Mat image) {
			if (image == null) throw new ArgumentNullException("image");
			lock (inputSync) {
				pendingInput.Enqueue(image);
			}
			stepSignal.Set();
		}
	}


}

