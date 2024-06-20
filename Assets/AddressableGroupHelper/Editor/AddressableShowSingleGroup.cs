
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

        internal AddressableShowSingleGroup(Object obj, AddressableDependenciesGraph addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {
            _target = obj as AddressableAssetRule;
        }

        internal string[] GetDependencies()
        {
            List<string> list = new List<string>();
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            var group = setting.FindGroup(_target.name);
            foreach (var item in group.entries)
            {
                var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                if (prefabType == PrefabAssetType.Variant)
                {
                    var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(item.MainAsset);
                    if (basePrefab != null)
                    {
                        var basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
                        var basePrefabDepenPaths = AddressableCache.GetDependencies(basePrefabPath, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                        foreach (var path in basePrefabDepenPaths)
                        {
                            if (!list.Contains(path))
                            {
                                list.Add(path);
                            }
                        }
                    }
                }

                var guid = item.guid;
                string guidToPah = AssetDatabase.GUIDToAssetPath(guid);
                var paths = AddressableCache.GetDependencies(guidToPah, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                foreach (var path in paths)
                {
                    if (!list.Contains(path))
                    {
                        list.Add(path);
                    }
                }
            }

            return list.ToArray();
        }

        internal override void DrawGroup(GraphView m_GraphView, 
            EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
            AddressableDependenciesGraph graphWindow)
        {
            _assetRulePath = AssetDatabase.GetAssetPath(_target);

            //assetPath will be empty if obj is null or isn't an asset (a scene object)
            if (string.IsNullOrEmpty(_assetRulePath))
                return;

            Object mainObject = AssetDatabase.LoadMainAssetAtPath(_assetRulePath);
            groupNode = new Group { title = mainObject.name };

            if (mainObject == null)
            {
                Debug.Log("Object doesn't exist anymore");
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
                    string[] dependencies = GetDependencies(item.MainAsset);
                    var node = CreateNode(item.MainAsset, item.AssetPath, true, dependencies.Length, graphWindow.m_GUIDNodeLookup);
                    node.userData = 0;
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
                    CreateDependencyNodes(dependencies, node, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, entryPath);


                    foreach (var graphBaseGroup in graphBaseGroupList)
                    {
                        if (this == graphBaseGroup)
                        {
                            continue;
                        }

                        string[] dependencePaths = null;
                        if (graphBaseGroup.IsReliance(item.AssetPath, out Node[] dependentNodes, out dependencePaths))
                        {
                            for (int i = 0; i < dependentNodes.Length; i++)
                            {
                                Edge edge = CreateEdge(node, dependentNodes[i], m_GraphView);
                                edge.userData = new EdgeUserData(dependencePaths[i], item.AssetPath);
                                edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencePaths[i], item.AssetPath) };
                                m_AssetConnections.Add(edge);
                            }
                        }
                    }


                    count++;
                }
            }
        }

        internal override void CreateDependencyNodes(string[] dependencies, Node parentNode,
    Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup, string dependentName)
        {
            List<GraphBaseGroup> list = _window._addressableGroups;

            foreach (string dependencyString in dependencies)
            {
                foreach (var group in list)
                {
                    if (this == group)
                    {
                        continue;
                    }

                    string dependencePath = null;
                    if (group.IsDependence(dependencyString, out bool isDependence, out Node dependencyNode, out dependencePath))
                    {
                        if (isDependence)
                        {
                            Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);
                            edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
                            m_AssetConnections.Add(edge);
                        }
                        else
                        {
                            Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);
                            //edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencePath, dependentName) };
                            edge.userData = new List<EdgeUserData>() { new EdgeUserData("call jeff when you find this", "call jeff") };
                            Debug.Log(" dependencePath " + dependencePath + " dependentName " + dependentName);
                            m_AssetConnections.Add(edge);
                        }

                    }
                }
            }

            //AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            //var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            //var _targetGroup = addressableAssetProfileSettings.FindGroup(rule.name);

            //if (_targetGroup != null)
            //{
            //    foreach (var item in _targetGroup.entries)
            //    {
            //        foreach (var group in list)
            //        {
            //            if (this == group)
            //            {
            //                continue;
            //            }

            //            string[] dependencePaths = null;
            //            if (group.IsReliance(item.AssetPath, out Node[] dependentNodes, out dependencePaths))
            //            {
            //                for (int i = 0; i < dependentNodes.Length; i++)
            //                {
            //                    Edge edge = CreateEdge(parentNode, dependentNodes[i], m_GraphView);
            //                    edge.userData = new EdgeUserData(dependencePaths[i], item.AssetPath);
            //                    edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencePaths[i], dependentName) };
            //                    m_AssetConnections.Add(edge);
            //                }
            //            }
            //        }
            //    }
            //}
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
                        Debug.LogError("find reliance but don't find node");
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

    }
}
