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

namespace Irseny.Core.Tracking {
	public interface IPoseTracker : IDisposable {
		/// <summary>
		/// Occurs when a pose has been detected.
		/// </summary>
		event EventHandler<PositionDetectedEventArgs> PositionDetected;
		/// <summary>
		/// Occurs when the algorithm has data to process.
		/// </summary>
		event EventHandler InputAvailable;
		/// <summary>
		/// Occurs when the detection process is started.
		/// </summary>
		event EventHandler Started;
		/// <summary>
		/// Occurs when the detection process is stopped.
		/// </summary>
		event EventHandler Stopped;
		/// <summary>
		/// Indicates whether the detector is currently active.
		/// </summary>
		bool Running {  get; }
		/// <summary>
		/// Uses the current pose as tracking origin.
		/// </summary>
		/// <returns></returns>
		bool Center();
		/// <summary>
		/// Applies tracker settings.
		/// </summary>
		/// <param name="settings">Settings.</param>
		/// <returns>Whether the application was successful.</returns>
		bool ApplySettings(EquipmentSettings settings);
		/// <summary>
		/// Starts the detection process.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		bool Start();
		/// <summary>
		/// Executes one detection iteration.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		bool Step();
		/// <summary>
		/// Stops the detection process.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		bool Stop();
	}
}
