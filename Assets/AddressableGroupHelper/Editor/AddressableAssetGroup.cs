
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;
using Object = UnityEngine.Object;

namespace AddressableAssetTool.Graph
{
    internal class AddressableAssetGroup : GraphBaseGroup
    {
        public Node mainNode = new Node();

        public AddressableAssetGroup(Object obj, AddressableDependenciesGraph addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {
            this.obj = obj;
            _window = addressableDependenciesGraph;
        }

        internal virtual string[] GetDependencies()
        {
            throw new NotImplementedException();
        }


        internal override void DrawGroup(GraphView m_GraphView,
             EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement, AddressableDependenciesGraph graphWindow)
        {
            _assetRulePath = AssetDatabase.GetAssetPath(obj);

            groupNode = new Group { title = obj.name };

            string[] dependencies = AddressableCache.GetDependencies(_assetRulePath, false);

            mainNode = CreateNode(this, obj, _assetRulePath, true, dependencies.Length, graphWindow.m_GUIDNodeLookup);
            mainNode.userData = 0;

            Rect position = new Rect(0, 0, 0, 0);
            mainNode.SetPosition(position);

            if (!m_GraphView.Contains(groupNode))
            {
                m_GraphView.AddElement(groupNode);
            }

            m_GraphView.AddElement(mainNode);

            groupNode.AddElement(mainNode);

            CreateDependencyNodes(this, dependencies, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup);

            m_AssetNodes.Add(mainNode);

            groupNode.capabilities &= ~Capabilities.Deletable;

            groupNode.Focus();

            mainNode.RegisterCallback<GeometryChangedEvent, AddressableAssetGroup>(
                UpdateGroupDependencyNodePlacement, this
            );
        }

        internal virtual Node CreateNode(AddressableAssetGroup AddressableGroup, Object obj, string assetPath, bool isMainNode,
            int dependencyAmount, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            Node resultNode;
            string assetGUID = AssetDatabase.AssetPathToGUID(assetPath) + "_copy";
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
                if (isMainNode)
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

        internal virtual void CreateDependencyNodes(AddressableAssetGroup AddressableGroup, string[] dependencies, Node parentNode, Group groupNode, int depth,
            GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            //Debug.Log(depth);

            foreach (string dependencyString in dependencies)
            {
                Object dependencyAsset = AssetDatabase.LoadMainAssetAtPath(dependencyString);
                string[] deeperDependencies = AddressableCache.GetDependencies(dependencyString, false);

                var typeName = dependencyAsset.GetType().Name;


                Node dependencyNode = CreateNode(AddressableGroup, dependencyAsset, AssetDatabase.GetAssetPath(dependencyAsset),
                    false, deeperDependencies.Length, m_GUIDNodeLookup);

                if (!AddressableGroup.m_AssetNodes.Contains(dependencyNode))
                {
                    dependencyNode.userData = depth;
                }

                CreateDependencyNodes(AddressableGroup, deeperDependencies, dependencyNode, groupNode, depth + 1, m_GraphView, m_GUIDNodeLookup);

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
                //edge.userData = new EdgeData();

                AddressableGroup.m_AssetConnections.Add(edge);
                AddressableGroup.m_AssetNodes.Add(dependencyNode);
            }
        }

        internal virtual Edge CreateEdge(Node dependencyNode, Node parentNode, GraphView m_GraphView)
        {
            Edge edge = new Edge
            {
                input = dependencyNode.inputContainer[0] as Port,
                output = parentNode.outputContainer[0] as Port,
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

        internal virtual bool IsDependence(string dependencyString, out bool isDependence, out Node dependencyNode)
        {
            if (dependencyString.Equals(_assetRulePath))
            {
                dependencyNode = mainNode;
                isDependence = true;
                return true;
            }
            isDependence = false;
            dependencyNode = null;
            return false;
        }

        internal override bool IsReliance(string dependencyString, out Node dependencyNode)
        {
            if (dependencyString.Equals(_assetRulePath))
            {
                dependencyNode = mainNode;
                return true;
            }
            dependencyNode = null;
            return false;
        }


        internal override void SetPosition(Rect pos)
        {
            mainNode.SetPosition(pos);
        }

        internal override void UnregisterCallback(EventCallback<GeometryChangedEvent, GraphBaseGroup> updateGroupDependencyNodePlacement)
        {
            mainNode.UnregisterCallback<GeometryChangedEvent, AddressableGraphBaseGroup>(
                    updateGroupDependencyNodePlacement
                );
        }

        internal override bool IsReliance(string assetPath, out Node[] dependencyNode, out string[] dependencePath)
        {
            if (assetPath.Equals(_assetRulePath))
            {
                dependencyNode = new[] { mainNode };
                dependencePath = new[] { _assetRulePath };
                return true;
            }

            dependencyNode = null;
            dependencePath = null;
            return false;
        }

        internal override Rect GetMainNodePositoin()
        {
            return mainNode.GetPosition();
        }

        internal override bool IsDependence(string dependencyString, out NodeDepenData[] data , UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item = null)
        {
            throw new NotImplementedException();
        }

        internal override bool IsReliance(string assetPath, out NodeDepenData[] data, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item = null)
        {
            throw new NotImplementedException();
        }
    }

    
}