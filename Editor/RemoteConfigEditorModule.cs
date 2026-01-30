using com.ktgame.core.editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.ktgame.services.remote_config.editor
{
	[InitializeOnLoad]
	public class RemoteConfigEditorModule : IEditorDirtyHandler, IMenuTreeExtension
	{
		static RemoteConfigEditorModule()
		{
			var module = new RemoteConfigEditorModule();
			EditorDirtyRegistry.Register(module);
			MenuTreeExtensionRegistry.Register(module);
		}
		
		public void SetDirty()
		{
			var instance = RemoteConfigServiceSettings.Instance;
			if (instance != null)
			{
				EditorUtility.SetDirty(instance);
			}
		}
		public void BuildMenu(OdinMenuTree tree)
		{
			tree.Add("Remote Config", new RemoteConfigEditor(KTWindow.Setting), KTEditor.GetIconComponent("firebase"));
		}
	}
}
