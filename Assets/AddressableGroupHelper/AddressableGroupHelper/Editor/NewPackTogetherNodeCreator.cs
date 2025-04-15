using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace AddressableAssetTool.Graph
{
    internal class NewPackTogetherNodeCreator : BaseNodeCreator
    {
        public NewPackTogetherNodeCreator(AddressableAssetRule asset)
        {
            this.asset = asset;
        }

        internal override void CreateNode(string assetGUID, AddressableDependenciesGraph addressableDependenciesGraph, BundleBuildResults result)
        {
            ABResourceGraph.GetOrCreateNode(assetGUID);
            ResourceNode resourceNode = ABResourceGraph.GetNode(assetGUID);

            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = asset;
            resourceNode.AddAssetRule(rule);
            resourceNode.SetDependencyRule(AssetDatabase.GUIDToAssetPath(assetGUID));
            var group = setting.FindGroup(rule.name); if (group == null)
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
                    resourceNode.AddDependencies(dependenciesAfterFilter);
                    resourceNode.AddEntry(item, dependenciesAfterFilter);
                }
            }
            foreach (var rNode in ABResourceGraph.GetAllNodes())
            {
                if (rNode.ResourceId != assetGUID)
                {
                    rNode.CheckReferenceByEntry(resourceNode, ABResourceGraph);
                }
            }
        }
    }
}