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

        internal static string GetUniqueAssetPath(string path, bool createNewIfExits = true)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            string filename = System.IO.Path.GetFileNameWithoutExtension(path);
            string extension = System.IO.Path.GetExtension(path);

            string newPath = $"{directory}/{filename}{AddressaableToolKey.PrefabVariantName}{extension}";
            //directory = directory.Replace("Assets\\Addressables\\", "");
            //Debug.LogError(directory);
            //string tempDic = $"{Application.dataPath}\\AddressableTempPrefab\\{directory}";
            //Debug.LogError(tempDic);
            //if (!Directory.Exists(tempDic))
            //{
            //    Directory.CreateDirectory(tempDic);
            //}
            //string newPath = $"{tempDic}/{filename}{AddressaableToolKey.PrefabVariantName}{extension}";
            int counter = 1;

            while (System.IO.File.Exists(newPath) && createNewIfExits)
            {
                Debug.LogError("This variant has a new prefab! " + newPath);
                newPath = $"{directory}/{filename}{AddressaableToolKey.PrefabVariantName}{counter}{extension}";
                counter++;
            }

            return newPath;
        }

        internal static void GetEntryDependencies(List<string> dependenciesList, string[] directDependencies, bool recursive)
        {

            //UnityEngine.Debug.LogError("GetEntryDependencies start： " + DateTime.Now.ToString());
            foreach (var path in directDependencies)
            {
                if (dependenciesList.Contains(path))
                {
                    continue;
                }

                dependenciesList.Add(path);

                if (AddressabelUtilities.IsAssetAddressable(path))
                {
                    continue;
                }

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset == null)
                {
                    Debug.LogError("load asset failed " + path);
                    continue;
                }
                //UnityEngine.Debug.LogError("GetEntryDependencies path： " + path);
                var prefabType = PrefabUtility.GetPrefabAssetType(asset);

                if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                {
                    var indirectDps = AddressableCache.GetVariantDependencies(path, recursive);
                    GetEntryDependencies(dependenciesList, indirectDps, recursive);
                }
                else
                {
                    var indirectDps = AddressableCache.GetDependencies(path, recursive);
                    GetEntryDependencies(dependenciesList, indirectDps, recursive);
                }
            }
        }
    }
}