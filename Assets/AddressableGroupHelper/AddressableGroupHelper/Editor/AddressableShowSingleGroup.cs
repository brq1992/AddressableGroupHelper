
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AddressableAssetTool.Graph
{
    internal class AddressableShowSingleGroup : AddressableGraphBaseGroup
    {
        private AddressableAssetRule _target;

        internal AddressableShowSingleGroup(Object obj, GraphWindow addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {
            _target = obj as AddressableAssetRule;
        }


        internal override void DrawGroup(GraphView m_GraphView, 
            EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
            GraphWindow graphWindow)
        {
            _assetRulePath = AssetDatabase.GetAssetPath(_target);

            //assetPath will be empty if obj is null or isn't an asset (a scene object)
            if (string.IsNullOrEmpty(_assetRulePath))
                return;

            Object mainObject = AssetDatabase.LoadMainAssetAtPath(_assetRulePath);
            groupNode = new Group { title = mainObject.name };

            if (mainObject == null)
            {
                com.igg.core.IGGDebug.Log("Object doesn't exist anymore");
                return;
            }

            Rect position = new Rect(0, 0, 0, 0);

            if (!m_GraphView.Contains(groupNode))
            {
                m_GraphView.AddElement(groupNode);
            }

            groupNode.capabilities &= ~Capabilities.Deletable;

            groupNode.Focus();

            List<GraphBaseGroup> graphBaseGroupList = _window._addressableGroups;

            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(_target.name);
            int count = 0;
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    //string[] dependencies = GetDependencies(item.MainAsset);
                    int inDegree = -1;
                    int outDegree = -1;
                    //if (BaseNodeCreator.guidNodeDic.TryGetValue(item.guid, out var dgNode))
                    //{
                    //    if (dgNode != null)
                    //    {
                    //        inDegree = BaseNodeCreator.graph.GetInDegree(dgNode).Capacity;
                    //        outDegree = BaseNodeCreator.graph.GetOutDegree(dgNode).Capacity;

                    //    }
                    //}
                    var graph = BaseNodeCreator.ABResourceGraph;
                    var graphNode = graph.GetNode(item.guid);
                    if (graphNode != null)
                    {
                        inDegree = graphNode.ReferencedBy.Count;
                        outDegree = graphNode.References.Count;
                    }
                    var node = CreateNode(item.MainAsset, item.guid, true, outDegree, graphWindow.m_GUIDNodeLookup, inDegree);
                    DGTool.SetNodeData(node.userData, 0);
                    position = BaseLayout.GetNewNodePostion(count);
                    node.SetPosition(position);
                    groupNode.AddElement(node);
                    m_GraphView.AddElement(node);
                    m_AssetNodes.Add(node);
                    //groupNode.Add(node);
                    //node.RegisterCallback<GeometryChangedEvent, AddressableGraphBaseGroup>(
                    //    UpdateGroupDependencyNodePlacement, this
                    //);

                    string entryPath = item.AssetPath;

                    string entryAssetPath = item.AssetPath;
                    var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                    if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                    {
                        List<string> dependenciesList = new List<string>();
                        var directDependencies = AddressableCache.GetVariantDependencies(entryAssetPath);
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        var dependenciesAfterFilter = dependenciesList.ToArray();
                        CreateDependencyNodes(dependenciesAfterFilter, node, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, entryPath, item);
                    }
                    else
                    {
                        List<string> dependenciesList = new List<string>();
                        var directDependencies = AddressableCache.GetDependencies(entryAssetPath, false);
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        var dependenciesAfterFilter = dependenciesList.ToArray();
                        CreateDependencyNodes(dependenciesAfterFilter, node, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, entryPath, item);
                    }

                    AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
                    var _targetGroup = addressableAssetProfileSettings.FindGroup(rule.name);

                    foreach (var baseGroup in graphBaseGroupList)
                    {
                        //if (this == baseGroup)
                        //{
                        //    continue;
                        //}

                        if (baseGroup.IsReliance(item.AssetPath, out NodeDepenData[] data, item))
                        {
                            for (int i = 0; i < data.Length; i++)
                            {
                                var isDependence = data[i].IsDependence;
                                var dependencyNode = data[i].DependencyGraphViewNode;
                                if (dependencyNode != null)
                                {
                                    Edge edge = CreateEdge(node, dependencyNode, m_GraphView);
                                    List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>(); //new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
                                    for (int j = 0; j < data[i].Dependencies.Length; j++)
                                    {
                                        string dependencePath = data[i].Dependencies[j];
                                        edgeUserDatas.Add(new EdgeUserData(dependencePath, entryPath));
                                    }
                                    edge.userData = edgeUserDatas;
                                    m_AssetConnections.Add(edge);
                                }
                            }
                        }
                    }


                    count++;
                }
            }
        }
		
		
		
		//internal override void DrawGroup(GraphView m_GraphView, 
  //          EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
  //          AddressableDependenciesGraph graphWindow)
  //      {
  //          _assetRulePath = AssetDatabase.GetAssetPath(_target);

  //          //assetPath will be empty if obj is null or isn't an asset (a scene object)
  //          if (string.IsNullOrEmpty(_assetRulePath))
  //              return;

  //          Object mainObject = AssetDatabase.LoadMainAssetAtPath(_assetRulePath);
  //          groupNode = new Group { title = mainObject.name };

  //          if (mainObject == null)
  //          {
  //              Debug.Log("Object doesn't exist anymore");
  //              return;
  //          }

  //          Rect position = new Rect(0, 0, 0, 0);

  //          if (!m_GraphView.Contains(groupNode))
  //          {
  //              m_GraphView.AddElement(groupNode);
  //          }

  //          groupNode.capabilities &= ~Capabilities.Deletable;

  //          groupNode.Focus();

  //          List<GraphBaseGroup> graphBaseGroupList = _window._addressableGroups;

  //          var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
  //          var group = addressableAssetProfileSettings.FindGroup(_target.name);
  //          int count = 0;
  //          if (group != null)
  //          {
  //              foreach (var item in group.entries)
  //              {
  //                  string entryAssetPath = item.AssetPath;
  //                  //string[] dependencies = GetDependencies(item.MainAsset);
  //                  int inDegree = -1;
  //                  int outDegree = -1;
  //                  //if (BaseNodeCreator.guidNodeDic.TryGetValue(item.guid, out var dgNode))
  //                  //{
  //                  //    if (dgNode != null)
  //                  //    {
  //                  //        inDegree = BaseNodeCreator.graph.GetInDegree(dgNode).Capacity;
  //                  //        outDegree = BaseNodeCreator.graph.GetOutDegree(dgNode).Capacity;

  //                  List<AddressableAssetEntry> childEntries = new List<AddressableAssetEntry>();
  //                  item.GatherAllAssets(childEntries, true, true, true);
  //                  List<string> allAssets = new List<string>();
  //                  //allAssets.Add(entryAssetPath);
  //                  for (int i = 0; i < childEntries.Count; i++)
  //                  {
  //                      allAssets.Add(childEntries[i].AssetPath);
  //                  }
  //                  //    }
  //                  //}

  //                  var graph = BaseNodeCreator.ABResourceGraph;
  //                  var graphNode = graph.GetNode(item.guid);
  //                  if (graphNode != null)
  //                  {
  //                      inDegree = graphNode.ReferencedBy.Count;
  //                      outDegree = graphNode.References.Count;
  //                  }
  //                  var node = CreateNode(item.MainAsset, item.AssetPath, true, outDegree, graphWindow.m_GUIDNodeLookup, inDegree);
  //                  DGTool.SetNodeData(node.userData, 0);
  //                  position = BaseLayout.GetNewNodePostion(count);
  //                  node.SetPosition(position);
  //                  groupNode.AddElement(node);
  //                  m_GraphView.AddElement(node);
  //                  m_AssetNodes.Add(node);
  //                  //groupNode.Add(node);
  //                  //node.RegisterCallback<GeometryChangedEvent, AddressableGraphBaseGroup>(
  //                  //    UpdateGroupDependencyNodePlacement, this
  //                  //);

  //                  //string entryPath = item.AssetPath;
  //                  string[] dependenciesAfterFilter = null;
  //                  List<string> dependenciesList = new List<string>();
                   
  //                  for(int i = 0; i < allAssets.Count; i++)
  //                  {
  //                      string assetFilePath = allAssets[i];
  //                      Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetFilePath);
  //                      if(obj == null)
  //                      {
  //                          continue;
  //                      }
  //                      var prefabType = PrefabUtility.GetPrefabAssetType(obj);
  //                      if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
  //                      {
  //                          var directDependencies = AddressableCache.GetVariantDependencies(assetFilePath);
  //                          AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
  //                      }
  //                      else
  //                      {
  //                          var directDependencies = AddressableCache.GetDependencies(assetFilePath, false);
  //                          AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
  //                      }
  //                  }
                   

  //                  dependenciesAfterFilter = dependenciesList.ToArray();
  //                  CreateDependencyNodes(dependenciesAfterFilter, node, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, entryAssetPath, item);

  //                  AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
  //                  var _targetGroup = addressableAssetProfileSettings.FindGroup(rule.name);

  //                  foreach (var baseGroup in graphBaseGroupList)
  //                  {
  //                      //if (this == baseGroup)
  //                      //{
  //                      //    continue;
  //                      //}


  //                      for (int i = 0; i < allAssets.Count; i++)
  //                      {
  //                          if (baseGroup.IsReliance(allAssets[i], out NodeDepenData[] nodeDepenData, item))
  //                          {
  //                              for (int j = 0; j < nodeDepenData.Length; j++)
  //                              {
  //                                  var dependencyNode = nodeDepenData[j].DependencyGraphViewNode;
  //                                  if (dependencyNode != null)
  //                                  {
  //                                      Edge edge = CreateEdge(node, dependencyNode, m_GraphView);
  //                                      List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>(); //new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
  //                                      for (int m = 0; m < nodeDepenData[j].Dependencies.Length; m++)
  //                                      {
  //                                          string dependencePath = nodeDepenData[j].Dependencies[m];
  //                                          edgeUserDatas.Add(new EdgeUserData(dependencePath, entryAssetPath));
  //                                      }
  //                                      edge.userData = edgeUserDatas;
  //                                      m_AssetConnections.Add(edge);
  //                                  }
  //                              }
  //                          }
  //                      }
  //                      //if (baseGroup.IsReliance(item.AssetPath, out NodeDepenData[] data, item))
  //                      //{
  //                      //    for (int i = 0; i < data.Length; i++)
  //                      //    {
  //                      //        //var isDependence = data[i].IsDependence;
  //                      //        var dependencyNode = data[i].DependencyGraphViewNode;
  //                      //        if (dependencyNode != null)
  //                      //        {
  //                      //            Edge edge = CreateEdge(node, dependencyNode, m_GraphView);
  //                      //            List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>(); //new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
  //                      //            for (int j = 0; j < data[i].Dependencies.Length; j++)
  //                      //            {
  //                      //                string dependencePath = data[i].Dependencies[j];
  //                      //                edgeUserDatas.Add(new EdgeUserData(dependencePath, entryAssetPath));
  //                      //            }
  //                      //            edge.userData = edgeUserDatas;
  //                      //            m_AssetConnections.Add(edge);
  //                      //        }
  //                      //    }
  //                      //}
  //                  }


  //                  count++;
  //              }
  //          }
  //      }

        internal override void CreateDependencyNodes(string[] dependencies, Node parentNode,
    Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup, string dependentName, AddressableAssetEntry item = null)
        {
            List<GraphBaseGroup> list = _window._addressableGroups;

            foreach (string dependencyString in dependencies)
            {
                foreach (var group in list)
                {
                    //if (this == group)
                    //{
                    //    continue;
                    //}

                    //string[] dependencePath = null;
                    if (group.IsDependence(dependencyString, out NodeDepenData[] data, item))
                    {
                        for(int i =0; i< data.Length; i++)
                        {
                            var isDependence = data[i].IsDependence;
                            var dependencyNode = data[i].DependencyGraphViewNode;
                            if (isDependence && dependencyNode != null)
                            {
                                Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);
                                List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>(); //new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
                                for(int j = 0; j < data[i].Dependencies.Length; j++)
                                {
                                    string dependencePath = data[i].Dependencies[j];
                                    edgeUserDatas.Add(new EdgeUserData(dependentName, dependencePath));
                                }
                                edge.userData = edgeUserDatas;
                                m_AssetConnections.Add(edge);
                            }
                            //else
                            //{
                            //    Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);
                            //    //edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencePath, dependentName) };
                            //    edge.userData = new List<EdgeUserData>() { new EdgeUserData("call jeff when you find this", "call jeff") };
                            //    //IGGDebug.Log(" dependencePath " + dependencePath + " dependentName " + dependentName);
                            //    m_AssetConnections.Add(edge);
                            //}
                        }
                    }
                }
            }
        }


        internal void CreateDependencyBetweenMainNodes(AddressableAssetGroup AddressableGroup, string[] dependencies, Node parentNode,
          Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            foreach (string dependencyString in dependencies)
            {
                Object dependencyAsset = AssetDatabase.LoadMainAssetAtPath(dependencyString);
                string[] deeperDependencies = AddressableCache.GetDependencies(dependencyString, false);

                var typeName = dependencyAsset.GetType().Name;


                Node dependencyNode = CreateNode(dependencyAsset, AssetDatabase.GetAssetPath(dependencyAsset),
                    false, deeperDependencies.Length, m_GUIDNodeLookup);

                if (!AddressableGroup.m_AssetNodes.Contains(dependencyNode))
                {
                    dependencyNode.userData = depth;
                }

                //CreateDependencyNodes(AddressableGroup, deeperDependencies, dependencyNode, groupNode, depth + 1, m_GraphView, m_GUIDNodeLookup);

                //if the node doesnt exists yet, put it in the group
                if (!m_GraphView.Contains(dependencyNode))
                {
                    m_GraphView.AddElement(dependencyNode);

                    AddressableGroup.m_DependenciesForPlacement.Add(dependencyNode);
                    groupNode.AddElement(dependencyNode);
                }

                Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

                AddressableGroup.m_AssetConnections.Add(edge);
                AddressableGroup.m_AssetNodes.Add(dependencyNode);
            }
        }

        //internal override void CreateDependencyNodes(AddressableBaseGroup AddressableGroup, string[] dependencies, Node parentNode,
        //    Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup)
        //{
        //    List<AddressableGraphBaseGroup> list = _window._addressableGroups;

        //    foreach (string dependencyString in dependencies)
        //    {
        //        foreach (var group in list)
        //        {
        //            if (this == group)
        //            {
        //                continue;
        //            }

        //            if (group.IsDependence(dependencyString, out bool isDependence, out Node dependencyNode, out _))
        //            {
        //                if (isDependence)
        //                {
        //                    Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

        //                    AddressableGroup.m_AssetConnections.Add(edge);
        //                    //AddressableGroup.m_AssetNodes.Add(dependencyNode);
        //                }
        //                else
        //                {
        //                    Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);

        //                    AddressableGroup.m_AssetConnections.Add(edge);
        //                    //AddressableGroup.m_AssetNodes.Add(dependencyNode);
        //                }
        //            }
        //        }
        //    }

        //    var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
        //    var _targetGroup = addressableAssetProfileSettings.FindGroup(_target.name);

        //    if (_targetGroup != null)
        //    {
        //        foreach (var item in _targetGroup.entries)
        //        {
        //            foreach (var group in list)
        //            {
        //                if (this == group)
        //                {
        //                    continue;
        //                }

        //                if (group.IsReliance(item.AssetPath, out Node dependencyNode))
        //                {
        //                    //if (isDependence)
        //                    //{
        //                    //    Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

        //                    //    AddressableGroup.m_AssetConnections.Add(edge);
        //                    //    //AddressableGroup.m_AssetNodes.Add(dependencyNode);
        //                    //}
        //                    //else
        //                    //{

        //                    //}

        //                    Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);

        //                    AddressableGroup.m_AssetConnections.Add(edge);
        //                    //AddressableGroup.m_AssetNodes.Add(dependencyNode);
        //                }
        //            }
        //        }
        //    }
        //}



        internal override void SetPosition(Rect pos)
        {
            foreach (var item in groupChildNodes)
            {
                item.SetPosition(pos);
            }
        }

        internal override bool IsReliance(string assetPath, out Node[] dependentNodes, out string[] dependentPaths)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            if (rule != null && rule.IsReliance(assetPath, out dependentPaths))
            {
                dependentNodes = new Node[dependentPaths.Length];
                for (int i=0;i<dependentPaths.Length;i++)
                {
                    string guid = AssetDatabase.AssetPathToGUID(dependentPaths[i]);
                    if (_window.m_GUIDNodeLookup.TryGetValue(guid, out Node node))
                    {
                        dependentNodes[i] = node;
                    }
                    else
                    {
                        com.igg.core.IGGDebug.LogError("find reliance but don't find node");
                    }
                }

                return true;
            }

            dependentNodes = null;
            dependentPaths = null;
            return false;
        }

        internal override bool IsReliance(string assetPath, out Node dependencyNode)
        {
            throw new System.NotImplementedException();
        }

        internal override bool IsDependence(string dependencyString, out bool isDependence, out Node dependencyNode, out string edgeUserData)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            if (rule != null && rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
            {
                dependencyNode = groupChildNodes[0];
                return true;
            }
            return base.IsDependence(dependencyString, out isDependence, out dependencyNode, out edgeUserData);
        }

        internal override bool IsDependence(string dependencyString, out NodeDepenData[] data, AddressableAssetEntry item = null, string groupName = null)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            if (rule != null && DGTool.HasConnect(dependencyString, rule, out data, () => GetName(rule), () => IsMultiNode(rule), entry:item))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
            {
                for (int i = 0;i<data.Length;i++)
                {
                    if(_window.m_GUIDNodeLookup.TryGetValue(data[i].Guids[0], out var result))
                        data[i].DependencyGraphViewNode = result;
                }
                return true;
            }
            data = new NodeDepenData[0];
            return false;
        }

        internal override bool IsReliance(string assetPath, out NodeDepenData[] data, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item = null, string groupName = null)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            if (rule != null && DGTool.IsReliance(assetPath, rule, out data, ()=> GetName(rule), () => IsMultiNode(rule)))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (_window.m_GUIDNodeLookup.TryGetValue(data[i].Guids[0], out var result))
                        data[i].DependencyGraphViewNode = result;
                }
                return true;
            }
            data = new NodeDepenData[0];
            return false;
        }
    }
}
