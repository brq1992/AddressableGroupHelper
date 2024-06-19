using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

namespace AddressableAssetTool.Graph
{
    internal abstract class AddressableGraphBaseGroup
    {
        public List<GraphElement> m_AssetConnections = new List<GraphElement>();
        public List<GraphElement> m_AssetNodes = new List<GraphElement>();
        public List<Node> m_DependenciesForPlacement = new List<Node>();

        protected Object _assetRuleObj;

        public string _assetRulePath;
        internal Group groupNode;
        protected List<Node> groupChildNodes = new List<Node>();
        protected readonly float kNodeWidth = AddressaableToolKey.Size.x;
        protected AddressableDependenciesGraph _window;

        public AddressableGraphBaseGroup(Object obj, AddressableDependenciesGraph addressableDependenciesGraph)
        {
            this._assetRuleObj = obj;
            _window = addressableDependenciesGraph;
        }

        internal abstract void SetPosition(Rect pos);

        internal abstract void DrawGroup(GraphView m_GraphView, EventCallback<GeometryChangedEvent, AddressableGraphBaseGroup> UpdateGroupDependencyNodePlacement,
            AddressableDependenciesGraph graphWindow);

        internal virtual bool IsDependence(string dependencyString, out bool isDependence, out string dependencePath)
        {
            isDependence = false;
            dependencePath = null;
            return false;
        }

        internal virtual bool IsDependence(string dependencyString, out bool isDependence, out Node dependencyNode, out string dependencePath)
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
            Debug.LogError("edge click");
        }

        internal abstract bool IsReliance(string assetPath, out Node[] dependentNodes, out string[] dependentPaths);

        internal string[] GetDependencies()
        {
            throw new NotImplementedException();
        }

        internal abstract bool IsReliance(string assetPath, out Node dependencyNode);


        internal Rect GetMainNodePositoin()
        {
            return new Rect(0, 0, 0, 0);
        }

        internal virtual void UnregisterCallback(EventCallback<GeometryChangedEvent, AddressableGraphBaseGroup> updateGroupDependencyNodePlacement)
        {
            foreach (var item in groupChildNodes)
            {
                item.UnregisterCallback<GeometryChangedEvent, AddressableGraphBaseGroup>(
                    updateGroupDependencyNodePlacement
                );
            }
        }

        internal virtual Node CreateNode(Object obj, string assetPath, bool prefabCheck, int dependencyAmount, Dictionary<string, Node> m_GUIDNodeLookup)
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
    Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup, string dependentName)
        {
            List<AddressableGraphBaseGroup> list = _window._addressableGroups;

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
                foreach (var item in _targetGroup.entries)
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
                        Debug.LogError("not implement!");
                    }
                }
            }
        }
    }
}