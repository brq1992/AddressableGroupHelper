
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AddressableAssetTool
{
    internal class AddressableCache
    {
        private static Dictionary<string, string[]> recursiveDic = new Dictionary<string, string[]>();
        private static Dictionary<string, string[]> noRecursiveDic = new Dictionary<string, string[]>();

        internal static string[] GetDependencies(string assetPath, bool recursive)
        {
            if(recursive)
            {
                return GetDependenciesIncludeRecur(assetPath);
            }
            else
            {
                return GetDependenciesNotIncludeRecur(assetPath);
            }
        }

        private static string[] GetDependenciesNotIncludeRecur(string assetPath)
        {
            string[] deps = null;
            if (noRecursiveDic.TryGetValue(assetPath, out deps))
            {
                return deps;
            }

            // When trying to get the dependencies of a prefab, it should check if it is a variant of another prefab.
            // If it is, it should also get all the dependencies of its base prefab.
            List<string> list = new List<string>();
            var assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            var prefabType = PrefabUtility.GetPrefabAssetType(assetObj);
            if (prefabType == PrefabAssetType.Variant)
            {
                var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(assetObj);
                if (basePrefab != null)
                {
                    var basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
                    var basePrefabDepenPaths = AssetDatabase.GetDependencies(basePrefabPath, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                    foreach (var basePrefabDepenPath in basePrefabDepenPaths)
                    {
                        if (!list.Contains(basePrefabDepenPath))
                        {
                            list.Add(basePrefabDepenPath);
                        }
                    }
                }
            }

            var assetDependencyPaths = AssetDatabase.GetDependencies(assetPath, false);
            for(int i = 0; i < assetDependencyPaths.Length; i++)
            {
                if (!list.Contains(assetDependencyPaths[i]))
                {
                    list.Add(assetDependencyPaths[i]);
                }
            }

            deps = list.ToArray();

            noRecursiveDic.Add(assetPath, deps);
            return deps;
        }

        private static string[] GetDependenciesIncludeRecur(string assetPath)
        {
            string[] deps = null;
            if (recursiveDic.TryGetValue(assetPath, out deps))
            {
                return deps;
            }

            // When trying to get the dependencies of a prefab, it should check if it is a variant of another prefab.
            // If it is, it should also get all the dependencies of its base prefab.
            List<string> list = new List<string>();
            var assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            var prefabType = PrefabUtility.GetPrefabAssetType(assetObj);
            if (prefabType == PrefabAssetType.Variant)
            {
                var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(assetObj);
                if (basePrefab != null)
                {
                    var basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
                    var basePrefabDepenPaths = AssetDatabase.GetDependencies(basePrefabPath, true); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                    foreach (var basePrefabDepenPath in basePrefabDepenPaths)
                    {
                        if (!list.Contains(basePrefabDepenPath))
                        {
                            list.Add(basePrefabDepenPath);
                        }
                    }
                }
            }

            var assetDependencyPaths = AssetDatabase.GetDependencies(assetPath, true);
            for (int i = 0; i < assetDependencyPaths.Length; i++)
            {
                if (!list.Contains(assetDependencyPaths[i]))
                {
                    list.Add(assetDependencyPaths[i]);
                }
            }

            deps = list.ToArray();
            recursiveDic.Add(assetPath, deps);
            return deps;
        }

        private static void CacheClear()
        {
            recursiveDic.Clear();
            noRecursiveDic.Clear();
        }
    }
}