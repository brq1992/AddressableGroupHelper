
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace AddressableAssetTool
{
    [CustomEditor(typeof(AddressableAssetRule))]
    [CanEditMultipleObjects]
    public class AddressableAssetRuleInspector : Editor
    {
        private GUIContent _addressablePackModeGUIContent = new GUIContent("Pack Mode", "Set how to pack the assets in this group into bundles. If Pack Together, " +
    "assets in this folder will be put into the group with the folder structure preserved. If Pack Separately, assets will be put into the group separately.");
        private GUIContent _compressGUIContent = new GUIContent("Bundle CompressionMode", "Set whether it need be compressed!");
        private GUIContent isUseableContent = new GUIContent("IsUsed", "Whether this rule will be used!");
        private GUIContent isPullEntryByFolderContent = new GUIContent("PackGroupByFolder", "If you enable it, all the files will be pack into a group by folders");
        private static bool changed;
        private static string[] label = new string[] { AddressaableToolKey.ScriptObjAssetLabel };

        [MenuItem("Assets/AddressableAssetManager/Create/Addressable Asset Rules")]
        [MenuItem("Assets/Addressable Asset Rules")]
        public static void CreateAddressableAssetRule()
        {
            AddressableAssetRule newRule = CreateInstance<AddressableAssetRule>();
            newRule.ApplyDefaults();
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

            CreateRule(newRule, selectionpath);
        }

        public static void CreateRule(string selectionpath, BundledAssetGroupSchema.BundlePackingMode packModel = BundledAssetGroupSchema.BundlePackingMode.PackTogether)
        {
            AddressableAssetRule newRule = CreateInstance<AddressableAssetRule>();
            newRule.ApplyDefaults();
            newRule.PackModel = packModel;
            CreateRule(newRule, selectionpath);
        }

        public static void CreateRule(AddressableAssetRule newRule, string selectionpath)
        {
            string unifiedPath = selectionpath.Replace("\\", "/");
            string[] directorys = unifiedPath.Split("/");

            StringBuilder stringBuilder = new StringBuilder();
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
            string newRuleFileName = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(newRule, newRuleFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.SetLabels(newRule, label);
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newRule;
            changed = true;
        }

        private string[] LabelsContent = null;

        private int _currentIndex = -1;

        private void OnEnable()
        {
            var labels = AddressableAssetSettingsDefaultObject.Settings.GetLabels();
            LabelsContent = new string[labels.Count];
            for (int i = 0; i < labels.Count; i++)
            {
                LabelsContent[i] = labels[i];
            }
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
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                var group = settings.FindGroup(t.name);
                if (group != null)
                {
                    var schema = group.GetSchema<BundledAssetGroupSchema>();
                    schema.BundleMode = t.PackModel;
                    foreach (var entry in group.entries)
                    {
                        entry.SetLabel(t.Lable, true);
                    }

                    //settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
                    //AssetDatabase.SaveAssets();
                }
                //if (GUILayout.Button("Apply"))
                //    Apply(t);
                EditorUtility.SetDirty(t);
                //AssetDatabase.SaveAssets();
            }
        }


        private void DrawMeshSettings(AddressableAssetRule t)
        {
            t.IsRuleUsed = EditorGUILayout.Toggle(isUseableContent, t.IsRuleUsed);
            t.AddEntryByFolder = EditorGUILayout.Toggle(isPullEntryByFolderContent, t.AddEntryByFolder);
            if(t.AddEntryByFolder)
            {
                GUI.enabled = false;
                t.PackModel = (BundledAssetGroupSchema.BundlePackingMode)EditorGUILayout.EnumPopup(_addressablePackModeGUIContent, t.PackModel);
                GUI.enabled = true;
            }
            else
                t.PackModel = (BundledAssetGroupSchema.BundlePackingMode)EditorGUILayout.EnumPopup(_addressablePackModeGUIContent, t.PackModel);

            t.BundleCompressionMode = (AddressableAssetRule.CompressionMode)EditorGUILayout.EnumPopup(_compressGUIContent, t.BundleCompressionMode);
            int index = -1;
            for (int i = 0; i < LabelsContent.Length; i++)
            {
                if (t.Lable.Equals(LabelsContent[i]))
                {
                    index = i;
                    break;
                }
            }
            _currentIndex = EditorGUILayout.Popup(index, LabelsContent);
            if (_currentIndex != -1)
                t.Lable = LabelsContent[_currentIndex];
            //System.Collections.Generic.List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup> addressableAssetGroups = t.addressableAssetGroups;
        }

        private void Apply(AddressableAssetRule t)
        {
            //todo: reset the addressable group, use a interface to inhericte this
            changed = false;
        }
    }

    //public enum PackMode
    //{
    //    PackTogether,
    //    PackSeparately
    //}
}