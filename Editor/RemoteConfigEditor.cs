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

		[PropertyOrder(-1)]
		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			if (!IsInstalledFirebase)
				RenderIfNotInstalled();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(_parametersRc);
				AssetDatabase.SaveAssets();
			}
		}

		private void RenderIfNotInstalled()
		{
			EditorGUILayout.LabelField("Firebase is not installed.");
			EditorGUILayout.LabelField("Please install Firebase SDK to use this service.");

			_setting.EdmVersion = EditorGUILayout.TextField("EDM4U Version", _setting.EdmVersion);
			_setting.FirebaseVersion = EditorGUILayout.TextField("Firebase Version", _setting.FirebaseVersion);
			EditorGUILayout.Space();

			if (!_isInstalled)
			{
				if (GUILayout.Button("Install"))
				{
					PackageDependenceEditor.InstallPackage(VariableEditor.ExternalDependencyManagerName,
						_setting.EdmVersion);
					for (int i = 0; i < VariableEditor.FirebasePackageName.Length; i++)
					{
						PackageDependenceEditor.InstallPackage(VariableEditor.FirebasePackageName[i],
							_setting.FirebaseVersion);
					}

					_isInstalled = true;
				}
			}
			else
			{
				if (GUILayout.Button("Refresh Package"))
				{
					PackageDependenceEditor.RefreshPackage();
					DefineSymbolsEditor.AddDefineSymbol(DefineSymbolName.DS_FIREBASE_INSTALLED);
				}
			}

			EditorGUILayout.Space();
		}

		[ListDrawerSettings(CustomAddFunction = "CreateNewParameter")]
		//[HideReferenceObjectPicker]
		//[InlineProperty]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		[ShowInInspector]
		[LabelText("Remote Config Parameters")]
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

		[Title("")]
		[PropertyOrder(1)]
		[Button("Log Remote Config")]
		private void LogRemoteConfig()	
		{
			StringBuilder sb = new StringBuilder();
			foreach (var parameter in Parameters.OrderBy(x => x.Name))
			{
				sb.AppendLine($"{parameter.Name} : {parameter.DefaultValue}");
			}
			
			Debug.LogError(sb.ToString());
		}
		
		[Button("Generate Config")]
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

		[Title("Generate Key")] [PropertyOrder(1)] [HorizontalGroup("GenerateEnum")] [HideLabel]
		public string NewKey;

		[Title("")]
		[PropertyOrder(1)]
		[HorizontalGroup("GenerateEnum", Width = 25)]
		[Button(SdfIconType.Plus, "")]
		private void AddNewKey()
		{
			if (NewKey == null || NewKey.Equals(string.Empty))
				return;

			// if (Enum.TryParse(NewKey, out RCParameterDataType key))
			// 	return;

			EnumGenerator.Generate("RCParameterKey", NewKey);
		}
    }
}
