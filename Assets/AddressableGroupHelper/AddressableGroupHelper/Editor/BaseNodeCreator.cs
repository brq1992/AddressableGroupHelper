

using com.igg.editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AddressableAssetTool.Graph
{
    internal abstract class BaseNodeCreator
    {
        protected AddressableAssetRule asset;

        internal static bool Init = false;

        internal static DirectedGraph.DirectedGraph graph = new DirectedGraph.DirectedGraph();
       
        internal static Dictionary<string, DirectedGraph.Node> guidNodeDic = new Dictionary<string, DirectedGraph.Node>();

        protected AddressableDependenciesGraph _window;

        internal abstract void CreateNode(string assetGUID, AddressableDependenciesGraph addressableDependenciesGraph);

        internal virtual void CreateDependencyNodes(string[] dependencies, string guid, DirectedGraph.Node parentNode)
        {
            List<GraphBaseGroup> list = _window._addressableGroups;

            foreach (string dependencyString in dependencies)
            {
                foreach (var item in guidNodeDic)
                {
                    if (guid == item.Key)
                    {
                        continue;
                    }

                    AddressableAssetRule rule = item.Value.Rule;
                    if (rule != null && DGTool.HasConnect(dependencyString, rule, out NodeDepenData[] data))// rule.HasConnenct(dependencyString, out isDependence, out edgeUserData))
                    {
                        string path = AssetDatabase.GetAssetPath(rule);
                        string assetRuleGuid = AssetDatabase.AssetPathToGUID(path);
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackTogether)
                                data[i].DependencyGraphNode = guidNodeDic[assetRuleGuid];
                            else
                                data[i].DependencyGraphNode = guidNodeDic[data[i].Guids[0]];
                        }

                        for (int i = 0; i < data.Length; i++)
                        {
                            var isDependence = data[i].IsDependence;
                            var dependencyNode = data[i].DependencyGraphNode;
                            if (isDependence)
                            {
                                graph.AddEdge(parentNode, dependencyNode);
                            }
                            else
                            {
                                graph.AddEdge(dependencyNode, parentNode);
                            }
                        }
                    }
                    
                }
            }
        }





        #region NewNode

        internal static bool NewNodeInit = false;

        internal static ResourceGraph ABResourceGraph = new ResourceGraph();
        internal static void ClearABGraphData()
        {
            NewNodeInit = false;
            ABResourceGraph.Clear();
        }

        #endregion
    }
}