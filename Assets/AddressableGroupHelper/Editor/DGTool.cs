using AddressableAssetTool.DirectedGraph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using UnityEditor;
using UnityEditor.AddressableAssets;

namespace AddressableAssetTool.Graph
{
    internal class DGTool
    {
        internal static bool CheckNodeIsDependencies(AddressableAssetRule rule, Node key, string dependencyString, out bool isDependence, out Node dependencyNode)
        {
            if (rule != null && rule.HasConnenct(dependencyString, out isDependence, out _))
            {
                dependencyNode = key;
                return true;
            }
            dependencyNode = null;
            isDependence = false;
            return false;
        }

        internal static bool CheckPackTNodeIsDependencies(AddressableDependenciesGraph _window, AddressableAssetRule rule,string _assetRulePath, KeyValuePair<string, Node> item, string dependencyString, out NodeDepenData[] data)
        {
            if (rule != null && HasConnect(dependencyString, rule, out data))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
            {
                string guid = AssetDatabase.AssetPathToGUID(_assetRulePath);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i].DependencyGraphViewNode = _window.m_GUIDNodeLookup[guid];
                }
                return true;
            }
            data = new NodeDepenData[0];
            return false;
        }

        internal static bool HasConnect(string dependencyString, AddressableAssetRule rule, out NodeDepenData[] data)
        {
            bool connnect = false;
            List<NodeDepenData> list = new List<NodeDepenData>();
            List<string> depns = new List<string>();
            List<string> guids = new List<string>();
            NodeDepenData nodeData = new NodeDepenData();
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(rule.name);
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    if (dependencyString.Equals(item.AssetPath))
                    {
                        connnect = true;
                        if (rule.PackModel == PackMode.PackSeparately)
                        {
                            list.Add(new NodeDepenData() { IsDependence = true, Dependencies = new string[] { item.AssetPath }, Guids = new string[] { item.guid } });
                        }
                        else
                        {
                            nodeData.IsDependence = true;
                            depns.Add(item.AssetPath);
                            guids.Add(item.guid);
                        }
                    }
                }

                if(nodeData.IsDependence)
                {
                    nodeData.Dependencies = depns.ToArray();
                    nodeData.Guids = guids.ToArray();
                    list.Add(nodeData);
                }
            }

            data = list.ToArray();
            return connnect;
        }

        internal static bool IsReliance(string assetPath, AddressableAssetRule rule, Node node, out Node[] dependentNodes)
        {
            if (rule != null && rule.IsReliance(assetPath, out var dependentPaths))
            {
                dependentNodes = new Node[dependentPaths.Length];
                for (int i = 0; i < dependentPaths.Length; i++)
                {
                    dependentNodes[i] = node;
                }

                return true;
            }

            dependentNodes = null;
            dependentPaths = null;
            return false;
        }

        internal static bool IsReliance(string assetPath, AddressableAssetRule rule, out NodeDepenData[] data)
        {
            List<string> list = new List<string>();
            var nodeDepenDataList = new List<NodeDepenData>();
            List<string> packTDepns = new List<string>();
            List<string> packTGuids = new List<string>();
            NodeDepenData packTogData = new NodeDepenData();
            bool connnect = false;
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(rule.name);
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    //UnityEngine.Debug.LogError("IsReliance 1 " + item.AssetPath+ " " + DateTime.Now.ToString());
                    var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                    string[] dependenciesAfterFilter = new string[0];
                    if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                    {
                        //UnityEngine.Debug.LogError("IsReliance 2 " + DateTime.Now.ToString());
                        List<string> dependenciesList = new List<string>();
                        //UnityEngine.Debug.LogError("IsReliance 3 " + item.AssetPath + " " + DateTime.Now.ToString());
                        var directDependencies = AddressableCache.GetVariantDependencies(item.AssetPath, false);
                        //UnityEngine.Debug.LogError("IsReliance 4 " + item.AssetPath + " " + DateTime.Now.ToString());
                        Graph.AddressablePackTogetherGroup.GetEntryDependencies(dependenciesList, directDependencies, false);
                        //UnityEngine.Debug.LogError("IsReliance 5 " + item.AssetPath + " " + DateTime.Now.ToString());
                        dependenciesAfterFilter = dependenciesList.ToArray();
                    }
                    else
                    {
                        //UnityEngine.Debug.LogError("IsReliance 6 " + DateTime.Now.ToString());
                        List<string> dependenciesList = new List<string>();
                        var directDependencies = AddressableCache.GetDependencies(item.AssetPath, false);
                        Graph.AddressablePackTogetherGroup.GetEntryDependencies(dependenciesList, directDependencies, false);
                        dependenciesAfterFilter = dependenciesList.ToArray();
                    }
                    //UnityEngine.Debug.LogError("IsReliance 7 " + DateTime.Now.ToString());
                    if (rule.PackModel == PackMode.PackSeparately)
                    {
                        var separData = new NodeDepenData();
                        List<string> depns = new List<string>();
                        List<string> guids = new List<string>();
                        foreach (var path in dependenciesAfterFilter)
                        {
                            if (assetPath.Equals(path))
                            {
                                connnect = true;
                                list.Add(item.AssetPath);
                                separData.IsDependence = true;
                                depns.Add(item.AssetPath);
                                guids.Add(item.guid);
                            }
                        }
                        if (separData.IsDependence)
                        {
                            separData.Dependencies = depns.ToArray();
                            separData.Guids = guids.ToArray();
                            nodeDepenDataList.Add(separData);
                        }
                    }
                    else
                    {
                        foreach (var path in dependenciesAfterFilter)
                        {
                            if (assetPath.Equals(path))
                            {
                                connnect = true;
                                list.Add(item.AssetPath);
                                packTogData.IsDependence = true;
                                packTDepns.Add(item.AssetPath);
                                packTGuids.Add(item.guid);
                            }
                        }
                    }
                }

                if (packTogData.IsDependence)
                {
                    packTogData.Dependencies = packTDepns.ToArray();
                    packTogData.Guids = packTGuids.ToArray();
                    nodeDepenDataList.Add(packTogData);
                }
            }
            data = nodeDepenDataList.ToArray();
            return connnect;
        }

        internal static void SetNodeData(object userData, int depth)
        {
            var data = userData as GraphViewNodeUserData;
            if(data != null)
            {
                data.Depth = depth;
            }
            else
            {
                data = new GraphViewNodeUserData() { Depth = depth };
            }
        }

        internal static GraphViewNodeUserData GetNodeData(object userData, string guid)
        {
            var data = userData as GraphViewNodeUserData;
            if (data != null)
            {
                data.Guid = guid;
            }
            else
            {
                data = new GraphViewNodeUserData() { Guid = guid };
            }
            return data;
        }
    }
}