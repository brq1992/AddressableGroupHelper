using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace AddressableAssetTool
{
    internal class AddressabelUtilities
    {
        public static string[] GetDependPaths(string path, bool includeIndirect)
        {
            string[] dependPaths;
            if (includeIndirect)
            {
                var dependPathList = new List<string>();
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                foreach (Object obj in EditorUtility.CollectDependencies(new Object[] { asset }))
                {
                    if (obj != null)
                    {
                        string p = AssetDatabase.GetAssetPath(obj);
                        if (p != path && !dependPathList.Contains(p))
                        {
                            dependPathList.Add(p);
                        }
                    }
                }
                dependPaths = dependPathList.ToArray();
            }
            else
            {
                dependPaths = AssetDatabase.GetDependencies(path, false);
            }

            return dependPaths;
        }
    }
}