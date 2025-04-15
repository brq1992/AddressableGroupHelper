using AddressableAssetTool.DirectedGraph;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AddressableAssetTool.Graph
{
    internal class DGTool
    {
        internal static bool HasConnect(string dependencyString, AddressableAssetRule rule, out NodeDepenData[] data, Func<string> groupName, Func<bool> multiNode,
            UnityEditor.AddressableAssets.Settings.AddressableAssetEntry entry = null)
        {
            bool connnect = false;
            List<NodeDepenData> list = new List<NodeDepenData>();
            List<string> depns = new List<string>();
            List<string> guids = new List<string>();
            NodeDepenData nodeData = new NodeDepenData();
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(groupName());
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    if(entry != null && entry == item)
                    {
                        continue;
                    }

                    if (dependencyString.Equals(item.AssetPath))
                    {
                        connnect = true;
                        if (multiNode())
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

        
        //internal static bool HasConnect(string dependencyString, AddressableAssetRule rule, out NodeDepenData[] data, AddressableAssetEntry entry = null)
        //      {
        //          bool connnect = false;
        //          List<NodeDepenData> list = new List<NodeDepenData>();
        //          List<string> depns = new List<string>();
        //          List<string> guids = new List<string>();
        //          NodeDepenData nodeData = new NodeDepenData();
        //          var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
        //          var group = addressableAssetProfileSettings.FindGroup(rule.name);
        //          if (group != null)
        //          {
        //              foreach (var item in group.entries)
        //              {
        //                  if(entry != null && entry == item)
        //                  {
        //                      continue;
        //                  }

        //                  List<AddressableAssetEntry> childEntries = new List<AddressableAssetEntry>();
        //                  item.GatherAllAssets(childEntries, true, true, true);

        //                  List<string> allAssets = new List<string>();
        //                  allAssets.Add(item.AssetPath);
        //                  //for (int i = 0; i < childEntries.Count; i++)
        //                  //{
        //                  //    allAssets.Add(childEntries[i].AssetPath);
        //                  //}

        //                  for(int i = 0; i < allAssets.Count; i++)
        //                  {
        //                      string assetPath = allAssets[i];
        //                      if (dependencyString.Equals(assetPath))
        //                      {
        //                          connnect = true;
        //                          if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
        //                          {
        //                              list.Add(new NodeDepenData() { IsDependence = true, Dependencies = new string[] { assetPath }, Guids = new string[] { item.guid } });
        //                          }
        //                          else
        //                          {
        //                              nodeData.IsDependence = true;
        //                              depns.Add(assetPath);
        //                              guids.Add(item.guid);
        //                          }
        //                      }
        //                  }

        //                  //if (dependencyString.Equals(item.AssetPath))
        //                  //{
        //                  //    connnect = true;
        //                  //    if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
        //                  //    {
        //                  //        list.Add(new NodeDepenData() { IsDependence = true, Dependencies = new string[] { item.AssetPath }, Guids = new string[] { item.guid } });
        //                  //    }
        //                  //    else
        //                  //    {
        //                  //        nodeData.IsDependence = true;
        //                  //        depns.Add(item.AssetPath);
        //                  //        guids.Add(item.guid);
        //                  //    }
        //                  //}
        //              }

        //              if(nodeData.IsDependence)
        //              {
        //                  nodeData.Dependencies = depns.ToArray();
        //                  nodeData.Guids = guids.ToArray();
        //                  list.Add(nodeData);
        //              }
        //          }

        //          data = list.ToArray();
        //          return connnect;
        //      }

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

        internal static bool IsReliance(string assetPath, AddressableAssetRule rule, out NodeDepenData[] data, Func<string> groupName, Func<bool> multiNode,
            UnityEditor.AddressableAssets.Settings.AddressableAssetEntry entry = null)
        {
            List<string> list = new List<string>();
            var nodeDepenDataList = new List<NodeDepenData>();
            List<string> packTDepns = new List<string>();
            List<string> packTGuids = new List<string>();
            NodeDepenData packTogData = new NodeDepenData();
            bool connnect = false;
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(groupName());
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    if(entry != null && item == entry)
                    {
                        continue;
                    }
                    //com.igg.core.IGGDebug.LogError("IsReliance 1 " + item.AssetPath+ " " + DateTime.Now.ToString());
                    var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                    string[] dependenciesAfterFilter = new string[0];
                    if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                    {
                        //com.igg.core.IGGDebug.LogError("IsReliance 2 " + DateTime.Now.ToString());
                        List<string> dependenciesList = new List<string>();
                        //com.igg.core.IGGDebug.LogError("IsReliance 3 " + item.AssetPath + " " + DateTime.Now.ToString());
                        var directDependencies = AddressableCache.GetVariantDependencies(item.AssetPath, false);
                        //com.igg.core.IGGDebug.LogError("IsReliance 4 " + item.AssetPath + " " + DateTime.Now.ToString());
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        //com.igg.core.IGGDebug.LogError("IsReliance 5 " + item.AssetPath + " " + DateTime.Now.ToString());
                        dependenciesAfterFilter = dependenciesList.ToArray();
                    }
                    else
                    {
                        //com.igg.core.IGGDebug.LogError("IsReliance 6 " + DateTime.Now.ToString());
                        List<string> dependenciesList = new List<string>();
                        var directDependencies = AddressableCache.GetDependencies(item.AssetPath, false);
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        dependenciesAfterFilter = dependenciesList.ToArray();
                    }
                    //com.igg.core.IGGDebug.LogError("IsReliance 7 " + DateTime.Now.ToString());
                    if (multiNode())
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

        //internal static bool IsReliance(string assetPath, AddressableAssetRule rule, out NodeDepenData[] data, AddressableAssetEntry entry = null)
        //{
        //    List<string> list = new List<string>();
        //    var nodeDepenDataList = new List<NodeDepenData>();
        //    List<string> packTDepns = new List<string>();
        //    List<string> packTGuids = new List<string>();
        //    NodeDepenData packTogData = new NodeDepenData();
        //    bool connnect = false;
        //    var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
        //    var group = addressableAssetProfileSettings.FindGroup(rule.name);
        //    if (group != null)
        //    {
        //        foreach (var item in group.entries)
        //        {
        //            if (entry != null && item == entry)
        //            {
        //                continue;
        //            }

        //            List<AddressableAssetEntry> childEntries = new List<AddressableAssetEntry>();
        //            item.GatherAllAssets(childEntries, true, true, true);

        //            List<string> allAssets = new List<string>();
        //            for (int i = 0; i < childEntries.Count; i++)
        //            {
        //                allAssets.Add(childEntries[i].AssetPath);
        //            }

        //            for (int i = 0; i < allAssets.Count; i++)
        //            {
        //                var assetFilePath = allAssets[i];
        //                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetFilePath);
        //                if (obj == null)
        //                {
        //                    continue;
        //                }
        //                var prefabType = PrefabUtility.GetPrefabAssetType(obj);
        //                string[] dependenciesAfterFilter = new string[0];
        //                if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
        //                {
        //                    List<string> dependenciesList = new List<string>();
        //                    var directDependencies = AddressableCache.GetVariantDependencies(assetFilePath, false);
        //                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
        //                    dependenciesAfterFilter = dependenciesList.ToArray();
        //                }
        //                else
        //                {
        //                    List<string> dependenciesList = new List<string>();
        //                    var directDependencies = AddressableCache.GetDependencies(assetFilePath, false);
        //                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
        //                    dependenciesAfterFilter = dependenciesList.ToArray();
        //                }
        //                if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
        //                {
        //                    var separData = new NodeDepenData();
        //                    List<string> depns = new List<string>();
        //                    List<string> guids = new List<string>();
        //                    foreach (var path in dependenciesAfterFilter)
        //                    {
        //                        if (assetFilePath.Equals(path))
        //                        {
        //                            connnect = true;
        //                            list.Add(assetFilePath);
        //                            separData.IsDependence = true;
        //                            depns.Add(assetFilePath);
        //                            guids.Add(item.guid);
        //                        }
        //                    }
        //                    if (separData.IsDependence)
        //                    {
        //                        separData.Dependencies = depns.ToArray();
        //                        separData.Guids = guids.ToArray();
        //                        nodeDepenDataList.Add(separData);
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (var path in dependenciesAfterFilter)
        //                    {
        //                        if (assetFilePath.Equals(path))
        //                        {
        //                            connnect = true;
        //                            list.Add(assetFilePath);
        //                            packTogData.IsDependence = true;
        //                            packTDepns.Add(assetFilePath);
        //                            packTGuids.Add(item.guid);
        //                        }
        //                    }
        //                }
        //            }
        //        }



        //        if (packTogData.IsDependence)
        //        {
        //            packTogData.Dependencies = packTDepns.ToArray();
        //            packTogData.Guids = packTGuids.ToArray();
        //            nodeDepenDataList.Add(packTogData);
        //        }
        //    }
        //    data = nodeDepenDataList.ToArray();
        //    return connnect;
        //}

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

        internal static bool Reference(string dependencyString, AddressableAssetRule rule, out NodeDepenData[] data)
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
                        if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
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

                if (nodeData.IsDependence)
                {
                    nodeData.Dependencies = depns.ToArray();
                    nodeData.Guids = guids.ToArray();
                    list.Add(nodeData);
                }
            }

            data = list.ToArray();
            return connnect;
        }

        internal static bool ReferenceBy(string assetPath, AddressableAssetRule rule, out NodeDepenData[] data)
        {
            List<string> list = new List<string>();
            List<NodeDepenData> nodeDepenDataList = new List<NodeDepenData>();
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
                    string[] dependenciesAfterFilter = new string[0];
                    string newPath = AddressabelUtilities.GetUniqueAssetPath(item.AssetPath);
                    if(AddressableCache.TryGetVariantDependencies(newPath, out var directDependencies, false))
                    {

                    }
                    else
                    {
                        directDependencies = AddressableCache.GetDependencies(item.AssetPath, false);
                    }
                    List<string> dependenciesList = new List<string>();
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                    dependenciesAfterFilter = dependenciesList.ToArray();

                    if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
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
    }
}