using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using AddressableAssetTool;
using System.Linq;

public class ResourceGraphBuilder
{
    private ResourceGraph resourceGraph;

    public ResourceGraphBuilder()
    {
        resourceGraph = new ResourceGraph();
    }

    public void BuildGraph()
    {
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        var resourceData = new List<(string guid, List<string> dependencyGuids)>();

        foreach (string path in assetPaths)
        {
            if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets/Test") && !AddressabelUtilities.IsAFolder(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset == null)
                {
                    Debug.LogError("load asset failed " + path);
                    continue;
                }
                var prefabType = PrefabUtility.GetPrefabAssetType(asset);

                bool recursive = false;
                List<string> dependenciesList = new List<string>();
                if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                {
                    var indirectDps = AddressableCache.GetVariantDependencies(path, recursive);
                    dependenciesList = indirectDps.ToList();
                }
                else
                {
                    var indirectDps = AddressableCache.GetDependencies(path, recursive);
                    dependenciesList = indirectDps.ToList();
                }

                var dependencyGuids = new List<string>();
                foreach (string dependency in dependenciesList)
                {
                    if (!string.IsNullOrEmpty(dependency) && !dependency.StartsWith("Packages/") && !dependency.StartsWith("Assets/Editor"))
                    {
                        string dependencyGuid = AssetDatabase.AssetPathToGUID(dependency);
                        dependencyGuids.Add(dependencyGuid);
                    }
                }
                resourceData.Add((guid, dependencyGuids));

                resourceGraph.GetOrCreateNode(guid);
            }
        }

        //Parallel.ForEach(resourceData, data =>
        //{
        //    resourceGraph.GetOrCreateNode(data.guid);
        //});

        Parallel.ForEach(resourceData, data =>
        {
            foreach (string dependencyGuid in data.dependencyGuids)
            {
                resourceGraph.AddReference(data.guid, dependencyGuid);
            }
        });
    }

    public ResourceGraph GetResourceGraph()
    {
        return resourceGraph;
    }
}
