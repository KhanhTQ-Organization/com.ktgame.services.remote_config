using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using com.ktgame.core.editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace com.ktgame.services.remote_config.editor
{
    public class RemoteConfigEditor
    {
        private static bool _isInstalled = false;
        private KTSettingSO _setting;
        private RemoteConfigServiceSettings _parametersRc;
        
        private bool IsInstalledFirebase => DefineSymbolsEditor.HasDefineSymbol(DefineSymbolName.DS_FIREBASE_INSTALLED);
        public RemoteConfigEditor(KTSettingSO setting)
		{
			_setting = setting;
			_parametersRc = RemoteConfigServiceSettings.Instance;
		}

		[Title("Remote Configuration", "Manage your remote configuration keys and default values.", TitleAlignments.Centered, horizontalLine: true)]
		[InfoBox("Firebase is not installed. Please install Firebase SDK to use this service.", InfoMessageType.Warning, VisibleIf = "@!IsInstalledFirebase")]
		[PropertyOrder(-10)]
		[ShowInInspector, HideLabel, DisplayAsString(false)]
		private string _dummyInfo = "";

		[ShowIf("@!IsInstalledFirebase")]
		[BoxGroup("Installation", CenterLabel = true)]
		[PropertyOrder(-5)]
		[ShowInInspector]
		public string EdmVersion
		{
			get => _setting?.EdmVersion;
			set { if (_setting != null) _setting.EdmVersion = value; }
		}

		[ShowIf("@!IsInstalledFirebase")]
		[BoxGroup("Installation")]
		[PropertyOrder(-5)]
		[ShowInInspector]
		public string FirebaseVersion
		{
			get => _setting?.FirebaseVersion;
			set { if (_setting != null) _setting.FirebaseVersion = value; }
		}

		[ShowIf("@!IsInstalledFirebase")]
		[BoxGroup("Installation")]
		[PropertyOrder(-5)]
		[Button("Install / Refresh", ButtonSizes.Medium), GUIColor(0.2f, 0.6f, 1f)]
		private void HandleInstallation()
		{
			if (!_isInstalled)
			{
				PackageDependenceEditor.InstallPackage(VariableEditor.ExternalDependencyManagerName, _setting.EdmVersion);
				for (int i = 0; i < VariableEditor.FirebasePackageName.Length; i++)
				{
					PackageDependenceEditor.InstallPackage(VariableEditor.FirebasePackageName[i], _setting.FirebaseVersion);
				}
				_isInstalled = true;
			}
			else
			{
				PackageDependenceEditor.RefreshPackage();
				DefineSymbolsEditor.AddDefineSymbol(DefineSymbolName.DS_FIREBASE_INSTALLED);
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-1)]
		private void MarkDirtyOnGuiChange()
		{
			if (GUI.changed)
			{
				EditorUtility.SetDirty(_parametersRc);
				if (_setting != null) EditorUtility.SetDirty(_setting);
				AssetDatabase.SaveAssets();
			}
		}

		[PropertyOrder(0)]
		[ListDrawerSettings(CustomAddFunction = "CreateNewParameter")]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		[ShowInInspector]
		[LabelText("Parameters")]
		public List<ConfigData> Parameters
		{
			get => _parametersRc.Configs ?? new List<ConfigData>();
			set => _parametersRc.Configs = value;
		}

		private ConfigData CreateNewParameter()
		{
			return new ConfigData
			{
				Name = "",
				Type = ValueType.String,
				DefaultValue = ""
			};
		}

		[BoxGroup("Actions", CenterLabel = true)]
		[PropertyOrder(10)]
		[HorizontalGroup("Actions/Buttons")]
		[Button(SdfIconType.CodeSlash, "Generate Config"), GUIColor(0.2f, 0.8f, 0.2f)]
		private void GenerateConfig()
		{
			if (_parametersRc.Configs.Count <= 0) return;
			var builder = new StringBuilder();
			builder.AppendFormat("namespace {0}", _parametersRc.PackageName).Append("\n").Append("{").Append("\n");
			builder.Append("\t").Append("public class RemoteConfigKey").Append("\n");
			builder.Append("\t").Append("{").Append("\n");
			foreach (var config in _parametersRc.Configs)
			{
				builder.Append("\t\t").AppendFormat("public const string {0}", config.Name).Append(" = ").Append("\"").Append(config.Name).Append("\"")
					.Append(";").Append("\n");
			}

			builder.Append("\t").Append("}").Append("\n");
			builder.Append("}").Append("\n");
			var fileText = builder.ToString();

			var saveFolderPath = Path.Combine(Application.dataPath, "Scripts/Generated");
			var saveFilePath = Path.Combine(saveFolderPath, "RemoteConfigGenerate.cs");

			if (!Directory.Exists(saveFolderPath))
			{
				Directory.CreateDirectory(saveFolderPath);
			}

			if (File.Exists(saveFilePath))
			{
				File.Delete(saveFilePath);
			}

			if (File.Exists(saveFilePath + ".meta"))
			{
				File.Delete(saveFilePath + ".meta");
			}

			File.WriteAllText(saveFilePath, fileText, Encoding.UTF8);
			AssetDatabase.ImportAsset(saveFilePath);
			AssetDatabase.Refresh();
		}

		[HorizontalGroup("Actions/Buttons")]
		[PropertyOrder(10)]
		[Button(SdfIconType.ArrowRepeat, "Sync from Code"), GUIColor(0.2f, 0.6f, 1f)]
		private void SyncFromCode()
		{
			var typeName = $"{_parametersRc.PackageName}.RemoteConfigKey";
			var type = System.AppDomain.CurrentDomain.GetAssemblies()
				.Select(a => a.GetType(typeName))
				.FirstOrDefault(t => t != null);

			if (type == null)
			{
				Debug.LogWarning($"[RemoteConfig] Could not find class '{typeName}'. Make sure it exists and compiles.");
				return;
			}

			int addedCount = 0;
			var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
			foreach (var field in fields)
			{
				if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
				{
					string keyName = (string)field.GetRawConstantValue();
					if (!Parameters.Any(p => p.Name == keyName))
					{
						Parameters.Add(new ConfigData { Name = keyName, Type = ValueType.String, DefaultValue = "" });
						addedCount++;
					}
				}
			}

			if (addedCount > 0)
			{
				Debug.Log($"[RemoteConfig] Successfully synced {addedCount} new keys from code!");
				EditorUtility.SetDirty(_parametersRc);
			}
			else
			{
				Debug.Log("[RemoteConfig] All keys are already in sync.");
			}
		}

		[HorizontalGroup("Actions/Buttons")]
		[PropertyOrder(10)]
		[Button(SdfIconType.Terminal, "Log Config")]
		private void LogRemoteConfig()	
		{
			StringBuilder sb = new StringBuilder();
			foreach (var parameter in Parameters.OrderBy(x => x.Name))
			{
				sb.AppendLine($"{parameter.Name} : {parameter.DefaultValue}");
			}
			
			Debug.Log(sb.ToString()); // Changed to Log from LogError to avoid red spam
		}
    }
}
