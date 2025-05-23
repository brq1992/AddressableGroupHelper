
using com.igg.editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AddressableAssetTool.Graph
{
    public partial class ResourceNode
    {
        //move to main node?
        public HashSet<string> Dependencies { get; private set; }
        public Dictionary<AddressableAssetEntry, HashSet<string>> AddressableAssetEntries { get; private set; }
        public AddressableAssetRule AddressableAssetRule { get; private set; }

        private string dependencyRule;

        public void AddDependencies(string[] dependenciesAfterFilter)
        {
            if(dependenciesAfterFilter == null)
            {
                return;
            }
            for(int i =0; i< dependenciesAfterFilter.Length; i++)
            {
                if (!Dependencies.Contains(dependenciesAfterFilter[i]))
                {
                    //var guid = AddressabelUtilities.GetAssetGuid<FeatureDependenciesScriptableObject>(dependenciesAfterFilter[i]);
                    //if (string.IsNullOrEmpty(guid))
                    //{
                    //    com.igg.core.IGGDebug.Log("Can't find Dependencies");
                    //}
                    //else
                    //{
                    //    if (!AddressabelRuleDependencyCheck.CheckDependencyLegal(dependencyRule, guid))
                    //    {
                    //        com.igg.core.IGGDebug.LogError(AssetDatabase.GUIDToAssetPath(dependencyRule) + " ilegal refer " + AssetDatabase.GUIDToAssetPath(guid));
                    //    }
                    //}
                    Dependencies.Add(dependenciesAfterFilter[i]);
                }
            }
        }

        public void AddEntry(AddressableAssetEntry item, string[] dependencies)
        {
            if (!AddressableAssetEntries.ContainsKey(item))
            {
                HashSet<string> depnHS = new HashSet<string>();
                for (int i = 0; i < dependencies.Length; i++)
                {
                    depnHS.Add(dependencies[i]);
                }
                AddressableAssetEntries.Add(item, depnHS);
                return;
            }

            for (int i = 0; i < dependencies.Length; i++)
            {
                AddressableAssetEntries[item].Add(dependencies[i]);
            }
        }

        public void AddAssetRule(AddressableAssetRule rule)
        {
            if (AddressableAssetRule == null)
            {
                AddressableAssetRule = rule;
                return;
            }
            com.igg.core.IGGDebug.LogError("AddAssetRule wanna set different value " + AddressableAssetRule.name + " " + rule.name);
        }


        public void CheckReferenceByEntry(ResourceNode resourceNode, ResourceGraph aBResourceGraph)
        {
            var thisEntryDic = new Dictionary<string, AddressableAssetEntry>();

            foreach (var entry in AddressableAssetEntries)
            {
                if (!thisEntryDic.ContainsKey(entry.Key.AssetPath))
                {
                    thisEntryDic[entry.Key.AssetPath] = entry.Key;
                }
            }

            var matchingKeys = new List<AddressableAssetEntry>();

            foreach (var entry in resourceNode.AddressableAssetEntries)
            {
                foreach(var path in entry.Value)
                {
                    if (thisEntryDic.ContainsKey(path))
                    {
                        resourceNode.AddReference(this);
                        aBResourceGraph.AddEdge(resourceNode, entry.Key.AssetPath, this, path);
                    }
                }
            }

            var otherEntryDic = new Dictionary<string, AddressableAssetEntry>();
            foreach (var entry in resourceNode.AddressableAssetEntries)
            {
                if (!otherEntryDic.ContainsKey(entry.Key.AssetPath))
                {
                    otherEntryDic[entry.Key.AssetPath] = entry.Key;
                }
            }

            foreach (var entry in AddressableAssetEntries)
            {
                foreach (var path in entry.Value)
                {
                    if (otherEntryDic.ContainsKey(path))
                    {
                        this.AddReference(resourceNode);
                        aBResourceGraph.AddEdge(this, entry.Key.AssetPath, resourceNode, path);
                    }
                }
            }
        }

        public void Clear()
        {
            Dependencies.Clear();
            AddressableAssetEntries.Clear();
            References.Clear();
            ReferencedBy.Clear();
        }

        private void ExConstruct()
        {
            Dependencies = new HashSet<string>();
            AddressableAssetEntries = new Dictionary<AddressableAssetEntry, HashSet<string>>();
        }

        internal void SetDependencyRule(string assetPath)
        {
            var guid = AddressabelUtilities.GetAssetGuid<FeatureDependenciesScriptableObject>(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                com.igg.core.IGGDebug.Log("Can't find Dependencies "  + assetPath);
            }
            else
            {
                dependencyRule = guid;
            }
        }
    }


    public class ResourceEdge
    {
        public ResourceNode From { get; private set; }
        public ResourceNode To { get; private set; }

        public ResourceEdge(ResourceNode from, ResourceNode to)
        {
            From = from;
            To = to;
        }
    }
}
