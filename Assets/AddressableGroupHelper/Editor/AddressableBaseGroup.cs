
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace AddressableAssetTool.Graph
{
    internal class AddressableBaseGroup
    {
        internal Group groupNode;
        public Node mainNode = new Node();
        internal string assetPath;
        public List<GraphElement> m_AssetNodes = new List<GraphElement>();
        public List<GraphElement> m_AssetConnections = new List<GraphElement>();
        //public Dictionary<string, Node> m_GUIDNodeLookup = new Dictionary<string, Node>();
        public List<Node> m_DependenciesForPlacement = new List<Node>();

        internal string[] GetDependencies()
        {
            return AssetDatabase.GetDependencies(assetPath, false);
        }

        internal void CreateDependencyNodes(AddressableBaseGroup adGroup, string[] dependencies, Node mainNode, Group groupNode, int v)
        {
            throw new NotImplementedException();
        }
    }
}