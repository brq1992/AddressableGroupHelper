using UnityEditor.Experimental.GraphView;

namespace AddressableAssetTool.Graph
{
    internal class NodeDepenData
    {
        public bool IsDependence { get; internal set; }
        public Node DependencyGraphViewNode { get; internal set; }
        public string[] Dependencies { get; internal set; }
        public string[] Guids { get; internal set; }
        public DirectedGraph.Node DependencyGraphNode { get; internal set; }
    }
}