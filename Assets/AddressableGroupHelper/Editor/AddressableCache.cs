
using System.Collections.Generic;
using UnityEditor;

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
            deps = AssetDatabase.GetDependencies(assetPath, false);
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
            deps = AssetDatabase.GetDependencies(assetPath, false);
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