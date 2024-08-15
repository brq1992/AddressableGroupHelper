using AddressableAssetTool;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.igg.editor
{
    internal class RuleIONode
    {
        public bool IsValid { get { return !string.IsNullOrEmpty(_directoryName); } }
        internal string Name { get { return _directoryName; } }

        private string _directoryName;
        private readonly Dictionary<string, RuleIONode> _childDic = new Dictionary<string, RuleIONode>();
        private readonly List<AddressableAssetRule> _rules = new List<AddressableAssetRule>();
        private int _depth;
        private bool _foldout = false;


        public RuleIONode(string directoryName)
        {
            this._directoryName = directoryName;
        }

        public RuleIONode()
        {

        }

        internal RuleIONode GetNode(string[] direcoties, int depth)
        {
            if(direcoties.Length - 1 == depth && direcoties[depth].Equals(Name))
            {
                return this;
            }

            _depth = depth;

            RuleIONode node = null;
            int childDepth = depth + 1;
            if (_childDic.TryGetValue(direcoties[childDepth], out node))
            {
                return node.GetNode(direcoties, childDepth);
            }
            else
            {
                RuleIONode newNode = new RuleIONode(direcoties[childDepth]);
                _childDic.Add(direcoties[childDepth], newNode);
                return newNode.GetNode(direcoties, childDepth);
            }
        }

        internal void AddRule(AddressableAssetRule rule)
        {
            if(!_rules.Contains(rule))
            {
                _rules.Add(rule);
            }
        }

        internal Dictionary<string, RuleIONode> GetNodes()
        {
            return _childDic;
        }

        internal List<AddressableAssetRule> GetRules()
        {
            return _rules;
        }

  
        internal void DrawGUI()
        {
            //_foldout = EditorGUILayout.Foldout(_foldout, Name);


            //foreach (var item in _rules)
            //{
            //    EditorGUILayout.BeginHorizontal();


            //    EditorGUILayout.LabelField(item.name);

            //    bool value = EditorGUILayout.Toggle(item.IsRuleUsed);
            //    item.IsRuleUsed = value;

            //    EditorGUILayout.EndHorizontal();
            //}

            _foldout = true;

            if (_foldout)
            {
                EditorGUILayout.BeginVertical();

                foreach (var item in _childDic)
                {
                    item.Value.DrawGUI();
                }

                foreach (var item in _rules)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(item.name, EditorStyles.label, GUILayout.MinWidth(500), GUILayout.MaxWidth(700)))
                    {
                        EditorGUIUtility.PingObject(item);
                    }
                    //EditorGUILayout.LabelField(item.name);

                    bool value = EditorGUILayout.Toggle(item.IsRuleUsed);
                    if (value != item.IsRuleUsed)
                    {
                        EditorUtility.SetDirty(item);
                    }
                    item.IsRuleUsed = value;

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }



        }
    }
}