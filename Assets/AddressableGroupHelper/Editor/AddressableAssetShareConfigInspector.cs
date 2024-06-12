
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
        List<GroupShareData> list = new List<GroupShareData>();
        private bool _includeIndirect = true;
        private GroupShareDataAnalyzer analyzer;
        private Dictionary<string, ShareEntry> data;
        private bool showRefer;


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
            AddressableAssetShareConfig t = (AddressableAssetShareConfig)target;

            var rules = t.AssetbundleGroups;

            if(rules.Count < 2)
            {
                return;
            }
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            analyzer = new GroupShareDataAnalyzer();

            foreach (var rule in rules)
            {
                //Debug.LogError(rule1.name);
                var group = setting.FindGroup(rule.name);

                GroupShareData groupShareData = new GroupShareData(group);

                

                //Debug.LogError(group.name);
                foreach (var item in group.entries)
                {
                    var guid = item.guid;
                    var paths = AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                    foreach (var path in paths)
                    {
                        //Debug.LogError("add " + path);
                        groupShareData.AddAssetPath(path);
                        analyzer.AddAssetPath(path, item);
                    }
                }

               
                list.Add(groupShareData);
            }

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

            EditorGUILayout.LabelField("Shared Assets");
            if (GUILayout.Button((showRefer ? "Hide Refercence" : "Show Refercence")))
            {
                showRefer = !showRefer;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10f);
            foreach (var item in data)
            {
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