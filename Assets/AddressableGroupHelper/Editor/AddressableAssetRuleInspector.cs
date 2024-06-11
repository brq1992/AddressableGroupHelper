
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AddressableAssetTool
{
    [CustomEditor(typeof(AddressableAssetRule))]
    public class AddressableAssetRuleInspector : Editor
    {
        private static bool changed;

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

            Debug.LogError("selectionpath: " + selectionpath);
            string newRuleFileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(selectionpath, "New Addressable Asset Rule.asset"));
            newRuleFileName = newRuleFileName.Replace("\\", "/");
            AssetDatabase.CreateAsset(newRule, newRuleFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newRule;
            changed = true;
        }

        public override void OnInspectorGUI()
        {
            AddressableAssetRule t = (AddressableAssetRule)target;

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
                if (GUILayout.Button("Apply"))
                    Apply(t);
                EditorUtility.SetDirty(t);
            }
        }

        public GUIContent _AddressablePackMode = new GUIContent("Pack Mode", "Set how to pack the assets in this group into bundles");
        public GUIContent IsReadable = new GUIContent("IsUsed Enabled", "Whether this rule will be used!");

        private void DrawMeshSettings(AddressableAssetRule t)
        {
            t._isRuleUsed = EditorGUILayout.Toggle(IsReadable, t._isRuleUsed);
            t._packModel = (PackMode)
                EditorGUILayout.EnumPopup(_AddressablePackMode, t._packModel);
        }

        private void Apply(AddressableAssetRule t)
        {
            //todo: reset the addressable group, use a interface to inhericte this
            changed = false;
        }
    }

    enum PackMode
    {
        PackTogether,
        PackSeparately,
        PackTogetherByLable
    }
}