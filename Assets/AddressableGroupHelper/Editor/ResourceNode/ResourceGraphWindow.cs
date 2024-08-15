using AddressableAssetTool;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ResourceGraphWindow : EditorWindow
{
    private ResourceGraphBuilder graphBuilder;
    private ResourceGraph resourceGraph;

    [MenuItem("Tools/Build Resource Graph")]
    public static void ShowWindow()
    {
        GetWindow<ResourceGraphWindow>("Resource Graph Builder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Build Resource Graph"))
        {
            AddressableCache.CacheClear();
            BuildGraph();
        }

        if (resourceGraph != null)
        {
            //GUILayout.Label("Resource Graph:");
            //foreach (var node in resourceGraph.GetAllNodes())
            //{
            //    GUILayout.Label($"Resource {node.ResourceId} references:");
            //    foreach (var reference in node.References)
            //    {
            //        GUILayout.Label($"  - {reference.ResourceId}");
            //    }

            //    GUILayout.Label($"Resource {node.ResourceId} is referenced by:");
            //    foreach (var referencedBy in node.ReferencedBy)
            //    {
            //        if(string.IsNullOrEmpty(referencedBy.ResourceId))
            //        {
            //            Debug.LogError("null!");
            //        }
            //        //GUILayout.Label($"  - {referencedBy.ResourceId}");
            //    }
            //}
        }
    }

    private void BuildGraph()
    {
        graphBuilder = new ResourceGraphBuilder();
        graphBuilder.BuildGraph();
        resourceGraph = graphBuilder.GetResourceGraph();

        var circularReferences = resourceGraph.GetAllCircularReferences();
        Debug.LogError("Circular References:");
        foreach (var circularPath in circularReferences)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var guid in circularPath)
            {
                builder.Append(AssetDatabase.GUIDToAssetPath(guid) + " -> ");
            }
            Debug.LogError(builder.ToString());
        }

        // 计算并输出所有节点的依赖层级
        //resourceGraph.PrintAllNodesDepth();
    }
}
