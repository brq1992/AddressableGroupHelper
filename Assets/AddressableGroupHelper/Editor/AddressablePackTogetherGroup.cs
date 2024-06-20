using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using NUnit.Framework;

namespace AddressableAssetTool.Graph
{
    internal class AddressablePackTogetherGroup : AddressableGraphBaseGroup
    {
        public AddressablePackTogetherGroup(Object obj, AddressableDependenciesGraph addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {

        }

        internal override void DrawGroup(GraphView m_GraphView, EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement, 
            AddressableDependenciesGraph graphWindow)
        {
            _assetRulePath = AssetDatabase.GetAssetPath(_assetRuleObj);

            //assetPath will be empty if obj is null or isn't an asset (a scene object)
            if (string.IsNullOrEmpty(_assetRulePath))
                return;

            groupNode = new Group { title = _assetRuleObj.name };

            string[] dependencies = GetDependencies();

            var mainNode = CreateNode(_assetRuleObj, _assetRulePath, true, dependencies.Length, graphWindow.m_GUIDNodeLookup);
            mainNode.userData = 0;

            Rect position = new Rect(0, 0, 0, 0);
            mainNode.SetPosition(position);
            groupChildNodes.Add(mainNode);

            graphWindow.AddAndPosGroupNode(groupNode);

            if (!m_GraphView.Contains(groupNode))
            {
                m_GraphView.AddElement(groupNode);
            }
            m_GraphView.AddElement(mainNode);
            groupNode.AddElement(mainNode);

            List<GraphBaseGroup> baseGroupList = _window._addressableGroups;
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            var group = setting.FindGroup(rule.name);
            foreach (var item in group.entries)
            {
                var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                if (prefabType == PrefabAssetType.Variant)
                {
                    var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(item.MainAsset);
                    if (basePrefab != null)
                    {
                        var basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
                        var basePrefabDepenPaths = AddressableCache.GetDependencies(basePrefabPath, false); 
                        CreateDependencyNodes(basePrefabDepenPaths, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, item.AssetPath);
                    }
                }

                var guid = item.guid;
                string guidToPah = AssetDatabase.GUIDToAssetPath(guid);
                var entryDependencies = AddressableCache.GetDependencies(guidToPah, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                CreateDependencyNodes(entryDependencies, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, item.AssetPath);

                
                foreach (var baseGroup in baseGroupList)
                {
                    if (this == baseGroup)
                    {
                        continue;
                    }

                    string[] dependencePaths = null;
                    if (baseGroup.IsReliance(item.AssetPath, out Node[] dependentNodes, out dependencePaths))
                    {
                        for (int i = 0; i < dependentNodes.Length; i++)
                        {
                            Edge edge = CreateEdge(mainNode, dependentNodes[i], m_GraphView);
                            edge.userData = new EdgeUserData(dependencePaths[i], item.AssetPath);
                            edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencePaths[i], item.AssetPath) };
                            m_AssetConnections.Add(edge);
                        }
                    }
                }
            }

            m_AssetNodes.Add(mainNode);

            groupNode.capabilities &= ~Capabilities.Deletable;

            groupNode.Focus();

            mainNode.RegisterCallback<GeometryChangedEvent, GraphBaseGroup>(
                UpdateGroupDependencyNodePlacement, this
            );
        }

        private string[] GetDependencies()
        {
            List<string> list = new List<string>();
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            var group = setting.FindGroup(rule.name);
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
                            //edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencyString, dependentName) }; 
                            edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
                            m_AssetConnections.Add(edge);
                        }
                        else
                        {
                            Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);
                            //edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependentName,dependencyString) }; //edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
                            edge.userData = new List<EdgeUserData>() { new EdgeUserData("unknow", "unknow") };
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
            //                for(int i =0;i<dependentNodes.Length;i++)
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

        internal override Node CreateNode(Object obj, string assetPath, bool prefabCheck, int dependencyAmount, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            Node resultNode;
            string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            if (m_GUIDNodeLookup.TryGetValue(assetGUID, out resultNode))
            {
                return resultNode;
            }

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var assetGuid, out long _))
            {
                var objNode = new Node
                {
                    title = obj.name,
                    style =
                {
                    width = kNodeWidth
                }
                };

                objNode.extensionContainer.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 0.8f);

                #region Select button
                objNode.titleContainer.Add(new Button(() =>
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                })
                {
                    style =
            {
                        height = 16.0f,
                        alignSelf = Align.Center,
                        alignItems = Align.Center
                    },
                    text = "Select"
                });
                #endregion

                #region Padding
                var infoContainer = new VisualElement
                {
                    style =
                    {
                    paddingBottom = 4.0f,
                    paddingTop = 4.0f,
                    paddingLeft = 4.0f,
                    paddingRight = 4.0f
                }
                };
                #endregion

                #region Asset Path, maybe removed to improve visibility with large amount of assets
                //                infoContainer.Add(new Label
                //                {
                //                    text = assetPath,
                //#if UNITY_2019_1_OR_NEWER
                //                    style = { whiteSpace = WhiteSpace.Normal }
                //#else
                //                                style = { wordWrap = true }
                //#endif
                //                });
                #endregion

                #region Asset type
                var typeName = obj.GetType().Name;
                if (prefabCheck)
                {
                    var prefabType = PrefabUtility.GetPrefabAssetType(obj);
                    if (prefabType != PrefabAssetType.NotAPrefab)
                        typeName = $"{prefabType} Prefab";
                }

                var typeLabel = new Label
                {
                    text = $"Type: {typeName}",
                };
                infoContainer.Add(typeLabel);

                objNode.extensionContainer.Add(infoContainer);
                #endregion

                var typeContainer = new VisualElement
                {
                    style =
                    {
                    paddingBottom = 4.0f,
                    paddingTop = 4.0f,
                    paddingLeft = 4.0f,
                    paddingRight = 4.0f,
                    backgroundColor = AddressableDependenciesGraph.GetColorByAssetType(obj)
        }
                };

                objNode.extensionContainer.Add(typeContainer);

                #region Node Icon, replaced with color 
                //Texture assetTexture = AssetPreview.GetAssetPreview(obj);
                //if (!assetTexture)
                //    assetTexture = AssetPreview.GetMiniThumbnail(obj);

                //if (assetTexture)
                //{
                //    AddDivider(objNode);

                //    objNode.extensionContainer.Add(new Image
                //    {
                //        image = assetTexture,
                //        scaleMode = ScaleMode.ScaleToFit,
                //        style =
                //        {
                //            paddingBottom = 4.0f,
                //            paddingTop = 4.0f,
                //            paddingLeft = 4.0f,
                //            paddingRight = 4.0f
                //        }
                //    });
                //} 
                #endregion

                // Ports
                //if (!isMainNode) {
                Port realPort = objNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Object));
                realPort.portName = "Dependent";
                objNode.inputContainer.Add(realPort);
                //}

                if (dependencyAmount > 0)
                {
#if UNITY_2018_1
                Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, typeof(Object));
#else
                    Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Object));
#endif
                    port.portName = dependencyAmount + " Dependencies";
                    objNode.outputContainer.Add(port);
                    objNode.RefreshPorts();
                }

                resultNode = objNode;

                resultNode.RefreshExpandedState();
                resultNode.RefreshPorts();
                resultNode.capabilities &= ~Capabilities.Deletable;
                resultNode.capabilities |= Capabilities.Collapsible;
            }
            m_GUIDNodeLookup[assetGUID] = resultNode;
            return resultNode;
        }


        internal override void SetPosition(Rect pos)
        {
            groupChildNodes[0].SetPosition(pos);
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

        internal override bool IsReliance(string assetPath, out Node[] dependentNodes, out string[] dependentPaths)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            if (rule != null && rule.IsReliance(assetPath, out dependentPaths))
            {
                dependentNodes = new Node[dependentPaths.Length];
                for (int i = 0; i < dependentPaths.Length; i++)
                {
                    string guid = AssetDatabase.AssetPathToGUID(_assetRulePath);
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