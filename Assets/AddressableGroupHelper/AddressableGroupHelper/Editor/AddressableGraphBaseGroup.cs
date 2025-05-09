﻿using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AddressableAssetTool.Graph
{
    internal abstract class AddressableGraphBaseGroup : GraphBaseGroup
    {
        public AddressableGraphBaseGroup(Object obj, GraphWindow addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {
            this._assetRuleObj = obj;
            _window = addressableDependenciesGraph;
        }

        internal override void DrawGroup(GraphView m_GraphView, EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
            GraphWindow graphWindow)
        {
            throw new NotImplementedException();
        }

        internal override bool IsDependence(string dependencyString, out bool isDependence, out string dependencePath)
        {
            isDependence = false;
            dependencePath = null;
            return false;
        }

        internal override bool IsDependence(string dependencyString, out bool isDependence, out Node dependencyNode, out string dependencePath)
        {
            isDependence = false;
            dependencyNode = null; 
            dependencePath = null;
            return false;
        }

        internal virtual Edge CreateEdge(Node dependencyNode, Node parentNode, GraphView m_GraphView)
        {
            Port inport = dependencyNode.inputContainer[0] as Port;
            Port outport = parentNode.outputContainer[0] as Port;

            Edge edge = new Edge
            {
                input = inport,
                output = outport,
            };

            edge.RegisterCallback<MouseUpEvent>(OnMouseUp);

            edge.input?.Connect(edge);
            edge.output?.Connect(edge);

            dependencyNode.RefreshPorts();

            m_GraphView.AddElement(edge);

            edge.capabilities &= ~Capabilities.Deletable;

            return edge;
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            _window.ShowInfoWindow(evt);
        }

        internal string[] GetDependencies()
        {
            throw new NotImplementedException();
        }

        internal override Rect GetMainNodePositoin()
        {
            return new Rect(0, 0, 0, 0);
        }

        internal override void UnregisterCallback(EventCallback<GeometryChangedEvent, GraphBaseGroup> updateGroupDependencyNodePlacement)
        {
            foreach (var item in groupChildNodes)
            {
                item.UnregisterCallback<GeometryChangedEvent, AddressableGraphBaseGroup>(
                    updateGroupDependencyNodePlacement
                );
            }
        }

        internal virtual Node CreateNode(Object obj, string assetGUID, bool prefabCheck, int outDegree, Dictionary<string, Node> m_GUIDNodeLookup,
            int inDegree = 0)
        {
            Node resultNode;
            //string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            if (m_GUIDNodeLookup.TryGetValue(assetGUID, out resultNode))
            {
                return resultNode;
            }

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var assetGuid, out long _))
            {
                var objNode = new Node
                {
                    title = obj.name.Substring(0, Math.Min(obj.name.Length,30)),
                    style =
                {
                    width = kNodeWidth
                }
                };

                objNode.extensionContainer.style.backgroundColor = AddressaableToolKey.DefaultNodeBackgroundColor;// new Color(0.24f, 0.24f, 0.24f, 0.8f);

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
                realPort.portName = inDegree + " Dependent";
                realPort.RegisterCallback<MouseUpEvent>(OnInPortMouseUp);
                realPort.userData = DGTool.GetNodeData(realPort.userData, assetGUID);
                objNode.inputContainer.Add(realPort);
                //}

                //if (inDegree > 0)
                {
#if UNITY_2018_1
                Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, typeof(Object));
#else
                    Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Object));
#endif
                    port.portName = outDegree + " Dependencies";
                    DGTool.GetNodeData(port.userData, assetGUID);
                    port.RegisterCallback<MouseUpEvent>(OnOutPortMouseUp);
                    port.userData = DGTool.GetNodeData(port.userData, assetGUID);
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

        protected virtual void OnOutPortMouseUp(MouseUpEvent evt)
        {
            VisualElement element = evt.currentTarget as VisualElement;
            if (element == null)
            {
                com.igg.core.IGGDebug.LogError(" OnOutPortMouseUp 1");
                return;
            }

            Port edge = evt.currentTarget as Port;
            if (edge == null)
            {
                com.igg.core.IGGDebug.LogError(" OnOutPortMouseUp 2");
                return;
            }

            var data = edge.userData as GraphViewNodeUserData;
            if (data == null)
            {
                com.igg.core.IGGDebug.LogError(" OnOutPortMouseUp 3");
                return;
            }

            var guid = data.Guid;
            var node = BaseNodeCreator.ABResourceGraph.GetNode(guid);
            //var inDegree = BaseNodeCreator.graph.GetOutDegree(node);
            //Debug.LogError(AssetDatabase.GUIDToAssetPath(data.Guid) + " ----------------- out:");
            foreach (var item in node.References.Values)
            {
                //Debug.LogError(AssetDatabase.GUIDToAssetPath(item.ResourceId));
                //var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(AssetDatabase.GUIDToAssetPath(item.ResourceId));
                //if (asset != null && asset.IsRuleUsed)
                //{
                //    _window.AddElement(asset);
                //}
                var asset = item.AddressableAssetRule;
                if (asset != null && asset.IsRuleUsed)
                {
                    _window.AddElement(asset);
                }
            }
        }

        protected virtual void OnInPortMouseUp(MouseUpEvent evt)
        {
            VisualElement element = evt.currentTarget as VisualElement;
            if (element == null)
            {
                com.igg.core.IGGDebug.LogError(" OnInPortMouseUp 1");
                return;
            }

            Port edge = evt.currentTarget as Port;
            if (edge == null)
            {
                com.igg.core.IGGDebug.LogError(" OnInPortMouseUp 2");
                return;
            }

            var data = edge.userData as GraphViewNodeUserData;
            if (data == null)
            {
                com.igg.core.IGGDebug.LogError(" OnInPortMouseUp 3");
                return;
            }

            var guid = data.Guid;
            var node = BaseNodeCreator.ABResourceGraph.GetNode(guid);
            //var inDegree = BaseNodeCreator.graph.GetInDegree(node);
            //Debug.LogError(AssetDatabase.GUIDToAssetPath(data.Guid) + " ----------------- in:");
            foreach (var item in node.ReferencedBy.Values)
            {
                //Debug.LogError(AssetDatabase.GUIDToAssetPath(item.ResourceId));
                //var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(AssetDatabase.GUIDToAssetPath(item.AddressableAssetRule));
                var asset = item.AddressableAssetRule;
                if (asset != null && asset.IsRuleUsed)
                {
                    _window.AddElement(asset);
                }
            }
        }

        internal virtual string[] GetDependencies(Object obj)
        {
            List<string> list = new List<string>();

            var prefabType = PrefabUtility.GetPrefabAssetType(obj);
            if (prefabType == PrefabAssetType.Variant)
            {
                var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
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

            string guidToPah = AssetDatabase.GetAssetPath(obj);
            var paths = AddressableCache.GetDependencies(guidToPah, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
            foreach (var path in paths)
            {
                if (!list.Contains(path))
                {
                    list.Add(path);
                }
            }

            return list.ToArray();
        }

        internal virtual void CreateDependencyNodes(string[] dependencies, Node parentNode,
    Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup, string dependentName, AddressableAssetEntry item = null)
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
                            edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencyString, dependentName) };
                            m_AssetConnections.Add(edge);
                        }
                        else
                        {
                            Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);
                            edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencyString) };
                            m_AssetConnections.Add(edge);
                        }

                    }
                }
            }

            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var _targetGroup = addressableAssetProfileSettings.FindGroup(rule.name);

            if (_targetGroup != null)
            {
                foreach (var item1 in _targetGroup.entries)
                {
                    foreach (var group in list)
                    {
                        if (this == group)
                        {
                            continue;
                        }

                        string dependencePath = null;
                        //if (group.IsReliance(item.AssetPath, out  dependencyNode, out dependencePath))
                        //{
                        //    Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);
                        //    edge.userData = new EdgeUserData("unknown", item.AssetPath);
                        //    edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencePath, dependentName) };
                        //    m_AssetConnections.Add(edge);
                        //}
                        com.igg.core.IGGDebug.LogError("not implement!");
                    }
                }
            }
        }

        protected virtual bool IsMultiNode(AddressableAssetRule rule)
        {
            return rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        }

        protected virtual string GetName(AddressableAssetRule rule)
        {
            return rule.name;
        }
    }
}