using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AddressableAssetTool.Graph
{
    internal class PackTogetherNodeCreator : BaseNodeCreator
    {
        
        public PackTogetherNodeCreator(AddressableAssetRule asset)
        {
            this.asset = asset;
        }

        internal override void CreateNode(string assetGUID, AddressableDependenciesGraph addressableDependenciesGraph)
        {
            _window = addressableDependenciesGraph;

            DirectedGraph.Node dgNode = new DirectedGraph.Node(assetGUID, asset.name);
            dgNode.Rule = asset;
            graph.AddNode(dgNode);
            
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = asset;// as AddressableAssetRule;
            var group = setting.FindGroup(rule.name);
            foreach (var item in group.entries)
            {
                string entryAssetPath = item.AssetPath;
                if(item.MainAsset == null)
                {
                    Debug.LogError("Get Entry failed " + group.name + " " + item.AssetPath);
                    return;
                }
                var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                {
                    List<string> dependenciesList = new List<string>();
                    var directDependencies = AddressableCache.GetVariantDependencies(item.AssetPath);
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                    var dependenciesAfterFilter = dependenciesList.ToArray();
                    CreateDependencyNodes(dependenciesAfterFilter, assetGUID, dgNode);
                }
                else
                {
                    List<string> dependenciesList = new List<string>();
                    var directDependencies = AddressableCache.GetDependencies(entryAssetPath, false);
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                    var dependenciesAfterFilter = dependenciesList.ToArray();
                    CreateDependencyNodes(dependenciesAfterFilter, assetGUID, dgNode);
                }



                foreach (var node in guidNodeDic)
                {
                    if (assetGUID == node.Key)
                    {
                        continue;
                    }

                    AddressableAssetRule nodeRule = node.Value.Rule;
                    if (nodeRule != null && DGTool.IsReliance(item.AssetPath, nodeRule, out var data))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
                    {
                        string _assetRulePath = AssetDatabase.GetAssetPath(nodeRule);
                        string guid = AssetDatabase.AssetPathToGUID(_assetRulePath);
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackTogether)
                                data[i].DependencyGraphNode = guidNodeDic[guid];
                            else
                                data[i].DependencyGraphNode = guidNodeDic[data[i].Guids[0]];
                        }

                        for (int i = 0; i < data.Length; i++)
                        {
                            var isDependence = data[i].IsDependence;
                            var dependencyNode = data[i].DependencyGraphNode;
                            graph.AddEdge(dependencyNode, dgNode);
                        }
                    }
                }
            }

            guidNodeDic[assetGUID] = dgNode;

        }


        //internal override void CreateDependencyNodes(string[] dependencies, string guid, DirectedGraph.Node parentNode)
        //{
        //    List<GraphBaseGroup> list = _window._addressableGroups;

        //    foreach (string dependencyString in dependencies)
        //    {
        //        foreach (var item in guidNodeDic)
        //        {
        //            if (guid == item.Key)
        //            {
        //                continue;
        //            }

        //            AddressableAssetRule rule = item.Value.Rule;
        //            if (rule != null && DGTool.HasConnect(dependencyString, rule, out NodeDepenData[] data))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
        //            {
        //                string path = AssetDatabase.GetAssetPath(rule);
        //                string assetRuleGuid = AssetDatabase.AssetPathToGUID(path);
        //                for (int i = 0; i < data.Length; i++)
        //                {
        //                    if(rule.PackModel == PackMode.PackTogether)
        //                        data[i].DependencyGraphNode = guidNodeDic[assetRuleGuid];
        //                    else
        //                        data[i].DependencyGraphNode = guidNodeDic[data[i].Guids[0]];
        //                }

        //                for (int i = 0; i < data.Length; i++)
        //                {
        //                    var isDependence = data[i].IsDependence;
        //                    var dependencyNode = data[i].DependencyGraphNode;
        //                    if (isDependence)
        //                    {
        //                        graph.AddEdge(parentNode, dependencyNode);
        //                    }
        //                    else
        //                    {
        //                        graph.AddEdge(dependencyNode, parentNode);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

    }
}