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
using System.Diagnostics;

namespace Irseny.Core.Shared {
	/// <summary>
	/// Analyzation tool for average framerate and frametime variance.
	/// </summary>
	public class FrameRateAnalyzer {
		Queue<long> ticks;
		Stopwatch watch;
		long avgFrameTime;
		long combinedVariance;
		long lastTimeStamp;

		public FrameRateAnalyzer() {
			TimeSpan = 1000;
			AverageFrameRate = 0;
			AverageFrameTime = 0;
			StandardFrameTimeDeviation = 0;
			avgFrameTime = 0;
			combinedVariance = 0;
			lastTimeStamp = 0;
			ticks = new Queue<long>();
			watch = null;
		}
		/// <summary>
		/// Indicates whether the analyzation process has been started.
		/// </summary>
		/// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
		public bool Running {
			get { return watch != null; }
		}

		/// <summary>
		/// Gets the number of milliseconds over which frame times are averaged.
		/// </summary>
		/// <value>The time span.</value>
		public int TimeSpan { get; private set; }
		/// <summary>
		/// Gets the average number of frames per second.
		/// </summary>
		/// <value>The average framerate.</value>
		public int AverageFrameRate { get; private set; }
		/// <summary>
		/// Gets the average frame time.
		/// </summary>
		/// <value>The average frame time.</value>
		public int AverageFrameTime { get; private set; }
		/// <summary>
		/// Gets the standard frametime deviation.
		/// </summary>
		/// <value>The variance in the frametimes.</value>
		public int StandardFrameTimeDeviation { get; private set; }

		// TODO make private
		public long GetTimeStamp() {
			if (watch != null) {
				return watch.ElapsedMilliseconds;
			} else {
				return 0;
			}
		}
		/// <summary>
		/// Starts the capture process.
		/// </summary>
		/// <param name="timeSpan">Milliseconds to average over.</param>
		public void Start(int timeSpan) {
			if (timeSpan <= 0) throw new ArgumentOutOfRangeException("timeSpan");
			if (Running) {
				return;
			}
			TimeSpan = timeSpan;
			ticks.Clear();
			watch = new Stopwatch();
			watch.Start();
			lastTimeStamp = watch.ElapsedMilliseconds;
			avgFrameTime = 0;
			combinedVariance = 0;
		}
		/// <summary>
		/// Stops the catpure process.
		/// </summary>
		public void Stop() {
			if (!Running) {
				return;
			}
			watch.Stop();
			watch = null;
			ticks.Clear();
			avgFrameTime = 0;
			combinedVariance = 0;
			lastTimeStamp = 0;
		}
		/// <summary>
		/// Registers a tick.
		/// Updates the frame time measurement properties in the process.
		/// </summary>
		public void Tick() {
			if (!Running) {
				throw new InvalidOperationException("Can not register a frame before starting the capture process");
			}
			// add a new tick
			long timeStamp = watch.ElapsedMilliseconds;
			ticks.Enqueue(timeStamp);
			// remove outdated ticks
			long timeoutNo = 0;
			for (long stamp = ticks.Peek(); ticks.Count > 1; stamp = ticks.Peek()) {
				long elapsed = timeStamp - stamp;
				if (elapsed > TimeSpan) {
					ticks.Dequeue();
					timeoutNo += 1;
					//totalFrameTime -= AverageFrameTime;
					//combinedVariance -= StandardFrameTimeDeviation*StandardFrameTimeDeviation;
				} else {
					break;
				}
			}
			long frameNo = ticks.Count;
			long frameTime = timeStamp - lastTimeStamp;
			double blendFactor = 1.0/ticks.Count;
			avgFrameTime = (long)((1.0 - blendFactor)*avgFrameTime + blendFactor*frameTime);
			long firstTimeStamp = ticks.Peek();
			long timeSpan = timeStamp - firstTimeStamp;
			AverageFrameTime = (int)(timeSpan/frameNo);
			if (timeSpan > 0) {
				AverageFrameRate = (int)(frameNo*1000/timeSpan);
			}
			long variance = (AverageFrameTime - frameTime)*(AverageFrameTime - frameTime);
			combinedVariance = (long)((1.0 - blendFactor)*combinedVariance + blendFactor*variance);
			StandardFrameTimeDeviation = (int)Math.Sqrt(combinedVariance);
			lastTimeStamp = timeStamp;
		}
	}
}

