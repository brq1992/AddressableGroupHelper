
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AddressableAssetTool.Graph
{
    internal class AddressableShowSingleGroup : AddressableBaseGroup
    {
        private AddressableAssetRule _target;

        internal AddressableShowSingleGroup(Object obj, AddressableDependenciesGraph addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {
            _target = obj as AddressableAssetRule;
        }

        internal override string[] GetDependencies()
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
                        var basePrefabDepenPaths = AssetDatabase.GetDependencies(basePrefabPath, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
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
                var paths = AssetDatabase.GetDependencies(guidToPah, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
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

        internal override void DrawGroup(AddressableBaseGroup adGroup, Object obj1, GraphView m_GraphView,
            EventCallback<GeometryChangedEvent, AddressableBaseGroup> UpdateGroupDependencyNodePlacement, AddressableDependenciesGraph graphWindow)
        {
            assetPath = AssetDatabase.GetAssetPath(_target);

            //assetPath will be empty if obj is null or isn't an asset (a scene object)
            if (string.IsNullOrEmpty(assetPath))
                return;

            Object mainObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
            groupNode = new Group { title = mainObject.name };

            if (mainObject == null)
            {
                Debug.Log("Object doesn't exist anymore");
                return;
            }

            string[] dependencies = GetDependencies();

            //mainNode = CreateNode(adGroup, mainObject, assetPath, true, dependencies.Length, graphWindow.m_GUIDNodeLookup);
            //mainNode.userData = 0;

            Rect position = new Rect(0, 0, 0, 0);
            //mainNode.SetPosition(position);

            if (!m_GraphView.Contains(groupNode))
            {
                m_GraphView.AddElement(groupNode);
            }

            //m_GraphView.AddElement(mainNode);

            //groupNode.AddElement(mainNode);

            //CreateDependencyNodes(adGroup, dependencies, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup);

            //m_AssetNodes.Add(mainNode);

            groupNode.capabilities &= ~Capabilities.Deletable;

            groupNode.Focus();

            //mainNode.RegisterCallback<GeometryChangedEvent, AddressableBaseGroup>(
            //    UpdateGroupDependencyNodePlacement, this
            //);

            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(_target.name);
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    var node = CreateNode(adGroup, item.MainAsset, item.AssetPath, true, dependencies.Length, graphWindow.m_GUIDNodeLookup);
                    node.userData = 0;
                    node.SetPosition(position);
                    groupNode.AddElement(node);
                    m_GraphView.AddElement(node);
                    m_AssetNodes.Add(node);
                    node.RegisterCallback<GeometryChangedEvent, AddressableBaseGroup>(
                    UpdateGroupDependencyNodePlacement, this
                    );

                    CreateDependencyNodes(null, dependencies, node, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup);
                }
            }

           
        }
        internal void CreateDependencyBetweenMainNodes(AddressableBaseGroup AddressableGroup, string[] dependencies, Node parentNode,
          Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            foreach (string dependencyString in dependencies)
            {
                Object dependencyAsset = AssetDatabase.LoadMainAssetAtPath(dependencyString);
                string[] deeperDependencies = AssetDatabase.GetDependencies(dependencyString, false);

                var typeName = dependencyAsset.GetType().Name;


                Node dependencyNode = CreateNode(AddressableGroup, dependencyAsset, AssetDatabase.GetAssetPath(dependencyAsset),
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

        internal override void CreateDependencyNodes(AddressableBaseGroup AddressableGroup, string[] dependencies, Node parentNode,
            Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            List<AddressableBaseGroup> list = _window._addressableGroups;

            foreach (string dependencyString in dependencies)
            {
                foreach (var group in list)
                {
                    if (this == group)
                    {
                        continue;
                    }

                    if (group.IsDependence(dependencyString, out bool isDependence, out Node dependencyNode))
                    {
                        if (isDependence)
                        {
                            Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

                            AddressableGroup.m_AssetConnections.Add(edge);
                            //AddressableGroup.m_AssetNodes.Add(dependencyNode);
                        }
                        else
                        {
                            Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);

                            AddressableGroup.m_AssetConnections.Add(edge);
                            //AddressableGroup.m_AssetNodes.Add(dependencyNode);
                        }
                    }
                }
            }

            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var _targetGroup = addressableAssetProfileSettings.FindGroup(_target.name);

            if (_targetGroup != null)
            {
                foreach (var item in _targetGroup.entries)
                {
                    foreach (var group in list)
                    {
                        if (this == group)
                        {
                            continue;
                        }

                        if (group.IsReliance(item.AssetPath, out Node dependencyNode))
                        {
                            //if (isDependence)
                            //{
                            //    Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

                            //    AddressableGroup.m_AssetConnections.Add(edge);
                            //    //AddressableGroup.m_AssetNodes.Add(dependencyNode);
                            //}
                            //else
                            //{

                            //}

                            Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);

                            AddressableGroup.m_AssetConnections.Add(edge);
                            //AddressableGroup.m_AssetNodes.Add(dependencyNode);
                        }
                    }
                }
            }
        }

        internal override bool IsDependence(string dependencyString, out bool isDependence, out Node dependencyNode)
        {
            if (_target.HasConnenct(dependencyString, out isDependence))
            {
                dependencyNode = mainNode;
                return true;
            }
            return base.IsDependence(dependencyString, out isDependence, out dependencyNode);
        }

        internal override bool IsReliance(string dependencyString, out Node dependencyNode)
        {
            if (_target.IsReliance(dependencyString))
            {
                dependencyNode = mainNode;
                return true;
            }
            return base.IsReliance(assetPath, out dependencyNode);
        }
    }
}
