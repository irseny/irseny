﻿// This file is part of Irseny.
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

namespace Irseny.Main.Content {
	public abstract class ContentManager {

		public ContentManager() {
			Loaded = false;
			Modified = false;
			Settings = null;
		}
		public bool Loaded { get; protected set; }
		public bool Modified { get; protected set; }
		protected ContentManagerSettings Settings { get; set; }

		public void FlagManuallyModified() {
			Modified = true;
		}
		public virtual void Save() {
			throw new NotSupportedException();
		}
		public abstract void Load(ContentManagerSettings settings);
		public abstract void Reload();
		public abstract void Unload();
	}

	public abstract class ContentManager<T> : ContentManager {
		Dictionary<string, T> entries;

		public ContentManager() : base() {
			entries = new Dictionary<string, T>(64);
		}
		public T GetEntry(string name) {
			if (name == null) throw new ArgumentNullException("name");
			T result;
			if (entries.TryGetValue(name, out result)) {
				return result;
			} else {
				throw new KeyNotFoundException(name);
			}
		}
		public T GetEntry(string name, T fallback) {
			if (name == null) throw new ArgumentNullException("name");
			T result;
			if (entries.TryGetValue(name, out result)) {
				return result;
			} else {
				return fallback;
			}
		}
		public bool TryGetEntry(string name, out T result) {
			if (name == null) throw new ArgumentNullException("name");
			return entries.TryGetValue(name, out result);
		}
		public void SetEntry(string name, T value) {
			if (name == null) throw new ArgumentNullException("name");
			if (value == null) throw new ArgumentNullException("value");
			entries[name] = value;
			Modified = true;
		}
		public void ClearEntries() {
			entries.Clear();
			Modified = true;
		}
	}
}

