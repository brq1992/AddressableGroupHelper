using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using System;

namespace AddressableAssetTool.Graph
{
    internal class AddressablePackTogetherGroup : AddressableGraphBaseGroup
    {
        public AddressablePackTogetherGroup(Object obj, GraphWindow addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {

        }

        internal override void DrawGroup(GraphView m_GraphView, EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
            GraphWindow graphWindow)
        {
            _assetRulePath = AssetDatabase.GetAssetPath(_assetRuleObj);

            //assetPath will be empty if obj is null or isn't an asset (a scene object)
            if (string.IsNullOrEmpty(_assetRulePath))
                return;

            groupNode = new Group { title = _assetRuleObj.name };

            //string[] dependencies = GetDependencies();

            string assetGUID = AssetDatabase.AssetPathToGUID(_assetRulePath);
            int inDegree = -1;
            int outDegree = -1;
            //if (BaseNodeCreator.guidNodeDic.TryGetValue(assetGUID, out var dgNode))
            //{
            //    if (dgNode != null)
            //    {
            //        inDegree = BaseNodeCreator.graph.GetInDegree(dgNode).Capacity;
            //        outDegree = BaseNodeCreator.graph.GetOutDegree(dgNode).Capacity;

            //    }
            //}
            var graph = BaseNodeCreator.ABResourceGraph;
            var node = graph.GetNode(assetGUID);
            if(node != null)
            {
                inDegree = node.ReferencedBy.Count;
                outDegree = node.References.Count;
            }
            Node mainNode = CreateNode(_assetRuleObj, assetGUID, true, outDegree, graphWindow.m_GUIDNodeLookup, inDegree);
            mainNode.userData = new GraphViewNodeUserData() { Depth = 0, Guid = assetGUID };

            Rect position = new Rect(0, 0, 0, 0);
            //mainNode.SetPosition(position);
            graphWindow.AddAndPosMainNode(mainNode);

            groupChildNodes.Add(mainNode);

            //graphWindow.AddAndPosGroupNode(groupNode);
           

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
            if(group == null)
            {
                com.igg.core.IGGDebug.LogError(" group is null " + rule.name);
                return;
            }
            foreach (var item in group.entries)
            {
                string entryAssetPath = item.AssetPath;
                PrefabAssetType prefabType = PrefabAssetType.Regular;
                if (entryAssetPath.EndsWith(".prefab"))
                {

                }
                prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                {
                    List<string> dependenciesList = new List<string>();
                    var directDependencies = AddressableCache.GetVariantDependencies(item.AssetPath);
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                    var dependenciesAfterFilter = dependenciesList.ToArray();
                    CreateDependencyNodes(dependenciesAfterFilter, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, entryAssetPath);
                }
                else
                {
                    List<string> dependenciesList = new List<string>();
                    var directDependencies = AddressableCache.GetDependencies(entryAssetPath, false);
                    AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                    var dependenciesAfterFilter = dependenciesList.ToArray();
                    CreateDependencyNodes(dependenciesAfterFilter, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, entryAssetPath);
                }

                //IGGDebug.LogError("DrawGroup group.entries " + baseGroupList.Count);

                foreach (var baseGroup in baseGroupList)
                {
                    if (this == baseGroup)
                    {
                        continue;
                    }
                    if (baseGroup.IsReliance(item.AssetPath, out NodeDepenData[] data))
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            var isDependence = data[i].IsDependence;
                            var dependencyNode = data[i].DependencyGraphViewNode;
                            Edge edge = CreateEdge(mainNode, dependencyNode, m_GraphView);
                            List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>(); //new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
                            for (int j = 0; j < data[i].Dependencies.Length; j++)
                            {
                                string dependencePath = data[i].Dependencies[j];
                                edgeUserDatas.Add(new EdgeUserData(dependencePath, item.AssetPath));
                            }
                            edge.userData = edgeUserDatas;
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

        internal static void RemovePrefabInstanceOverrides(GameObject gameObject)
        {
            var components = gameObject.GetComponentsInChildren<Transform>(true);

            foreach (var component in components)
            {
                PrefabUtility.DisconnectPrefabInstance(component.gameObject);
            }
        }

        internal override void CreateDependencyNodes(string[] dependencies, Node parentNode,
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

                    if (group.IsDependence(dependencyString, out NodeDepenData[] data))
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            var isDependence = data[i].IsDependence;
                            var dependencyNode = data[i].DependencyGraphViewNode;
                            if (isDependence)
                            {
                                Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);
                                List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>(); //new List<EdgeUserData>() { new EdgeUserData(dependentName, dependencePath) };
                                for (int j = 0; j < data[i].Dependencies.Length; j++)
                                {
                                    string dependencePath = data[i].Dependencies[j];
                                    edgeUserDatas.Add(new EdgeUserData(dependentName, dependencePath));
                                }
                                edge.userData = edgeUserDatas;
                                m_AssetConnections.Add(edge);
                            }
                            else
                            {
                                Edge edge = CreateEdge(parentNode, dependencyNode, m_GraphView);
                                //edge.userData = new List<EdgeUserData>() { new EdgeUserData(dependencePath, dependentName) };
                                edge.userData = new List<EdgeUserData>() { new EdgeUserData("call jeff when you find this", "call jeff") };
                                //IGGDebug.Log(" dependencePath " + dependencePath + " dependentName " + dependentName);
                                m_AssetConnections.Add(edge);
                            }
                        }

                    }
                }
            }
        }

        internal override Node CreateNode(Object obj, string assetGUID, bool prefabCheck, int dependencyAmount, Dictionary<string, Node> m_GUIDNodeLookup,
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
                    title = obj.name.Substring(0, Math.Min(obj.name.Length, 30)),
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
                //IGGDebug.LogError(data.Guid);
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
                    port.userData =DGTool.GetNodeData(port.userData, assetGUID);
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

        private void OnOutPortMouseUp(MouseUpEvent evt)
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
            var inDegree = node.References;
            foreach (var item in inDegree)
            {
                var asset = item.Value.AddressableAssetRule;
                if (asset != null && asset.IsRuleUsed)
                {
                    _window.AddElement(asset);
                }
            }
        }

        private void OnInPortMouseUp(MouseUpEvent evt)
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
            var node =BaseNodeCreator.guidNodeDic[guid];
            var inDegree = BaseNodeCreator.graph.GetInDegree(node);
            foreach(var item in inDegree)
            {
                var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(AssetDatabase.GUIDToAssetPath(item.Id));
                if(asset != null && asset .IsRuleUsed)
                {
                    _window.AddElement(asset);
                }
            }
            
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

        internal override bool IsDependence(string dependencyString, out NodeDepenData[] data, AddressableAssetEntry item = null, string groupName = null)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            if (rule != null && DGTool.HasConnect(dependencyString, rule, out data, () => GetName(rule), () => IsMultiNode(rule)))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
            {
                string guid = AssetDatabase.AssetPathToGUID(_assetRulePath);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i].DependencyGraphViewNode = _window.m_GUIDNodeLookup[guid];
                }
                return true;
            }
            data = new NodeDepenData[0];
            return false;
        }

        internal override bool IsReliance(string assetPath, out NodeDepenData[] data, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item = null, string groupName = null)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            if (rule != null && DGTool.IsReliance(assetPath, rule, out data, () => GetName(rule), () => IsMultiNode(rule)))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
            {
                string guid = AssetDatabase.AssetPathToGUID(_assetRulePath);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i].DependencyGraphViewNode = _window.m_GUIDNodeLookup[guid];
                }
                return true;
            }
            data = new NodeDepenData[0];
            return false;
        }
    }
}