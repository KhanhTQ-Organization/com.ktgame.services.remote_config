using UnityEditor;

namespace com.ktgame.services.remote_config.editor
{
	public class PackageInstaller
	{
		private const string PackageName = "com.ktgame.services.remote-config";
        
		[MenuItem("Ktgame/Services/Settings/Remote Config")]
		private static void SelectionSettings()
		{
			Selection.activeObject = RemoteConfigServiceSettings.Instance;
		}
	}
}
