using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using com.igg.editor;

namespace AddressableAssetTool.Graph
{
    internal class DependencyPackFolderGroup : AddressableGraphBaseGroup
    {
        public DependencyPackFolderGroup(Object obj, GraphWindow addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {
        }

        internal override void DrawGroup(GraphView m_GraphView,
            EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
            GraphWindow graphWindow)
        {
            _assetRulePath = AssetDatabase.GetAssetPath(_assetRuleObj);
            var dic = Path.GetDirectoryName(_assetRulePath);
            string filter = "t:Object";
            var _diChildGuids = AddressabelUtilities.FindDirectChildren(filter, dic);
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;

            groupNode = new Group { title = rule.name };

            Rect position = new Rect(0, 0, 0, 0);

            if (!m_GraphView.Contains(groupNode))
            {
                m_GraphView.AddElement(groupNode);
            }

            groupNode.capabilities &= ~Capabilities.Deletable;

            groupNode.Focus();
            int count = 0;
            //List<GraphBaseGroup> graphBaseGroupList = _window._addressableGroups;
            foreach (var diGuid in _diChildGuids)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(diGuid);
                var groupName = AddresaableGroupBuildUtilities.GetFolderEntryName(rule.name, groupPath);
                var group = setting.FindGroup(groupName);
                if (group == null)
                {
                    com.igg.core.IGGDebug.LogError(" group is null " + groupName);
                    continue;
                }

                int inDegree = -1;
                int outDegree = -1;
                var graph = BaseNodeCreator.ABResourceGraph;
                var abNode = graph.GetNode(diGuid);
                if (abNode != null)
                {
                    inDegree = abNode.ReferencedBy.Count;
                    outDegree = abNode.References.Count;
                }

                Object selectObj = AssetDatabase.LoadAssetAtPath<Object>(groupPath);

                var node = CreateNode(selectObj, diGuid, true, outDegree, graphWindow.m_GUIDNodeLookup, inDegree);
                DGTool.SetNodeData(node.userData, 0);
                position = BaseLayout.GetNewNodePostion(count);
                node.SetPosition(position);
                groupNode.AddElement(node);
                m_GraphView.AddElement(node);
                m_AssetNodes.Add(node);

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