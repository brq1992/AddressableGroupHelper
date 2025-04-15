using com.igg.editor;
using System;
using UnityEditor;

namespace AddressableAssetTool.Graph
{
    internal class AddressabelRuleDependencyCheck
    {
        static ResourceGraph resourceGraph;

        internal static bool CheckDependencyLegal(string dependencyRule, string guid)
        {
            if(dependencyRule.Equals(guid))
            {
                return true;
            }

            if(resourceGraph == null)
            {
                resourceGraph = new ResourceGraph();
                var guids = AssetDatabase.FindAssets("t:FeatureDependenciesScriptableObject");
                foreach(var soGuid in guids)
                {
                    var so = AssetDatabase.LoadAssetAtPath<FeatureDependenciesScriptableObject>(AssetDatabase.GUIDToAssetPath(soGuid));
                    if(so != null)
                    {
                        resourceGraph.GetOrCreateNode(soGuid);
                    }
                }

                foreach(var node in resourceGraph.GetAllNodes())
                {
                    var so = AssetDatabase.LoadAssetAtPath<FeatureDependenciesScriptableObject>(AssetDatabase.GUIDToAssetPath(node.ResourceId));
                    if (so != null)
                    {
                        foreach (var rule in so.AddrssableRules)
                        {
                            var findGuid = AddressabelUtilities.GetAssetGuid<FeatureDependenciesScriptableObject>(AssetDatabase.GetAssetPath(rule));
                            if (!string.IsNullOrEmpty(findGuid))
                            {
                                node.AddReference(resourceGraph.GetOrCreateNode(findGuid));
                            }
                        }
                        
                    }
                }
            }

            var check = resourceGraph.GetOrCreateNode(dependencyRule);

            return check.HasReference(resourceGraph.GetOrCreateNode(guid));
        }
    }
}