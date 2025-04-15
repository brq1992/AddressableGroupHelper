using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor;
using System.IO;
using UnityEngine;
using com.igg.editor;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline;

namespace AddressableAssetTool.Graph
{
    internal class NewAddEntryByFolderNodeCreator : BaseNodeCreator
    {
        public NewAddEntryByFolderNodeCreator(AddressableAssetRule asset)
        {
            this.asset = asset;
        }

        internal override void CreateNode(string assetGUID, AddressableDependenciesGraph addressableDependenciesGraph, BundleBuildResults result)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            var dic = Path.GetDirectoryName(assetPath);
            string filter = "t:Object";
            var diChildGuids = AddressabelUtilities.FindDirectChildren(filter, dic);
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = asset;
            foreach (var diGuid in diChildGuids)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(diGuid);
                var groupName = AddresaableGroupBuildUtilities.GetFolderEntryName(rule.name, groupPath);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(groupPath);

                ABResourceGraph.GetOrCreateNode(diGuid);
                ResourceNode resourceNode = ABResourceGraph.GetNode(diGuid);

                resourceNode.AddAssetRule(rule);
                resourceNode.SetDependencyRule(groupPath);
                var group = setting.FindGroup(groupName);
                if(group == null)
                {
                    com.igg.core.IGGDebug.LogError("Can't find group "+groupName);
                    continue;
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
                    if (rNode.ResourceId != diGuid)
                    {
                        rNode.CheckReferenceByEntry(resourceNode, ABResourceGraph);
                    }
                }
            }
            
        }
    }
}