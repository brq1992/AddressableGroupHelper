using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ResourceGraph
{
    private ConcurrentDictionary<string, ResourceNode> nodes;

    public ResourceGraph()
    {
        nodes = new ConcurrentDictionary<string, ResourceNode>();
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
        string assetPath = AssetDatabase.GUIDToAssetPath(node.ResourceId);
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

    public int GetReferenceDepth(string resourceId)
    {
        var node = GetNode(resourceId);
        if (node == null)
            return 0;

        var visited = new HashSet<string>();
        return GetReferenceDepthRecursive(node, visited);
    }

    private int GetReferenceDepthRecursive(ResourceNode node, HashSet<string> visited)
    {
        if (visited.Contains(node.ResourceId))
            return 0;

        visited.Add(node.ResourceId);
        int maxDepth = 0;

        foreach (var reference in node.References)
        {
            int depth = GetReferenceDepthRecursive(reference.Value, visited);
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
            int depth = GetReferenceDepth(node.ResourceId);
            Debug.LogError($"Resource {node.ResourceId} depth: {depth}");
        }
    }
}

public class ResourceNode
{
    public string ResourceId { get; private set; }
    public ConcurrentDictionary<string, ResourceNode> References { get; private set; }
    public ConcurrentDictionary<string, ResourceNode> ReferencedBy { get; private set; }

    public ResourceNode(string resourceId)
    {
        ResourceId = resourceId;
        References = new ConcurrentDictionary<string, ResourceNode>();
        ReferencedBy = new ConcurrentDictionary<string, ResourceNode>();
    }

    public void AddReference(ResourceNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        //Debug.LogError("node " + this.ResourceId + " add " + node.ResourceId);
        References[node.ResourceId] = node;
        node.ReferencedBy[this.ResourceId] = this;
        //Debug.LogError("node " + node.ResourceId + " referby " + this.ResourceId);
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
