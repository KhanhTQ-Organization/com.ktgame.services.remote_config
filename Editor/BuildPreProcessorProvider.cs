using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace com.ktgame.services.remote_config.editor
{
    public class BuildPreProcessorProvider : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            AddScriptingDefineSymbol("FIREBASE_REMOTE_CONFIG");
        }

        private static void AddScriptingDefineSymbol(string define)
        {
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var allDefines = definesString.Split(';').ToList();
            if (!allDefines.Contains(define))
            {
                allDefines.Add(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));
            }
        }
    }
}