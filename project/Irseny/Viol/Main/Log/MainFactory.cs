﻿using System;
using System.Collections.Generic;
namespace Irseny.Viol.Main.Log {
	public class MainFactory : InterfaceFactory {

		LinkedList<Irseny.Log.LogMessage> messages = new LinkedList<Irseny.Log.LogMessage>();
		bool[] filter;
		public MainFactory() : base() {
			filter = new bool[Enum.GetValues(typeof(Irseny.Log.MessageType)).Length];
			for (int i = 0; i < filter.Length; i++) {
				filter[i] = true;
			}
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("Log"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Log");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.PackStart(boxMain);
			Irseny.Log.LogManager.Instance.MessageAvailable += AddMessage;
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
			Irseny.Log.LogManager.Instance.MessageAvailable -= AddMessage;
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public void ClearMessages() {
			messages.Clear();
		}
		private void AddMessage(object sender, Irseny.Log.MessageEventArgs args) {
			Invoke(delegate {
				AddMessage(args.Message);
			});
		}
		public void AddMessage(Irseny.Log.LogMessage message) {
			messages.AddLast(message);
			Irseny.Log.LogMessage remaining = Filter(message);
			if (remaining != null) {
				var target = Container.GetWidget<Gtk.TextView>("txt_Log").Buffer;
				string text = remaining.ToString();
				var atEnd = target.EndIter;
				target.Insert(ref atEnd, text);
			}
		}
		private void UpdateFilter() {
			var cbxMessage = Container.GetWidget<Gtk.CheckButton>("cbx_Message");
			var cbxWarning = Container.GetWidget<Gtk.CheckButton>("cbx_Warning");
			var cbxError = Container.GetWidget<Gtk.CheckButton>("cbx_Error");
			filter[(int)Irseny.Log.MessageType.Signal] = cbxMessage.Active;
			filter[(int)Irseny.Log.MessageType.Warning] = cbxWarning.Active;
			filter[(int)Irseny.Log.MessageType.Error] = cbxError.Active;
		}
		public Irseny.Log.LogMessage Filter(Irseny.Log.LogMessage message) {
			if (message != null && filter[(int)message.MessageType]) {
				return message;
			} else {
				return null;
			}
		}
		public void Rewrite() {
			var target = Container.GetWidget<Gtk.TextView>("txt_Log").Buffer;
			target.Clear();
			foreach (Irseny.Log.LogMessage message in messages) {
				Irseny.Log.LogMessage remaining = Filter(message);
				if (remaining != null) {
					string text = remaining.ToString();
					var atEnd = target.EndIter;
					target.Insert(ref atEnd, text);
				}
			}
		}
	}
}
