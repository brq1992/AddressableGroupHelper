
using AddressableAssetTool.Graph;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace AddressableAssetTool
{
    [InitializeOnLoad]
    public static class AddressableCache
    {
        private static Dictionary<string, string[]> recursiveDic = new Dictionary<string, string[]>();
        private static Dictionary<string, string[]> noRecursiveDic = new Dictionary<string, string[]>();
        private static Dictionary<string, string> _spriteDic = new Dictionary<string, string>();
        private static Dictionary<string, string[]> _atlasSpritesDic = new Dictionary<string, string[]>();
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

            var assetDependencyPaths = AssetDatabase.GetDependencies(assetPath, false);
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
            var assetDependencyPaths = AssetDatabase.GetDependencies(assetPath, true);
            return assetDependencyPaths;
        }

        internal static void CacheClear()
        {
            recursiveDic.Clear();
            noRecursiveDic.Clear();
            _spriteDic.Clear();
            _atlasSpritesDic.Clear();
        }

        internal static bool TryGetVariantDependencies(string path, out string[] variantDependencies, bool recursive = false)
        {
            if(recursive)
            {
                if (recursiveDic.TryGetValue(path, out variantDependencies))
                {
                    return true;
                }
            }
            else
            {
                if (noRecursiveDic.TryGetValue(path, out variantDependencies))
                {
                    return false;
                }
            }
            return false;
        }

        internal static string[] GetVariantDependencies(string variantPath, bool recursive = false)
        {
            string newPath = AddressabelUtilities.GetUniqueAssetPath(variantPath);
            if (recursive)
            {
                if (recursiveDic.TryGetValue(newPath, out var dps))
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
                com.igg.core.IGGDebug.LogError("Failed to LoadAssetAtPath prefab variant! " + variantPath);
                return new string[0];
            }
            GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null)
            {
                com.igg.core.IGGDebug.LogError("Failed to instantiate prefab variant! " + variantPath);
                return new string[0];
            }
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            AddressablePackTogetherGroup.RemovePrefabInstanceOverrides(instance);

            PrefabUtility.SaveAsPrefabAsset(instance, newPath);

            Object.DestroyImmediate(instance);

            var directDependencies = GetDependencies(newPath, recursive);


            //TODO: Check if it need to use variantPath to replace it.
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

        internal static bool CheckSpriteReference(string entryAssetPath, out string atlasGuid)
        {
            if(_spriteDic.TryGetValue(entryAssetPath, out var guid))
            {
                atlasGuid = guid;
                return true;
            }

            string[] atlasPaths = AssetDatabase.FindAssets("t:SpriteAtlas");
            foreach(var itemAtlas in atlasPaths)
            {
                if (_atlasSpritesDic.TryGetValue(itemAtlas, out var dependencies))
                {

                }
                else
                {
                    dependencies = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(itemAtlas));
                    _atlasSpritesDic.Add(itemAtlas, dependencies);
                }
                foreach(var spriteName in dependencies)
                {
                    if(entryAssetPath.Equals(spriteName))
                    {
                        atlasGuid = itemAtlas;
                        _spriteDic.Add(entryAssetPath, atlasGuid);
                        return true;
                    }
                }
            }
            atlasGuid = string.Empty;
            return false;
        }
    }
}