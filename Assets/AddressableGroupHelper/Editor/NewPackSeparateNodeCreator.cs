using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using com.igg.editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.U2D;

namespace AddressableAssetTool.Graph
{
    internal class NewPackSeparateNodeCreator : BaseNodeCreator
    {
        public NewPackSeparateNodeCreator(AddressableAssetRule asset)
        {
            this.asset = asset;
        }

        internal override void CreateNode(string assetGUID, AddressableDependenciesGraph addressableDependenciesGraph)
        {
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = asset;
            var group = setting.FindGroup(rule.name);
            foreach (var item in group.entries)
            {
                string nodeGuid = null;
                string entryAssetPath = item.AssetPath;
                string[] dependenciesAfterFilter = null;
                //if(entryAssetPath.EndsWith(".prefab"))
                {
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
                    nodeGuid = item.guid;
                }

                //if(entryAssetPath.EndsWith(".png"))
                //{
                //    if(AddressableCache.CheckSpriteReference(entryAssetPath, out string atlas))
                //    {
                //        if(AddressabelUtilities.IsAssetAddressable(AssetDatabase.GUIDToAssetPath(atlas)))
                //        {
                //            return;
                //        }
                //        nodeGuid = atlas;
                //    }
                //    else
                //    {
                //        nodeGuid = item.guid;
                //    }
                //    dependenciesAfterFilter = AddressableCache.GetDependencies(entryAssetPath, false);
                //}

                ABResourceGraph.GetOrCreateNode(item.guid);
                ResourceNode resourceNode = ABResourceGraph.GetNode(item.guid);
                resourceNode.AddDependencies(dependenciesAfterFilter);
                resourceNode.AddEntry(item, dependenciesAfterFilter);
                resourceNode.AddAssetRule(rule);
                foreach (var rNode in ABResourceGraph.GetAllNodes())
                {
                    if (rNode.ResourceId != item.guid)
                    {
                        rNode.CheckReference(resourceNode);
                    }
                }
            }
        }
    }
}