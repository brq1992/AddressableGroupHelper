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
    internal class AddressableFolderGroup : AddressablePackTogetherGroup
    {
        private string[] _diChildGuids;

        public AddressableFolderGroup(UnityEngine.Object obj, GraphWindow addressableDependenciesGraph) : base(obj, addressableDependenciesGraph)
        {
        }

        internal override void DrawGroup(GraphView m_GraphView, EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
            GraphWindow graphWindow)
        {
            _assetRulePath = AssetDatabase.GetAssetPath(_assetRuleObj);
            var dic = Path.GetDirectoryName(_assetRulePath);
            string filter = "t:Object";
            _diChildGuids = AddressabelUtilities.FindDirectChildren(filter, dic);
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;

            Dictionary<string, Node> tempDic = new Dictionary<string, Node>();
            foreach (var diGuid in _diChildGuids)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(diGuid);
                var groupName = AddresaableGroupBuildUtilities.GetFolderEntryName(rule.name, groupPath);
                //Debug.LogError(groupPath + " " +groupName);
                //AddressableCache.AddGroupNameAndGuid(groupName, diGuid);
                groupNode = new Group { title = groupName };
                string assetGUID = diGuid;// AssetDatabase.AssetPathToGUID(_assetRulePath);
                int inDegree = -1;
                int outDegree = -1;
                var graph = BaseNodeCreator.ABResourceGraph;
                var node = graph.GetNode(assetGUID);
                if (node != null)
                {
                    inDegree = node.ReferencedBy.Count;
                    outDegree = node.References.Count;
                }
                Node mainNode = CreateNode(_assetRuleObj, assetGUID, true, outDegree, graphWindow.m_GUIDNodeLookup, inDegree);
                tempDic.Add(diGuid, mainNode);
                mainNode.userData = new GraphViewNodeUserData() { Depth = 0, Guid = assetGUID };

                Rect position = new Rect(0, 0, 0, 0);
                graphWindow.AddAndPosMainNode(mainNode);
                groupChildNodes.Add(mainNode);


                if (!m_GraphView.Contains(groupNode))
                {
                    m_GraphView.AddElement(groupNode);
                }
                m_GraphView.AddElement(mainNode);
                groupNode.AddElement(mainNode);

                List<GraphBaseGroup> baseGroupList = _window._addressableGroups;

                m_AssetNodes.Add(mainNode);

                groupNode.capabilities &= ~Capabilities.Deletable;

                groupNode.Focus();

                mainNode.RegisterCallback<GeometryChangedEvent, GraphBaseGroup>(
                    UpdateGroupDependencyNodePlacement, this
                );
            }

            foreach (var diGuid in _diChildGuids)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(diGuid);
                var groupName = AddresaableGroupBuildUtilities.GetFolderEntryName(rule.name, groupPath);
                List<GraphBaseGroup> baseGroupList = _window._addressableGroups;

                var group = setting.FindGroup(groupName);
                if (group == null)
                {
                    com.igg.core.IGGDebug.LogError(" group is null " + groupName);
                    continue;
                }

                var mainNode = tempDic[diGuid];

                foreach (var item in group.entries)
                {
                    string entryAssetPath = item.AssetPath;
                    PrefabAssetType prefabType = PrefabAssetType.Regular;
                    prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                    string[] dependenciesAfterFilter = null;
                    List<string> dependenciesList = new List<string>();
                    if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                    {

                        var directDependencies = AddressableCache.GetVariantDependencies(item.AssetPath);
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        dependenciesAfterFilter = dependenciesList.ToArray();
                    }
                    else
                    {
                        var directDependencies = AddressableCache.GetDependencies(entryAssetPath, false);
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        dependenciesAfterFilter = dependenciesList.ToArray();
                    }

                    
                    CreateDependencyNodes(dependenciesAfterFilter, mainNode, groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup, entryAssetPath, groupName);

                    foreach (var baseGroup in baseGroupList)
                    {
                        bool setGroupName = false;
                        if (this == baseGroup)
                        {
                            setGroupName = true;
                        }

                        if (baseGroup.IsReliance(item.AssetPath, out NodeDepenData[] data, groupName: setGroupName ? groupName : null))
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
            }
        }

        internal void CreateDependencyNodes(string[] dependencies, Node parentNode,
    Group groupNode, int depth, GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup, string dependentName, string groupName)
        {
            List<GraphBaseGroup> list = _window._addressableGroups;

            foreach (string dependencyString in dependencies)
            {
                foreach (var group in list)
                {
                    bool setGroupName = false;
                    if (this == group)
                    {
                        setGroupName = true;
                    }

                    if (group.IsDependence(dependencyString, out NodeDepenData[] data, groupName: setGroupName ? groupName : null))
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
                                edge.userData = new List<EdgeUserData>() { new EdgeUserData("call jeff when you find this", "call jeff") };
                                m_AssetConnections.Add(edge);
                            }
                        }

                    }
                }
            }
        }

        internal override bool IsDependence(string dependencyString, out NodeDepenData[] data, AddressableAssetEntry item = null, string groupName = null)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            foreach (var diGuid in _diChildGuids)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(diGuid);
                var groupName1 = AddresaableGroupBuildUtilities.GetFolderEntryName(rule.name, groupPath);
                if(groupName == groupName1)
                {
                    continue;
                }

                if (DGTool.HasConnect(dependencyString, rule, out data, () => { return groupName1; }, () => { return false; }))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i].DependencyGraphViewNode = _window.m_GUIDNodeLookup[diGuid];
                    }
                    return true;
                }
            }
            data = new NodeDepenData[0];
            return false;
        }

        internal override bool IsReliance(string dependencyString, out NodeDepenData[] data, AddressableAssetEntry item = null, string groupName = null)
        {
            AddressableAssetRule rule = _assetRuleObj as AddressableAssetRule;
            foreach (var diGuid in _diChildGuids)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(diGuid);
                var groupName1 = AddresaableGroupBuildUtilities.GetFolderEntryName(rule.name, groupPath);
                if (groupName == groupName1)
                {
                    continue;
                }
                if (DGTool.IsReliance(dependencyString, rule, out data, () => { return groupName1; }, () => { return false; }))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i].DependencyGraphViewNode = _window.m_GUIDNodeLookup[diGuid];
                    }
                    return true;
                }
            }
            data = new NodeDepenData[0];
            return false;
        }
    }
}