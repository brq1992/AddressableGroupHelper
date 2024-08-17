

using AddressableAssetTool;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace com.igg.editor
{
    public class FeatureDependenciesScriptableObject : ScriptableObject
    {
        public List<AddressableAssetRule> AddrssableRules = new List<AddressableAssetRule>();
    }


    [CustomEditor(typeof(FeatureDependenciesScriptableObject))]
    [CanEditMultipleObjects]
    public class FeatureDependenciesScriptableObjectInspector : Editor
    {
        private static bool changed;
        private static string[] label = new string[] { AddressaableToolKey.FeatureDependenciesLabel };

        [MenuItem("Assets/AddressableAssetManager/Create/Addressable Feature Dependencies")]
        [MenuItem("Assets/Addressable Feature Dependencies")]
        public static void CreateAddressableAssetRule()
        {
            FeatureDependenciesScriptableObject newRule = CreateInstance<FeatureDependenciesScriptableObject>();
            string selectionpath = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                selectionpath = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(selectionpath))
                {
                    selectionpath = Path.GetDirectoryName(selectionpath);
                }
                break;
            }

            string unifiedPath = selectionpath.Replace("\\", "/");
            string[] directorys = unifiedPath.Split("/");

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("FeatureDependenciesLabel-");
            for (int i = 2; i < directorys.Length; i++)
            {
                if (i > 2)
                {
                    stringBuilder.Append("_");
                }
                stringBuilder.Append(directorys[i]);
            }
            stringBuilder.Append(".asset");
            string assetName = stringBuilder.ToString();
            string assetPath = Path.Combine(unifiedPath, assetName);
            //Debug.LogError("assetPath "+ assetPath);
            string newRuleFileName = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            //Debug.LogError("newRuleFileName " +newRuleFileName);
            //newRuleFileName = newRuleFileName.Replace("\\", "/");
            AssetDatabase.CreateAsset(newRule, newRuleFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.SetLabels(newRule, label);
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newRule;
            changed = true;
        }
    }
}