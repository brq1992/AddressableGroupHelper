
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AddressableAssetTool.DirectedGraph
{
    public class DirectedGraph
    {
        private Dictionary<Node, List<Edge>> adjacencyList;
        private Dictionary<Node, List<Edge>> reverseAdjacencyList;

        public DirectedGraph()
        {
            adjacencyList = new Dictionary<Node, List<Edge>>();
            reverseAdjacencyList = new Dictionary<Node, List<Edge>>();
        }

        public void AddNode(Node node)
        {
            if (!adjacencyList.ContainsKey(node))
            {
                adjacencyList[node] = new List<Edge>();
            }
            if (!reverseAdjacencyList.ContainsKey(node))
            {
                reverseAdjacencyList[node] = new List<Edge>();
            }
        }

        public void RemoveNode(Node node)
        {
            if (adjacencyList.ContainsKey(node))
            {
                foreach (var edge in adjacencyList[node])
                {
                    reverseAdjacencyList[edge.To].Remove(edge);
                }
                adjacencyList.Remove(node);
            }

            if (reverseAdjacencyList.ContainsKey(node))
            {
                foreach (var edge in reverseAdjacencyList[node])
                {
                    adjacencyList[edge.From].Remove(edge);
                }
                reverseAdjacencyList.Remove(node);
            }
        }

        public void AddEdge(Node from, Node to, float weight = 1.0f)
        {
            AddNode(from);
            AddNode(to);
            Edge edge = new Edge(from, to, weight);
            adjacencyList[from].Add(edge);
            reverseAdjacencyList[to].Add(edge);
        }

        public void RemoveEdge(Node from, Node to)
        {
            if (adjacencyList.ContainsKey(from))
            {
                adjacencyList[from].RemoveAll(edge => edge.To.Equals(to));
            }
            if (reverseAdjacencyList.ContainsKey(to))
            {
                reverseAdjacencyList[to].RemoveAll(edge => edge.From.Equals(from));
            }
        }

        public List<Node> GetOutDegree(Node node)
        {
            if (adjacencyList.ContainsKey(node))
            {
                return adjacencyList[node].Select(edge => edge.To).ToList();
            }
            return new List<Node>();
        }

        public List<Node> GetInDegree(Node node)
        {
            if (reverseAdjacencyList.ContainsKey(node))
            {
                return reverseAdjacencyList[node].Select(edge => edge.From).ToList();
            }
            return new List<Node>();
        }

        public void PrintGraph()
        {
            com.igg.core.IGGDebug.LogError("Adjacency List:");
            foreach (var kvp in adjacencyList)
            {
                com.igg.core.IGGDebug.LogError(kvp.Key.Name + " -> ");
                foreach (var edge in kvp.Value)
                {
                    com.igg.core.IGGDebug.LogError(edge.To.Name + "(" + edge.Weight + ") ");
                }
            }

            com.igg.core.IGGDebug.LogError("Reverse Adjacency List:");
            foreach (var kvp in reverseAdjacencyList)
            {
                com.igg.core.IGGDebug.LogError(kvp.Key.Name + " <- ");
                foreach (var edge in kvp.Value)
                {
                    com.igg.core.IGGDebug.LogError(edge.From.Name + "(" + edge.Weight + ") ");
                }
            }
        }

        internal void Clear()
        {
            adjacencyList.Clear();
            reverseAdjacencyList.Clear();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            DirectedGraph graph = new DirectedGraph();

            // ????
            //Node node1 = new Node(, "Node1");
            //Node node2 = new Node(2, "Node2");
            //Node node3 = new Node(3, "Node3");
            //Node node4 = new Node(4, "Node4");

            //// ????
            //graph.AddNode(node1);
            //graph.AddNode(node2);
            //graph.AddNode(node3);
            //graph.AddNode(node4);

            //// ?????
            //graph.AddEdge(node1, node2, 1.5f);
            //graph.AddEdge(node1, node3, 2.0f);
            //graph.AddEdge(node2, node3, 2.5f);
            //graph.AddEdge(node3, node4, 3.0f);

            // ?????
            graph.PrintGraph();

            // ?????????
            //Console.WriteLine("Node1 Out-Degree: " + string.Join(", ", graph.GetOutDegree(node1).Select(n => n.Name)));

            //// ?????????
            //Console.WriteLine("Node3 In-Degree: " + string.Join(", ", graph.GetInDegree(node3).Select(n => n.Name)));

            // ???
            //graph.RemoveEdge(node1, node3);

            //// ????
            //graph.RemoveNode(node2);

            // ?????????
            graph.PrintGraph();
        }
    }

    public class Node
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public AddressableAssetRule Rule { get; internal set; }

        public Node(string id, string name = "")
        {
            Id = id;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Id == ((Node)obj).Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


    }

    public class Edge
    {
        public Node From { get; private set; }
        public Node To { get; private set; }
        public float Weight { get; set; } 

        public Edge(Node from, Node to, float weight = 1.0f)
        {
            From = from;
            To = to;
            Weight = weight;
        }
    }


}
