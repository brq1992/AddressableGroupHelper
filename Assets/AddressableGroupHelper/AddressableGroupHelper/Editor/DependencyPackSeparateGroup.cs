using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.AddressableAssets.Settings;

namespace AddressableAssetTool.Graph
{
    internal class DependencyPackSeparateGroup : AddressableGraphBaseGroup
    {
        private AddressableAssetRule _target;

        public DependencyPackSeparateGroup(Object obj, GraphWindow addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
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

            //List<GraphBaseGroup> graphBaseGroupList = _window._addressableGroups;

            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(_target.name);
            int count = 0;
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    int inDegree = -1;
                    int outDegree = -1;
                    var graph = BaseNodeCreator.ABResourceGraph;
                    var abNode = graph.GetNode(item.guid);
                    if (abNode != null)
                    {
                        inDegree = abNode.ReferencedBy.Count;
                        outDegree = abNode.References.Count;
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

                    if (abNode != null)
                    {
                        Dictionary<string, Node> m_GUIDNodeLookup = _window.m_GUIDNodeLookup;

                        var references = abNode.References;
                        var referenceEnumerator = references.GetEnumerator();
                        while (referenceEnumerator.MoveNext())
                        {
                            var referenceNode = referenceEnumerator.Current.Value;
                            var referenceGUID = referenceNode.ResourceId;
                            if (m_GUIDNodeLookup.TryGetValue(referenceGUID, out var referenceGraphNode))
                            {
                                List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>();
                                var edgeData = BaseNodeCreator.ABResourceGraph.GetOutDegree(abNode, referenceNode);
                                for (int i = 0; i < edgeData.Count; i++)
                                {
                                    edgeUserDatas.Add(new EdgeUserData(edgeData[i].Item1, edgeData[i].Item2));
                                }
                                Edge edge = CreateEdge(referenceGraphNode, node, m_GraphView);
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
                            if (m_GUIDNodeLookup.TryGetValue(referenceGUID, out var referenceByGraphNode))
                            {
                                List<EdgeUserData> edgeUserDatas = new List<EdgeUserData>();
                                var edgeData = BaseNodeCreator.ABResourceGraph.GetOutDegree(referenceNode, abNode);
                                for (int i = 0; i < edgeData.Count; i++)
                                {
                                    edgeUserDatas.Add(new EdgeUserData(edgeData[i].Item1, edgeData[i].Item2));
                                }
                                Edge edge = CreateEdge(node, referenceByGraphNode, m_GraphView);
                                edge.userData = edgeUserDatas;
                                m_AssetConnections.Add(edge);
                            }
                        }
                    }

                    count++;
                }
            }
        }

        internal override bool IsDependence(string dependencyString, out NodeDepenData[] data, AddressableAssetEntry item = null, string groupName = null)
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

        internal override bool IsReliance(string assetPath, out NodeDepenData[] data, AddressableAssetEntry item = null, string groupName = null)
        {
            throw new System.NotImplementedException();
        }

        internal override void SetPosition(Rect pos)
        {
        }
    }
}