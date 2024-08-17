using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AddressableAssetTool.Graph
{
    internal class AddressableHoleGroup : AddressableGraphBaseGroup
    {
        private AddressableAssetRule _target;
        private List<Node> mainNodes = new List<Node>();

        public AddressableHoleGroup(Object obj, AddressableDependenciesGraph addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
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

            string assetGUID = AssetDatabase.AssetPathToGUID(_assetRulePath);
            int inDegree = -1;
            int outDegree = -1;

            var graph = BaseNodeCreator.ABResourceGraph;
            var node = graph.GetNode(assetGUID);
            if (node != null)
            {
                inDegree = node.ReferencedBy.Count;
                outDegree = node.References.Count;
            }
            var mainNode = CreateNode(_assetRuleObj, _assetRulePath, true, outDegree, graphWindow.m_GUIDNodeLookup, inDegree);
            mainNode.userData = new GraphViewNodeUserData() { Depth = 0, Guid = assetGUID };

            Rect position = new Rect(0, 0, 0, 0);
            mainNode.SetPosition(position);
            groupChildNodes.Add(mainNode);

            //graphWindow.AddAndPosGroupNode(groupNode);
            graphWindow.AddAndPosMainNode(mainNode);

            if (!m_GraphView.Contains(groupNode))
            {
                m_GraphView.AddElement(groupNode);
            }
            m_GraphView.AddElement(mainNode);
            groupNode.AddElement(mainNode);

            var abNode = BaseNodeCreator.ABResourceGraph.GetNode(assetGUID);
            if(abNode != null)
            {
                Dictionary<string, Node> m_GUIDNodeLookup = _window.m_GUIDNodeLookup;

                var references = abNode.References;
                var referenceEnumerator = references.GetEnumerator();
                while (referenceEnumerator.MoveNext())
                {
                    var referenceNode = referenceEnumerator.Current.Value;
                    var referenceGUID = referenceNode.ResourceId;
                    if (m_GUIDNodeLookup.TryGetValue(referenceGUID, out var graphNode))
                    {
                        List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>();
                        var edgeData = BaseNodeCreator.ABResourceGraph.GetOutDegree(abNode, referenceNode);
                        for (int i = 0; i < edgeData.Count; i++)
                        {
                            edgeUserDatas.Add(new EdgeUserData(edgeData[i].Item1, edgeData[i].Item2));
                        }
                        Edge edge = CreateEdge(graphNode, mainNode, m_GraphView);
                        edge.userData = edgeUserDatas;
                        m_AssetConnections.Add(edge);
                    }
                }

                var referencesBy = abNode.ReferencedBy;
                var referenceByEnumerator = referencesBy.GetEnumerator();
                while (referenceByEnumerator.MoveNext())
                {
                    var referenceNode = referenceByEnumerator.Current.Value;
                    var referenceGUID = referenceNode.ResourceId;
                    if (m_GUIDNodeLookup.TryGetValue(referenceGUID, out var graphNode))
                    {
                        List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>();
                        var edgeData = BaseNodeCreator.ABResourceGraph.GetOutDegree(referenceNode, abNode);
                        for (int i = 0; i < edgeData.Count; i++)
                        {
                            edgeUserDatas.Add(new EdgeUserData(edgeData[i].Item1, edgeData[i].Item2));
                        }
                        Edge edge = CreateEdge(mainNode, graphNode, m_GraphView);
                        edge.userData = edgeUserDatas;
                        m_AssetConnections.Add(edge);
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

        internal override Node CreateNode(Object obj, string assetPath, bool prefabCheck, int dependencyAmount, Dictionary<string, Node> m_GUIDNodeLookup,
           int inDegree = 0)
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
                realPort.portName = inDegree + " Dependent";
                realPort.RegisterCallback<MouseUpEvent>(OnInPortMouseUp);
                realPort.userData = DGTool.GetNodeData(realPort.userData, assetGUID);
                var data = realPort.userData as GraphViewNodeUserData;
                //Debug.LogError(data.Guid);
                objNode.inputContainer.Add(realPort);
                //}

                //if (dependencyAmount > 0)
                {
#if UNITY_2018_1
                Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, typeof(Object));
#else
                    Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Object));
#endif
                    port.portName = dependencyAmount + " Dependencies";
                    port.userData = DGTool.GetNodeData(port.userData, assetGUID);
                    port.RegisterCallback<MouseUpEvent>(OnOutPortMouseUp);
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
            throw new System.NotImplementedException();
        }

        internal override bool IsReliance(string assetPath, out Node dependencyNode)
        {
            throw new System.NotImplementedException();
        }

        internal override bool IsReliance(string assetPath, out Node[] dependentNodes, out string[] dependentPaths)
        {
            throw new System.NotImplementedException();
        }

        internal override bool IsDependence(string dependencyString, out NodeDepenData[] data, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item =null)
        {
            throw new System.NotImplementedException();
        }

        internal override bool IsReliance(string assetPath, out NodeDepenData[] data, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item = null)
        {
            throw new System.NotImplementedException();
        }
    }
}