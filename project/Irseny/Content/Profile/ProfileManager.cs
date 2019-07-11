using System;
using System.IO;
using System.Xml;

namespace Irseny.Content.Profile {
	public class ProfileManager : ContentManager {
		ContentManagerSettings settings = null;

		public ProfileManager() {
		}

		public override void Load(ContentManagerSettings settings) {
			this.settings = settings;
			Loaded = true;
		}
		public override void Reload() {

		}
		public override void Unload() {

			Loaded = false;
		}
		public void SafeActiveProfile(SetupProfile profile) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (!Loaded) throw new InvalidOperationException("not loaded");
			string filePath = Path.Combine(settings.ResourceDirectory, "profiles", profile.Name + ".xml");

			using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write)) {
				XmlDocument document = new XmlDocument();
				XmlDeclaration decl = document.CreateXmlDeclaration("1.0", "UTF-8", null);
				document.InsertBefore(decl, document.DocumentElement);
				XmlElement root = document.CreateElement("Profile");
				document.AppendChild(root);


				XmlNode capture = new CaptureProfileWriter().Write(profile, document);
				root.AppendChild(capture);
				document.Save(stream);
			}

		}
		public SetupProfile LoadDefaultProfile() {
			if (!Loaded) throw new InvalidOperationException("not loaded");
			string filePath = Path.Combine(settings.ResourceDirectory, "profiles", "Default.xml");
			try {
				var result = new SetupProfile("Default");
				using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read)) {
					XmlDocument document = new XmlDocument();
					document.Load(stream);

					foreach (XmlNode root in document.DocumentElement.ChildNodes) {
						if (root.Name.Equals("VideoCapture")) {
							if (!new CaptureProfileReader().Read(result, root)) {
								return null;
							}
						} else if (root.Name.Equals("VirtualDevice")) {
							// TODO: complete
						}
					}

				}
				return result;
			} catch (FileNotFoundException) {
				return null;
			}
		}
	}
}
