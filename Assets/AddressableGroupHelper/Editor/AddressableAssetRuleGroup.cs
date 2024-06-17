using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AddressableAssetTool.Graph
{
    internal class AddressableAssetRuleGroup : AddressableBaseGroup
    {
        private AddressableAssetRule _target;
        private List<Node> mainNodes = new List<Node>();
        public AddressableAssetRuleGroup(Object obj, AddressableDependenciesGraph addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
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

        internal override void DrawGroup(AddressableBaseGroup adGroup, Object obj1, GraphView m_GraphView, EventCallback<GeometryChangedEvent, AddressableBaseGroup> UpdateGroupDependencyNodePlacement, AddressableDependenciesGraph graphWindow)
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
            /*
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
            //); */

            if (!m_GraphView.Contains(groupNode))
            {
                m_GraphView.AddElement(groupNode);
            }

            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            var group = setting.FindGroup(_target.name);
            Rect position = new Rect(0, 0, 0, 0);
            int count = 0;
            foreach (var item in group.entries)
            {
               
                int dependenciesLength = 0;
                var node = base.CreateNode(adGroup, item.MainAsset, item.AssetPath, true, dependenciesLength, graphWindow.m_GUIDNodeLookup);
                mainNodes.Add(node);
                node.SetPosition(new Rect(position.x + count * kNodeWidth, position.y, position.width, position.height));
                m_GraphView.AddElement(node);
                groupNode.AddElement(node);
                m_AssetNodes.Add(node);
                node.RegisterCallback<GeometryChangedEvent, AddressableBaseGroup>(
                    UpdateGroupDependencyNodePlacement, this
                );
                //node.userData = 0;
                count++;

                string[] mainNodeDependencies = AssetDatabase.GetDependencies(item.AssetPath, false);


                CreateDependencyBetweenMainNodes(adGroup, dependencies, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup);
            }

            //CreateDependencyNodes(adGroup, dependencies, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup);

            groupNode.capabilities &= ~Capabilities.Deletable;

            groupNode.Focus();
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
                else
                {
                    //TODO: if it already exists, put it in a separate group for shared assets
                    //Check if the dependencyNode is in the same group or not
                    //if it's a different group move it to a new shared group
                    /*
                    if (SharedToggle.value) {
                        if (!AddressableGroup.m_AssetNodes.Contains(dependencyNode)) {
                            if (AddressableGroup.SharedGroup == null) {
                                AddressableGroup.SharedGroup = new AddressableGroup();

                                AddressableGroups.Add(AddressableGroup.SharedGroup);
                                AddressableGroup.SharedGroup.assetPath = AddressableGroup.assetPath;

                                AddressableGroup.SharedGroup.groupNode = new Group { title = "Shared Group" };

                                AddressableGroup.SharedGroup.mainNode = dependencyNode;
                                AddressableGroup.SharedGroup.mainNode.userData = 0;
                            }

                            if (!m_GraphView.Contains(AddressableGroup.SharedGroup.groupNode)) {
                                m_GraphView.AddElement(AddressableGroup.SharedGroup.groupNode);
                            }

                            //add the node to the group and remove it from the previous group
                            AddressableGroup.m_AssetNodes.Remove(dependencyNode);
                            //AddressableGroup.groupNode.RemoveElement(dependencyNode);
                            AddressableGroup.m_DependenciesForPlacement.Remove(dependencyNode);

                            AddressableGroup.SharedGroup.m_DependenciesForPlacement.Add(dependencyNode);

                            if (!AddressableGroup.SharedGroup.groupNode.ContainsElement(dependencyNode)) {
                                AddressableGroup.SharedGroup.groupNode.AddElement(dependencyNode);
                            }

                            AddressableGroup.SharedGroup.m_AssetNodes.Add(dependencyNode);
                        }
                    }*/
                }

                Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

                AddressableGroup.m_AssetConnections.Add(edge);
                AddressableGroup.m_AssetNodes.Add(dependencyNode);
            }
        }

        internal override void CreateDependencyNodes(AddressableBaseGroup AddressableGroup, string[] dependencies, Node parentNode, 
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
                else
                {
                    //TODO: if it already exists, put it in a separate group for shared assets
                    //Check if the dependencyNode is in the same group or not
                    //if it's a different group move it to a new shared group
                    /*
                    if (SharedToggle.value) {
                        if (!AddressableGroup.m_AssetNodes.Contains(dependencyNode)) {
                            if (AddressableGroup.SharedGroup == null) {
                                AddressableGroup.SharedGroup = new AddressableGroup();

                                AddressableGroups.Add(AddressableGroup.SharedGroup);
                                AddressableGroup.SharedGroup.assetPath = AddressableGroup.assetPath;

                                AddressableGroup.SharedGroup.groupNode = new Group { title = "Shared Group" };

                                AddressableGroup.SharedGroup.mainNode = dependencyNode;
                                AddressableGroup.SharedGroup.mainNode.userData = 0;
                            }

                            if (!m_GraphView.Contains(AddressableGroup.SharedGroup.groupNode)) {
                                m_GraphView.AddElement(AddressableGroup.SharedGroup.groupNode);
                            }

                            //add the node to the group and remove it from the previous group
                            AddressableGroup.m_AssetNodes.Remove(dependencyNode);
                            //AddressableGroup.groupNode.RemoveElement(dependencyNode);
                            AddressableGroup.m_DependenciesForPlacement.Remove(dependencyNode);

                            AddressableGroup.SharedGroup.m_DependenciesForPlacement.Add(dependencyNode);

                            if (!AddressableGroup.SharedGroup.groupNode.ContainsElement(dependencyNode)) {
                                AddressableGroup.SharedGroup.groupNode.AddElement(dependencyNode);
                            }

                            AddressableGroup.SharedGroup.m_AssetNodes.Add(dependencyNode);
                        }
                    }*/
                }

                Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

                AddressableGroup.m_AssetConnections.Add(edge);
                AddressableGroup.m_AssetNodes.Add(dependencyNode);
            }
        }
    }
}