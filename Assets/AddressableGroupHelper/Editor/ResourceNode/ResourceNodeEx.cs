
using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AddressableAssetTool.Graph
{
    public partial class ResourceNode
    {

        public HashSet<string> Dependencies { get; private set; }
        public Dictionary<AddressableAssetEntry, HashSet<string>> AddressableAssetEntries { get; private set; }
        public AddressableAssetRule AddressableAssetRule { get; private set; }

        public void AddDependencies(string[] dependenciesAfterFilter)
        {
            for(int i =0; i< dependenciesAfterFilter.Length; i++)
            {
                if (!Dependencies.Contains(dependenciesAfterFilter[i]))
                {
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
            Debug.LogError("AddAssetRule wanna set different value " + AddressableAssetRule.name + " " + rule.name);
        }


        public void CheckReference(ResourceNode resourceNode)
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
