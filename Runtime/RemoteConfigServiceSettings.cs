using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using com.ktgame.core;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
#endif

namespace com.ktgame.services.remote_config
{
    public enum ValueType
    {
        Int,
        Float,
        String,
        Boolean,
    }

    [Serializable]
    public class ConfigData
    {
        [SerializeField] [FoldoutGroup("$name", expanded: false)]
        private string name;

        [SerializeField] [FoldoutGroup("$name", expanded: false)]
        private ValueType type;

        [SerializeField] [FoldoutGroup("$name", expanded: false)]
        private string defaultValue;

        public string Name => name;

        public ValueType Type => type;

        public string DefaultValue => defaultValue;
    }

    public class RemoteConfigServiceSettings : ServiceSettingsSingleton<RemoteConfigServiceSettings>
    {
        public override string PackageName => GetType().Namespace;

        [SerializeField] private bool autoFetching = true;

        [SerializeField] private List<ConfigData> configs;

        public bool AutoFetching => autoFetching;

        public List<ConfigData> Configs => configs ?? new List<ConfigData>();

#if UNITY_EDITOR
        [Button("Generate Config")]
        private void GenerateConfig()
        {
            if (configs.Count <= 0) return;
            var builder = new StringBuilder();
            builder.AppendFormat("namespace {0}", PackageName).Append("\n").Append("{").Append("\n");
            builder.Append("\t").Append("public class RemoteConfigKey").Append("\n");
            builder.Append("\t").Append("{").Append("\n");
            foreach (var config in configs)
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
#endif
    }
}