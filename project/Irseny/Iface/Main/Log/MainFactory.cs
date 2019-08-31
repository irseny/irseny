using System;
using System.Collections.Generic;
using Irseny.Log;
using Irseny.Content;

namespace Irseny.Iface.Main.Log {
	public class MainFactory : InterfaceFactory {

		LinkedList<LogEntry> messages = new LinkedList<LogEntry>();
		bool[] filter;
		public MainFactory() : base() {
			filter = new bool[Enum.GetValues(typeof(MessageType)).Length];
			for (int i = 0; i < filter.Length; i++) {
				filter[i] = true;
			}
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("Log");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Log");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.PackStart(boxMain, true, true, 0);
			LogManager.Instance.MessageAvailable += MessageAdded;
			var btnClear = Container.GetWidget<Gtk.Button>("btn_Clear");
			btnClear.Clicked += delegate {
				ClearMessages();
				Rewrite();
			};
			foreach (string name in new string[] { "cbx_Message", "cbx_Warning", "cbx_Error" }) {
				var cbxMessage = Container.GetWidget<Gtk.CheckButton>(name);
				cbxMessage.Clicked += delegate {
					UpdateFilter();
					Rewrite();
				};
			}
			UpdateFilter();
			ClearMessages();
			return true;
		}
		protected override bool DisconnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Log");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			LogManager.Instance.MessageAvailable -= MessageAdded;
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public void ClearMessages() {
			messages.Clear();
		}
		private void MessageAdded(object sender, MessageEventArgs args) {
			Invoke(delegate {
				AddMessage(args.Message);
			});
		}
		public void AddMessage(LogEntry message) {
			if (!Initialized) {
				return;
			}
			messages.AddLast(message);
			LogEntry remaining = Filter(message);
			if (remaining != null) {
				var target = Container.GetWidget<Gtk.TextView>("txt_Log").Buffer;
				string text = remaining.ToDescription();
				var atEnd = target.EndIter;
				target.Insert(ref atEnd, text);
				target.Insert(ref atEnd, "\n");
			}
		}
		private void UpdateFilter() {
			var cbxMessage = Container.GetWidget<Gtk.CheckButton>("cbx_Message");
			var cbxWarning = Container.GetWidget<Gtk.CheckButton>("cbx_Warning");
			var cbxError = Container.GetWidget<Gtk.CheckButton>("cbx_Error");
			filter[(int)MessageType.Signal] = cbxMessage.Active;
			filter[(int)MessageType.Warning] = cbxWarning.Active;
			filter[(int)MessageType.Error] = cbxError.Active;
		}
		public LogEntry Filter(LogEntry message) {
			if (message != null && filter[(int)message.MessageType]) {
				return message;
			} else {
				return null;
			}
		}
		public void Rewrite() {
			var target = Container.GetWidget<Gtk.TextView>("txt_Log").Buffer;
			target.Clear();
			foreach (LogEntry message in messages) {
				LogEntry remaining = Filter(message);
				if (remaining != null) {
					string text = remaining.ToDescription();
					var atEnd = target.EndIter;
					target.Insert(ref atEnd, text);
					target.Insert(ref atEnd, "\n");
				}
			}
		}
	}
}

