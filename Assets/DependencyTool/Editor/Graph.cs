using System.Collections.Generic;
using UnityEngine;

public class Node<T>
{
    public int Id { get; private set; }

    //public List<Edge> Edges { get; private set; }
    private Dictionary<int, Edge<T>> _edgesDic;

    public T Value { get; private set; }

    public Node(int id, T obj)
    {
        Id = id;
        Value = obj;
        _edgesDic = new Dictionary<int, Edge<T>>();

    }

    public void AddEdge(Node<T> target, int weight = 1)
    {
        //if(target == null)
        //{
        //    throw new System.Exception("Node target can't be null!");
        //}
        //if(target == this)
        //{
        //    throw new System.Exception("Can't add node to itself!");
        //}
        //Edges.Add(new Edge(this, target, weight));
        if (!_edgesDic.ContainsKey(target.Id))
        {
            _edgesDic.Add(target.Id, new Edge<T>(this, target, weight));

        }
    }
}

public class Edge<T>
{
    public Node<T> Source { get; private set; }
    public Node<T> Target { get; private set; }
    public int Weight { get; private set; }

    public Edge(Node<T> source, Node<T> target, int weight)
    {
        Source = source;
        Target = target;
        Weight = weight;
        //Debug.LogError("Add node " + source.Resource.name + " new edge -----> " + target.Resource.name);
    }
}

public class Graph<T>
{
    private Dictionary<int, Node<T>> _nodes;

    public Graph()
    {
        _nodes = new Dictionary<int, Node<T>>();
    }

    public Node<T> AddNode(int id, T obj)
    {
        var node = new Node<T>(id, obj);

        if (!_nodes.ContainsKey(id))
        {

            _nodes.Add(id, node);
        }
        return node;
    }

    public void AddEdge(int sourceId, int targetId, int weight = 1)
    {
        Node<T> source;
        Node<T> target;

        if (_nodes.TryGetValue(sourceId, out source) && _nodes.TryGetValue(targetId, out target))
        {
            source.AddEdge(target, weight);
        }
    }

    public Node<T> GetNode(int id)
    {
        Node<T> node;
        _nodes.TryGetValue(id, out node);
        return node;
    }

    public void OutputAllNode()
    {
        foreach (var node in _nodes)
        {
            Debug.LogError("node " + node.Key);
        }
    }
}
