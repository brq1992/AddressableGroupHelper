using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline;

namespace AddressableAssetTool.Graph
{
    internal class NewPackSeparateNodeCreator : BaseNodeCreator
    {
        public NewPackSeparateNodeCreator(AddressableAssetRule asset)
        {
            this.asset = asset;
        }

        internal override void CreateNode(string assetGUID, AddressableDependenciesGraph addressableDependenciesGraph, BundleBuildResults result)
        {
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = asset;
            var group = setting.FindGroup(rule.name);
            if(group == null)
            {
                Debug.LogError("Group not find " + rule.name);
                return;
            }
            foreach (var item in group.entries)
            {
                GUID gUID = AssetDatabase.GUIDFromAssetPath(item.AssetPath);
                if (result.AssetResults.TryGetValue(gUID, out AssetResultData value))
                {
                    string entryAssetPath = item.AssetPath;
                    string[] dependenciesAfterFilter = null;

                    List<string> dependenciesList = new List<string>();
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, value);
                    dependenciesAfterFilter = dependenciesList.ToArray();

                    ABResourceGraph.GetOrCreateNode(item.guid);
                    ResourceNode resourceNode = ABResourceGraph.GetNode(item.guid);
                    //Todo:dependency rule could be different.
                    resourceNode.SetDependencyRule(entryAssetPath);
                    resourceNode.AddDependencies(dependenciesAfterFilter);
                    resourceNode.AddEntry(item, dependenciesAfterFilter);
                    resourceNode.AddAssetRule(rule);
                    foreach (var rNode in ABResourceGraph.GetAllNodes())
                    {
                        if (rNode.ResourceId != item.guid)
                        {
                            rNode.CheckReferenceByEntry(resourceNode, ABResourceGraph);
                        }
                    }
                }
            }
        }
    }
}