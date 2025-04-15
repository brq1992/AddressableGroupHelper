using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor;

namespace AddressableAssetTool.Graph
{
    internal class NewPackTogetherNodeCreator : BaseNodeCreator
    {
        public NewPackTogetherNodeCreator(AddressableAssetRule asset)
        {
            this.asset = asset;
        }

        internal override void CreateNode(string assetGUID, AddressableDependenciesGraph addressableDependenciesGraph)
        {
            ABResourceGraph.GetOrCreateNode(assetGUID);
            ResourceNode resourceNode = ABResourceGraph.GetNode(assetGUID);

            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = asset;
            resourceNode.AddAssetRule(rule);
            resourceNode.SetDependencyRule(AssetDatabase.GUIDToAssetPath(assetGUID));
            var group = setting.FindGroup(rule.name);
            foreach (var item in group.entries)
            {
                string entryAssetPath = item.AssetPath;
                string[] dependenciesAfterFilter = null;
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                List<string> dependenciesList = new List<string>();
                if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                {
                    var directDependencies = AddressableCache.GetVariantDependencies(item.AssetPath);
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                    dependenciesAfterFilter = dependenciesList.ToArray();
                }
                else
                {
                    var directDependencies = AddressableCache.GetDependencies(entryAssetPath, false);
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                    dependenciesAfterFilter = dependenciesList.ToArray();
                }
               
                resourceNode.AddDependencies(dependenciesAfterFilter);
                resourceNode.AddEntry(item, dependenciesAfterFilter);
            }

            foreach (var rNode in ABResourceGraph.GetAllNodes())
            {
                if (rNode.ResourceId != assetGUID)
                {
                    rNode.CheckReference(resourceNode);
                }
            }
        }
    }
}