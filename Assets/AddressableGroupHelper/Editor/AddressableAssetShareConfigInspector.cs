
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace AddressableAssetTool
{
    [CustomEditor(typeof(AddressableAssetShareConfig))]
    public class AddressableAssetShareConfigInspector : Editor
    {
        private bool _includeIndirect = true;
        private GroupShareDataAnalyzer analyzer;
        private Dictionary<string, ShareEntry> data;
        private bool showRefer;
        private bool _showIndirectReferencies;
        private AddressableAssetShareConfig t;

        [MenuItem("Assets/AddressableAssetManager/Create/Addressable Asset ShareConfig")]
        public static void CreateShareConfig()
        {
            var newRule = CreateInstance<AddressableAssetShareConfig>();
            //newRule.ApplyDefaults();
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
            string[] directorys = selectionpath.Split("\\");

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("AssetShareConfig");
            stringBuilder.Append(".asset");
            string assetName = stringBuilder.ToString();
            string newRuleFileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(selectionpath, assetName));
            Debug.LogError(newRuleFileName);
            newRuleFileName = newRuleFileName.Replace("\\", "/");
            AssetDatabase.CreateAsset(newRule, newRuleFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newRule;
        }

        private void OnEnable()
        {
            t = (AddressableAssetShareConfig)target;

            var rules = t.AssetbundleGroups;

            if(rules.Count < 1)
            {
                return;
            }
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;

            analyzer = GroupShareDataAnalyzerFactory.GetAnalyzer(t);

            data = analyzer.GetColloction();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(analyzer == null)
            {
                return;
            }



            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Shared Assets", new GUILayoutOption[] { GUILayout.Width(80), GUILayout.MaxWidth(100f) });

            EditorGUILayout.LabelField(" Count "+ data.Count, new GUILayoutOption[] { GUILayout.Width(80), GUILayout.MaxWidth(100f) });

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(" ShowIndirect", new GUILayoutOption[] { GUILayout.Width(80), GUILayout.MaxWidth(100f) });

            bool showIndirectReferencies = EditorGUILayout.Toggle(t.ShowIndirectReferencies,
                new GUILayoutOption[] { GUILayout.Width(20), GUILayout.MaxWidth(80) });

            if (showIndirectReferencies != t.ShowIndirectReferencies)
            {
                t.ShowIndirectReferencies = showIndirectReferencies;
                analyzer.ShowIndirectReferencies = showIndirectReferencies;
                analyzer.ClearData();
                data = analyzer.GetColloction();
            }

            if (GUILayout.Button("Refresh", new GUILayoutOption[] { GUILayout.Width(80), GUILayout.MaxWidth(100f) }))
            {
                analyzer = GroupShareDataAnalyzerFactory.GetAnalyzer(t);

                data = analyzer.GetColloction();
            }

            if (GUILayout.Button(showRefer ? "HideRefere" : "ShowRefer", new GUILayoutOption[] { GUILayout.Width(80), GUILayout.MaxWidth(100f) }))
            {
                showRefer = !showRefer;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);
            foreach (var item in data)
            {
                //Debug.LogError(" set " + item.Key);
                //EditorGUILayout.BeginVertical();
                //EditorGUILayout.LabelField(item.Key);
                //EditorGUILayout.LabelField("ReferenceBy: ");
                if (GUILayout.Button(item.Key, EditorStyles.label))
                {
                    Object source = AssetDatabase.LoadAssetAtPath(item.Key, typeof(Object));
                    EditorGUIUtility.PingObject(source);
                }

                if (!showRefer)
                {
                    EditorGUILayout.Space(3f);
                    continue;
                }
                var entries = item.Value.Entries();

                EditorGUILayout.LabelField("ReferenceBy: ");

                foreach (var entry in entries)
                {
                    EditorGUILayout.BeginHorizontal();
                    //EditorGUILayout.ObjectField(source, typeof(Object), true);
                    
                    if (GUILayout.Button(entry.AssetPath, EditorStyles.label))
                    {
                        Object source = AssetDatabase.LoadAssetAtPath(entry.AssetPath, typeof(Object));
                        EditorGUIUtility.PingObject(source);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                //EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5f);
            }
        }




        
    }
}