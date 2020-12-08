using System;
using System.Collections.Generic;
using Irseny.Core.Util;

namespace Irseny.Core.Sensors.VideoCapture {
	/// <summary>
	/// Generic settings for sensor configuration.
	/// </summary>
	public class SensorSettings {
		Dictionary<SensorProperty, double> fProps;
		Dictionary<SensorProperty, int> iProps;

		public bool Running { get; set; }
		public string Name { get; set; }

		public SensorSettings() {
			// leave everything on auto
			fProps = new Dictionary<SensorProperty, double>(16);
			iProps = new Dictionary<SensorProperty, int>(16);
			Running = false;
			Name = string.Empty;
		}

		public SensorSettings(SensorSettings source) : this() {
			if (source == null) throw new ArgumentNullException("source");
			this.Running = source.Running;
			this.Name = source.Name;
			foreach (var pair in source.fProps) {
				this.fProps.Add(pair.Key, pair.Value);
			}
			foreach (var pair in source.iProps) {
				this.iProps.Add(pair.Key, pair.Value);
			}
		}
		public int GetInteger(SensorProperty prop, int fallback) {
			int result;
			if (!iProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetInteger(SensorProperty prop, int value) {
			iProps[prop] = value;
		}
		public double GetDecimal(SensorProperty prop, double fallback) {
			double result;
			if (!fProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetDecimal(SensorProperty prop, double value) {
			fProps[prop] = value;
		}
		public void SetAuto(SensorProperty prop) {
			if (fProps.ContainsKey(prop)) {
				fProps.Remove(prop);
			}
			if (iProps.ContainsKey(prop)) {
				iProps.Remove(prop);
			}
		}
		public bool IsAuto(SensorProperty prop) {
			return !fProps.ContainsKey(prop) && !iProps.ContainsKey(prop);
		}
		public JsonString ToJson() {
			var result = JsonString.CreateDict();
			{
				result.AddTerminal("type", StringifyTools.StringifyString("Webcam"));
				result.AddTerminal("running", StringifyTools.StringifyBool(Running));
				result.AddTerminal("name", StringifyTools.StringifyString(Name));
				foreach (SensorProperty prop in Enum.GetValues(typeof(SensorProperty))) {
					double fProp;
					int iProp;
					if (fProps.TryGetValue(prop, out fProp)) {
						result.AddTerminal(prop.ToString().Substring(0, 1).ToLower() + prop.ToString().Substring(1), StringifyTools.StringifyDouble(fProp));
					} else if (iProps.TryGetValue(prop, out iProp)) {
						result.AddTerminal(prop.ToString().Substring(0, 1).ToLower() + prop.ToString().Substring(1), StringifyTools.StringifyInt(iProp));
					}
				}
			}
			return result;
		}
	}
}
