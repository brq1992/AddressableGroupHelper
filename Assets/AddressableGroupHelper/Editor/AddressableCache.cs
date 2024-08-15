
using AddressableAssetTool.Graph;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
            //List<string> list = new List<string>();
            //var assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            //var prefabType = PrefabUtility.GetPrefabAssetType(assetObj);
            //if (prefabType == PrefabAssetType.Variant)
            //{
            //    var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(assetObj);
            //    if (basePrefab != null)
            //    {
            //        var basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
            //        // Temporarily disable this code since the base prefab will refer to internal assets in the same bundle
            //        // that will cause variant prefab get the wroing dependencies
            //        var basePrefabDepenPaths = AssetDatabase.GetDependencies(basePrefabPath, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
            //        foreach (var basePrefabDepenPath in basePrefabDepenPaths)
            //        {
            //            if (!list.Contains(basePrefabDepenPath))
            //            {
            //                list.Add(basePrefabDepenPath);
            //            }
            //        }

            //        //Variant prefab need to remove its base prefab, since assetbundle will not contain it.
            //        var assetVariantDependencyPaths = AssetDatabase.GetDependencies(assetPath, false);
            //        for (int i = 0; i < assetVariantDependencyPaths.Length; i++)
            //        {
            //            if (assetVariantDependencyPaths[i].Equals(basePrefabPath))
            //            {
            //                continue;
            //            }

            //            if (!list.Contains(assetVariantDependencyPaths[i]))
            //            {
            //                list.Add(assetVariantDependencyPaths[i]);
            //            }
            //        }

            //        deps = list.ToArray();
            //        noRecursiveDic.Add(assetPath, deps);
            //        return deps;
            //    }
            //}

            var assetDependencyPaths = AssetDatabase.GetDependencies(assetPath, false);
            //for (int i = 0; i < assetDependencyPaths.Length; i++)
            //{
            //    if (!list.Contains(assetDependencyPaths[i]))
            //    {
            //        list.Add(assetDependencyPaths[i]);
            //    }
            //}

            //deps = list.ToArray();

            noRecursiveDic.Add(assetPath, assetDependencyPaths);
            return assetDependencyPaths;
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
            //var assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            //var prefabType = PrefabUtility.GetPrefabAssetType(assetObj);
            //if (prefabType == PrefabAssetType.Variant)
            //{
            //    var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(assetObj);
            //    if (basePrefab != null)
            //    {
            //        var basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
            //        var basePrefabDepenPaths = AssetDatabase.GetDependencies(basePrefabPath, true); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
            //        foreach (var basePrefabDepenPath in basePrefabDepenPaths)
            //        {
            //            if (!list.Contains(basePrefabDepenPath))
            //            {
            //                list.Add(basePrefabDepenPath);
            //            }
            //        }
            //    }
            //}

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

        internal static void CacheClear()
        {
            recursiveDic.Clear();
            noRecursiveDic.Clear();
        }

        internal static string[] GetVariantDependencies(string variantPath, bool recursive = false)
        {
            string newPath = AddressablePackTogetherGroup.GetUniqueAssetPath(variantPath);
            if (recursive)
            {
                if(recursiveDic.TryGetValue(newPath, out var dps))
                {
                    return dps;
                }
            }
            else
            {
                if (noRecursiveDic.TryGetValue(newPath, out var dps))
                {
                    return dps;
                }
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(variantPath);
            if (asset == null)
            {
                Debug.LogError("Failed to LoadAssetAtPath prefab variant! " + variantPath);
                return new string[0];
            }
            GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate prefab variant! "+ variantPath);
                return new string[0];
            }
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            AddressablePackTogetherGroup.RemovePrefabInstanceOverrides(instance);

            PrefabUtility.SaveAsPrefabAsset(instance, newPath);

            Object.DestroyImmediate(instance);

            var directDependencies = GetDependencies(newPath, recursive);


            if (recursive)
            {
                recursiveDic[newPath] = directDependencies;
            }
            else
            {
                noRecursiveDic[newPath] = directDependencies;
            }

            AssetDatabase.DeleteAsset(newPath);

            return directDependencies;
        }
    }
}