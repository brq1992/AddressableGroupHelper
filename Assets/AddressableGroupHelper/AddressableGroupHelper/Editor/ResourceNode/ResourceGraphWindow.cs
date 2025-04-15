
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AddressableAssetTool.Graph
{
    public class ResourceGraphWindow : EditorWindow
    {
        private ResourceGraphBuilder graphBuilder;
        private ResourceGraph resourceGraph;

        [MenuItem("Tools/Test/Build Resource Graph")]
        public static void ShowWindow()
        {
            GetWindow<ResourceGraphWindow>("Resource Graph Build Tester");
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
                //            com.igg.core.IGGDebug.LogError("null!");
                //        }
                //        //GUILayout.Label($"  - {referencedBy.ResourceId}");
                //    }
                //}
            }

            if (GUILayout.Button("Check Sprite Circle Reference"))
            {
                AddressableCache.CacheClear();
                graphBuilder = new ResourceGraphBuilder();
                graphBuilder.BuildGraph();
                resourceGraph = graphBuilder.GetResourceGraph();
                var nodeEnumerator = resourceGraph.GetAllNodes().GetEnumerator();
                while(nodeEnumerator.MoveNext())
                {

                }
            }
        }

        private void BuildGraph()
        {
            graphBuilder = new ResourceGraphBuilder();
            graphBuilder.BuildGraph();
            resourceGraph = graphBuilder.GetResourceGraph();

            var circularReferences = resourceGraph.GetAllCircularReferences();
            com.igg.core.IGGDebug.LogError("Circular References:");
            foreach (var circularPath in circularReferences)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var guid in circularPath)
                {
                    builder.Append(AssetDatabase.GUIDToAssetPath(guid) + " -> ");
                }
                com.igg.core.IGGDebug.LogError(builder.ToString());
            }

            // 
            resourceGraph.PrintAllNodesDepth();

            //

        }
    }


    public class ABResourceGraphWindow : EditorWindow
    {
        private ResourceGraphBuilder graphBuilder;
        private ResourceGraph resourceGraph;

        [MenuItem("Tools/Build AB Resource Graph")]
        public static void ShowWindow()
        {
            GetWindow<ABResourceGraphWindow>("AB Resource Graph Builder");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Start Check"))
            {
                AddressableCache.CacheClear();
                BuildGraph();
            }
        }

        private void BuildGraph()
        {
            AddressableDependenciesGraph.CheckReference();

            AddressableDependenciesGraph.CheckReferenceDepth();



        }
    }

}