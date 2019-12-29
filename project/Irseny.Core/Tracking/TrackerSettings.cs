using System;
using System.Collections.Generic;
using Size = System.Drawing.Size;

namespace Irseny.Core.Tracking {
	public class TrackerSettings {
		Dictionary<TrackerProperty, double> fProps = new Dictionary<TrackerProperty, double>(16);
		Dictionary<TrackerProperty, int> iProps = new Dictionary<TrackerProperty, int>(16);

		public TrackerSettings() {
			// leave everything undefined
		}

		public TrackerSettings(TrackerSettings source) {
			if (source == null) throw new ArgumentNullException("source");
			foreach (var pair in source.fProps) {
				this.fProps.Add(pair.Key, pair.Value);
			}
			foreach (var pair in source.iProps) {
				this.iProps.Add(pair.Key, pair.Value);
			}
		}
		public int GetInteger(TrackerProperty prop, int fallback) {
			int result;
			if (!iProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetInteger(TrackerProperty prop, int value) {
			iProps[prop] = value;
		}
		public double GetDecimal(TrackerProperty prop, double fallback) {
			double result;
			if (!fProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetDecimal(TrackerProperty prop, double value) {
			fProps[prop] = value;
		}
		public void SetUndefined(TrackerProperty prop) {
			if (fProps.ContainsKey(prop)) {
				fProps.Remove(prop);
			}
			if (iProps.ContainsKey(prop)) {
				iProps.Remove(prop);
			}
		}
		public bool IsDefined(TrackerProperty prop) {
			return fProps.ContainsKey(prop) || iProps.ContainsKey(prop);
		}
	}
}