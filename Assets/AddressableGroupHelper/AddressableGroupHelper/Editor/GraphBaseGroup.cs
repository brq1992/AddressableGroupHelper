﻿using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AddressableAssetTool.Graph
{
    internal abstract class GraphBaseGroup
    {
        public List<GraphElement> m_AssetNodes = new List<GraphElement>();

        public Group groupNode;

        protected Object obj;
        protected GraphWindow addressableDependenciesGraph;
        public List<GraphElement> m_AssetConnections = new List<GraphElement>();

        public List<Node> m_DependenciesForPlacement = new List<Node>();

        protected Object _assetRuleObj;

        public string _assetRulePath;

        protected List<Node> groupChildNodes = new List<Node>();
        protected readonly float kNodeWidth = AddressaableToolKey.Size.x;
        protected GraphWindow _window;

        public GraphBaseGroup(Object obj, GraphWindow addressableDependenciesGraph)
        {
            this.obj = obj;
            this.addressableDependenciesGraph = addressableDependenciesGraph;
        }

        internal abstract void UnregisterCallback(EventCallback<GeometryChangedEvent, GraphBaseGroup> updateGroupDependencyNodePlacement);

        internal abstract void SetPosition(Rect pos);
        internal abstract Rect GetMainNodePositoin();

        internal abstract void DrawGroup(GraphView m_GraphView, EventCallback<GeometryChangedEvent, GraphBaseGroup> UpdateGroupDependencyNodePlacement,
    GraphWindow graphWindow);

        internal abstract bool IsReliance(string assetPath, out Node dependencyNode);
        internal abstract bool IsReliance(string assetPath, out Node[] dependentNodes, out string[] dependentPaths);

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

        internal abstract bool IsDependence(string dependencyString, out NodeDepenData[] data, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item = null,
            string groupName = null);

        internal abstract bool IsReliance(string assetPath, out NodeDepenData[] data, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry item = null, 
            string groupName = null);
    }
}