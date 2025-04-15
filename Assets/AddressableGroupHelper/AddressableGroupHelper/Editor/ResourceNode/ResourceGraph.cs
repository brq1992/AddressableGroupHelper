using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AddressableAssetTool.Graph
{
    public class ResourceGraph
    {
        private ConcurrentDictionary<string, ResourceNode> nodes;
        private Dictionary<(ResourceNode, ResourceNode), List<(string, string)>> adjacencyList;

        public ResourceGraph()
        {
            nodes = new ConcurrentDictionary<string, ResourceNode>();
            adjacencyList = new Dictionary<(ResourceNode, ResourceNode), List<(string, string)>>();
        }

        public ResourceNode GetOrCreateNode(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentException("Resource ID cannot be null or empty.");

            return nodes.GetOrAdd(resourceId, id => new ResourceNode(id));
        }

        public void AddReference(string fromResourceId, string toResourceId)
        {
            if (string.IsNullOrEmpty(fromResourceId) || string.IsNullOrEmpty(toResourceId))
                throw new ArgumentException("Resource ID cannot be null or empty.");

            var fromNode = GetOrCreateNode(fromResourceId);
            var toNode = GetOrCreateNode(toResourceId);
            fromNode.AddReference(toNode);

            if(!adjacencyList.ContainsKey((fromNode, toNode)))
                adjacencyList.Add((fromNode, toNode), new List<(string, string)>());
        }

        public ResourceNode GetNode(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                throw new ArgumentException("Resource ID cannot be null or empty.");

            nodes.TryGetValue(resourceId, out var node);
            return node;
        }

        public IEnumerable<ResourceNode> GetAllNodes()
        {
            return nodes.Values;
        }

        public List<List<string>> GetAllCircularReferences()
        {
            var allCircularReferences = new List<List<string>>();
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();
            var path = new Stack<string>();

            foreach (var node in GetAllNodes())
            {
                GetCircularReferencesRecursive(node, visited, stack, path, allCircularReferences);
            }

            return allCircularReferences;
        }

        private void GetCircularReferencesRecursive(ResourceNode node, HashSet<string> visited, HashSet<string> stack, Stack<string> path,
            List<List<string>> allCircularReferences)
        {
            if (stack.Contains(node.ResourceId))
            {
                var circularPath = path.Reverse().ToList();
                circularPath.Add(node.ResourceId);
                allCircularReferences.Add(circularPath);
                return;
            }

            if (visited.Contains(node.ResourceId))
            {
                return;
            }

            visited.Add(node.ResourceId);
            stack.Add(node.ResourceId);
            path.Push(node.ResourceId);

            foreach (var reference in node.References)
            {
                GetCircularReferencesRecursive(reference.Value, visited, stack, path, allCircularReferences);
            }

            stack.Remove(node.ResourceId);
            path.Pop();
        }

        public int GetReferenceDepth(string resourceId, out List<string> paths)
        {
            paths = new List<string>();
            var node = GetNode(resourceId);
            if (node == null)
                return 0;

            var visited = new HashSet<string>();
            return GetReferenceDepthRecursive(node, visited, paths);
        }

        private int GetReferenceDepthRecursive(ResourceNode node, HashSet<string> visited, List<string> paths)
        {
            if (visited.Contains(node.ResourceId))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(node.ResourceId));
                return 0;
            }
               

            visited.Add(node.ResourceId);
            int maxDepth = 0;

            foreach (var reference in node.References)
            {
                int depth = GetReferenceDepthRecursive(reference.Value, visited, paths);
                maxDepth = Math.Max(maxDepth, depth);
            }

            visited.Remove(node.ResourceId);
            return maxDepth + 1;
        }

        // 计算并输出所有节点的依赖层级
        public void PrintAllNodesDepth()
        {
            foreach (var node in GetAllNodes())
            {
                List<string> paths = new List<string>();
                int depth = GetReferenceDepth(node.ResourceId, out paths);
                string output = string.Join("-> " , paths);
                com.igg.core.IGGDebug.LogError($"Resource {AssetDatabase.GUIDToAssetPath(node.ResourceId)} depth: {--depth} paths {output}");
            }
        }

        internal void Clear()
        {
            var enumerator = nodes.GetEnumerator();
            while(enumerator.MoveNext())
            {
                enumerator.Current.Value.Clear();
            }
            nodes.Clear();
        }

        public void AddEdge(ResourceNode from, string fromName, ResourceNode to, string toName, float weight = 1.0f)
        {
            if(adjacencyList.ContainsKey((from, to)))
            {
                adjacencyList[(from, to)].Add((fromName, toName));
                return;
            }
            adjacencyList.Add((from, to), new List<(string, string)> { (fromName, toName) });


        }

        public void RemoveEdge(ResourceNode from, ResourceNode to)
        {
            if (adjacencyList.ContainsKey((from, to)))
            {
                adjacencyList[(from, to)].Clear();
            }
        }

        public List<(string, string)> GetOutDegree(ResourceNode from, ResourceNode to)
        {
            if (adjacencyList.TryGetValue((from, to), out var result))
            {
                return result;
            }
            return new List<(string, string)>();
        }
    }

    public partial class ResourceNode
    {
        public string ResourceId { get; private set; }
        public ConcurrentDictionary<string, ResourceNode> References { get; private set; }
        public ConcurrentDictionary<string, ResourceNode> ReferencedBy { get; private set; }

        public ResourceNode(string resourceId)
        {
            ResourceId = resourceId;
            References = new ConcurrentDictionary<string, ResourceNode>();
            ReferencedBy = new ConcurrentDictionary<string, ResourceNode>();

            ExConstruct();
        }

        public void AddReference(ResourceNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            References[node.ResourceId] = node;
            node.ReferencedBy[this.ResourceId] = this;
        }

        public bool HasReference(ResourceNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return References.ContainsKey(node.ResourceId);
        }

        public void RemoveReference(ResourceNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            References.TryRemove(node.ResourceId, out _);
            node.ReferencedBy.TryRemove(this.ResourceId, out _);
        }
    }
}