using AddressableAssetTool;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.igg.editor
{
    public class AddressableRuleWindow : IIGGTool
    {
        private RuleIONode _root;

        public string ToolName => "Addressable Rule Manager";

        public IGGToolCategory Category => IGGToolCategory.Build;

        public string Description => "Manage and view AssetRules in Asset folder";

        public void Dispose()
        {
        }

        public void Init(IGGToolsWindowEditor window)
        {
            string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
            var rulesGUID = AssetDatabase.FindAssets(ruleFilter,
                new[] { AddressaableToolKey.RuleSearchPath });

            //sort by folder

            List<RulePathData> guids = new List<RulePathData>();

            foreach (var ruleGuid in rulesGUID)
            {
                guids.Add(new RulePathData(ruleGuid, AssetDatabase.GUIDToAssetPath(ruleGuid)));
            }

            guids.Sort((x, y) =>
            {
                int lengthX = x.Path.Split("/").Length;
                int lengthY = y.Path.Split("/").Length;
                return lengthX.CompareTo(lengthY);
            });

            _root = new RuleIONode("Asset");
            foreach (var item in guids)
            {
                var path = item.Path;
                var dicPath = Path.GetDirectoryName(path);
                //IGGDebug.LogError(dicPath);
                var direcoties = dicPath.Split("\\");
                int depth = 0;

                var node = _root.GetNode(direcoties, depth);
                if (node.IsValid)
                {
                    AddressableAssetRule rule = AssetDatabase.LoadAssetAtPath(item.Path, typeof(AddressableAssetRule)) as AddressableAssetRule;
                    if (rule != null)
                    {
                        node.AddRule(rule);
                    }
                }


                //if (root.Name.Equals(direcoties[depth]))
                //{
                //    var node = root.GetNode(direcoties, depth+1);
                //    if(node.IsValid)
                //    {
                //        AddressableAssetRule rule = AssetDatabase.LoadAssetAtPath(item.Path ,typeof(AddressableAssetRule)) as AddressableAssetRule;
                //        if(rule !=null)
                //        {
                //            node.AddRule(rule);
                //        }
                //    }
                //}

            }


            int a = 0;


        }

        bool foldout = false;
        private Vector2 _scrollPos;
        private bool _allValue = false;

        public void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            {
                bool value = EditorGUILayout.Toggle(_allValue);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                _root.DrawGUI();
                if(value != _allValue)
                {
                    _root.ChangeValue(value);
                    _allValue = value;
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
    }

    public class AddressableRuleBuildManager : IIGGTool
    {
        public string ToolName => "Build Asset Rule To Group";

        public IGGToolCategory Category => IGGToolCategory.Build;

        public string Description => "Applay asset rule and build addressable group";

        public void Dispose()
        {
        }

        public void Init(IGGToolsWindowEditor window)
        {
        }

        public void OnGUI()
        {
        }
    }
}
