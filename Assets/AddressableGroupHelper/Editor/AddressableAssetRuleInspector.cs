
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AddressableAssetTool
{
    [CustomEditor(typeof(AddressableAssetRule))]
    public class AddressableAssetRuleInspector : Editor
    {
        private GUIContent _addressablePackModeGUIContent = new GUIContent("Pack Mode", "Set how to pack the assets in this group into bundles. If Pack Together, " +
    "assets in this folder will be put into the group with the folder structure preserved. If Pack Separately, assets will be put into the group separately.");
        private GUIContent isReadableContent = new GUIContent("IsUsed Enabled", "Whether this rule will be used!");
        private static bool changed;
        private static string[] label = new string[] { AddressaableToolKey.ScriptObjAssetLabel };

        [MenuItem("Assets/AddressableAssetManager/Create/Addressable Asset Rules")]
        public static void CreateAddressableAssetRule()
        {
            AddressableAssetRule newRule = CreateInstance<AddressableAssetRule>();
            newRule.ApplyDefaults();
            string selectionpath = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                selectionpath = AssetDatabase.GetAssetPath(obj);
                //Debug.Log("selectionpath in foreach: " + selectionpath);
                if (File.Exists(selectionpath))
                {
                    //Debug.Log("File.Exists: " + selectionpath);
                    selectionpath = Path.GetDirectoryName(selectionpath);
                }
                break;
            }

            //Debug.LogError("selectionpath: " + selectionpath);
            //var fullPath = Path.GetFullPath(selectionpath);
            string unifiedPath = selectionpath.Replace("\\", "/");
            string[] directorys = unifiedPath.Split("/");

            StringBuilder stringBuilder = new StringBuilder();
            for(int i =2;i<directorys.Length;i++)
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

        public override void OnInspectorGUI()
        {
            AddressableAssetRule t = (AddressableAssetRule)target;
            t.UpdateData();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AddressableAssetRules");
            EditorGUILayout.EndHorizontal();


            DrawMeshSettings(t);

            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }

            if (changed)
            {
                //if (GUILayout.Button("Apply"))
                //    Apply(t);
                EditorUtility.SetDirty(t);
                //AssetDatabase.SaveAssets();
            }
        }



        private void DrawMeshSettings(AddressableAssetRule t)
        {
            t.IsRuleUsed = EditorGUILayout.Toggle(isReadableContent, t.IsRuleUsed);
            t.PackModel = (PackMode)
                EditorGUILayout.EnumPopup(_addressablePackModeGUIContent, t.PackModel);

            System.Collections.Generic.List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup> addressableAssetGroups = t.addressableAssetGroups;
        }

        private void Apply(AddressableAssetRule t)
        {
            //todo: reset the addressable group, use a interface to inhericte this
            changed = false;
        }
    }

    public enum PackMode
    {
        PackTogether,
        PackSeparately
    }
}