using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.IO;

namespace AddressableAssetTool
{
    internal class AddressabelUtilities
    {
        /// <summary>
        /// if true, will return built in resource dependencies.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="includeIndirect"></param>
        /// <returns></returns>
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
                dependPaths = AddressableCache.GetDependencies(path, false);
            }

            return dependPaths;
        }

        internal static bool IsAssetAddressable(Object obj)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)));
            return entry != null;
        }

        internal static bool IsAssetAddressable(string path)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));
            return entry != null;
        }

        internal static bool IsAFolder(string assetPath)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(assetPath)))
            {
                return true;
            }
            return false;
        }

        internal static string[] GetAssetRuleGuidsInFolder(string itemPath)
        {
            string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
            var rulesGUID = AssetDatabase.FindAssets(ruleFilter,
                new[] { itemPath });
            return rulesGUID;
        }
    }
}