using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.IO;
using com.igg.editor;
using com.igg.core;
using UnityEditor.Build.Pipeline.Interfaces;

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

        internal static string[] GetObjectInFolder<T>(string dic) where T: Object
        {
            string ruleFilter = string.Format("t:{0}", typeof(T).Name);
            var rulesGUID = AssetDatabase.FindAssets(ruleFilter, new[] { dic });
            return rulesGUID;
        }

        internal static string GetUniqueAssetPath(string path, bool createNewIfExits = true)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            string filename = System.IO.Path.GetFileNameWithoutExtension(path);
            string extension = System.IO.Path.GetExtension(path);

            string newPath = $"{directory}/{filename}{AddressaableToolKey.PrefabVariantName}{extension}";
            //directory = directory.Replace("Assets\\Addressables\\", "");
            //IGGDebug.LogError(directory);
            //string tempDic = $"{Application.dataPath}\\AddressableTempPrefab\\{directory}";
            //IGGDebug.LogError(tempDic);
            //if (!Directory.Exists(tempDic))
            //{
            //    Directory.CreateDirectory(tempDic);
            //}
            //string newPath = $"{tempDic}/{filename}{AddressaableToolKey.PrefabVariantName}{extension}";
            int counter = 1;

            while (System.IO.File.Exists(newPath) && createNewIfExits)
            {
                com.igg.core.IGGDebug.LogError("This variant has a new prefab! " + newPath);
                newPath = $"{directory}/{filename}{AddressaableToolKey.PrefabVariantName}{counter}{extension}";
                counter++;
            }

            return newPath;
        }

        internal static void GetEntryDependencies(List<string> dependenciesList, string[] directDependencies, bool recursive)
        {
            //com.igg.core.IGGDebug.LogError("GetEntryDependencies start： " + DateTime.Now.ToString());
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
                    com.igg.core.IGGDebug.LogError("GetEntryDependencies failed when load asset at path: " + path);
                    continue;
                }
                //com.igg.core.IGGDebug.LogError("GetEntryDependencies path： " + path);
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

        internal static void GetAllDependencies(List<string> dependenciesList, string[] directDependencies, bool recursive)
        {
            //com.igg.core.IGGDebug.LogError("GetEntryDependencies start： " + DateTime.Now.ToString());
            foreach (var path in directDependencies)
            {
                if (dependenciesList.Contains(path))
                {
                    continue;
                }

                dependenciesList.Add(path);

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset == null)
                {
                    com.igg.core.IGGDebug.LogError("GetEntryDependencies failed when load asset at path: " + path);
                    continue;
                }
                //com.igg.core.IGGDebug.LogError("GetEntryDependencies path： " + path);
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

        private static List<string> GetGroupChilds(string ruleGuid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
            string guidDir = System.IO.Path.GetDirectoryName(assetPath);
            guidDir = guidDir.Replace("\\", "/");
            string filter = "t:Object";
            var guids = FindRootAssets(filter, guidDir);
            return guids;
        }

        //dir must be the root directory of the rule. 
        static List<string> FindRootAssets(string filter, string rootDir)
        {
            //IGGDebug.Log("Root dir: " + rootDir);
            List<string> guids = new List<string>();
            string[] assetGuids = AssetDatabase.FindAssets(filter, new[] { rootDir });
            List<string> assetPaths = new List<string>();

            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string childDir = System.IO.Path.GetDirectoryName(assetPath);
                childDir = childDir.Replace("\\", "/");
                if (childDir == rootDir && !IsAFolder(assetPath))
                {
                    //IGGDebug.Log("Found asset: " + assetPath);
                    if (assetPath.EndsWith(".asset"))
                    {
                        var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                        if (rootRule && rootRule.IsRuleUsed)
                        {
                            continue;
                        }
                        else
                        {
                            if (!assetPath.EndsWith(".cs"))
                            {
                                assetPaths.Add(assetPath);
                                guids.Add(guid);
                            }
                        }
                    }
                    else
                    {
                        if (!assetPath.EndsWith(".cs"))
                        {
                            assetPaths.Add(assetPath);
                            guids.Add(guid);
                        }
                    }
                }
                else
                {
                    string relativePath = assetPath.Substring(rootDir.Length);
                    int slashCount = relativePath.Split('/').Length - 1;
                    if (slashCount == 1)
                    {
                        //IGGDebug.Log("Found dir: " + assetPath);
                        List<string> childGuids = FindChildDirAssets(filter, assetPath);
                        if (childGuids != null)
                            guids.AddRange(childGuids);
                    }

                }
            }
            //IGGDebug.LogError("Root dir: " + rootDir);
            //foreach (var asset in guids)
            //{
            //    string assetPath = AssetDatabase.GUIDToAssetPath(asset);
            //    com.igg.core.IGGDebug.LogError("Valid Item: " + assetPath);
            //}
            return guids;
        }

        private static List<string> FindChildDirAssets(string filter, string dir)
        {
            List<string> guids = new List<string>();
            List<string> assetPaths = new List<string>();
            string[] assetGuids = AssetDatabase.FindAssets(filter, new[] { dir });
            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string childDir = System.IO.Path.GetDirectoryName(assetPath);
                childDir = childDir.Replace("\\", "/");
                if (childDir == dir && !IsAFolder(assetPath))
                {
                    //IGGDebug.Log("Found asset: " + assetPath);
                    if (assetPath.EndsWith(".asset"))
                    {
                        var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                        if (rootRule && rootRule.IsRuleUsed)
                        {
                            return new List<string>();
                        }
                        else
                        {
                            if (!assetPath.EndsWith(".cs"))
                            {
                                assetPaths.Add(assetPath);
                                guids.Add(guid);
                            }
                        }
                    }
                    else
                    {
                        if (!assetPath.EndsWith(".cs"))
                        {
                            assetPaths.Add(assetPath);
                            guids.Add(guid);
                        }
                    }
                }
                else
                {
                    string relativePath = assetPath.Substring(dir.Length);
                    int slashCount = relativePath.Split('/').Length - 1;
                    if (slashCount == 1 && IsAFolder(assetPath))
                    {
                        //IGGDebug.Log("Found child dir: " + assetPath);
                        List<string> childGuids = FindChildDirAssets(filter, assetPath);
                        if (childGuids != null)
                            guids.AddRange(childGuids);
                    }
                }
            }
            //if (!_filterDirGuidDic.ContainsKey((filter, dir)))
            //{
            //    _filterDirGuidDic.Add((filter, dir), guids);
            //}
            return guids;
        }

        public static string[] FindDirectChildren(string searchFilter, string parentFolder, bool containRules = false)
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter, new[] { parentFolder });
            List<string> directChildGuids = new List<string>();
            string replace = parentFolder.Replace("\\", "/");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                //int lastIndex = path.LastIndexOf("/");
                //string folder = path.Substring(0, lastIndex);
                //IGGDebug.LogError(path);
                string relativePath = path.Replace(replace, "").Trim('/');
                //IGGDebug.LogError(relativePath);
                if (!relativePath.Contains("/"))
                {
                    //directChildGuids.Add(guid);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        string[] subFolderGuids = AssetDatabase.FindAssets(searchFilter, new[] { path });
                        bool hasAssetFile = false;
                        if (!containRules)
                        {
                            string subPathReplace = path.Replace("\\", "/");
                            foreach (string subGuid in subFolderGuids)
                            {
                                string subPath = AssetDatabase.GUIDToAssetPath(subGuid);
                                string subRelativePath = subPath.Replace(subPathReplace, "").Trim('/');
                                if (!subRelativePath.Contains("/"))
                                {
                                    if (subPath.EndsWith(".asset"))
                                    {
                                        if (AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(path) != null ||
                                            AssetDatabase.LoadAssetAtPath<FeatureDependenciesScriptableObject>(path) != null)
                                        {
                                            IGGDebug.LogError("There is a group rule in child dic! parentFolder: " + parentFolder + " " + subPath);
                                            hasAssetFile = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!hasAssetFile)
                        {
                            directChildGuids.Add(guid);
                        }
                    }
                    else
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(path);
                        if (obj == null)
                        {
                            directChildGuids.Add(guid);
                        }
                    }
                }
            }
            return directChildGuids.ToArray();
        }

        private static Dictionary<string, string> ruleGuidDic = new Dictionary<string, string>();

        public static string GetAssetGuid<T>(string selectPath) where T : Object
        {
            string directory = System.IO.Path.GetDirectoryName(selectPath);

            if (ruleGuidDic.TryGetValue(directory, out var guid))
            {
                return guid;
            }

            T abcObject = default;

            while (!string.IsNullOrEmpty(directory) && directory != "Assets")
            {
                string[] files = Directory.GetFiles(directory, "*.asset", SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    string filePath = file.Replace("/", "\\");
                    T obj = AssetDatabase.LoadAssetAtPath<T>(filePath);
                    if (obj != null)
                    {
                        abcObject = obj;
                        Selection.activeObject = abcObject; // 选中找到的对象
                        EditorGUIUtility.PingObject(abcObject); // 在项目视图中高亮显示
                        guid = AssetDatabase.AssetPathToGUID(file);
                        ruleGuidDic[directory] = guid;
                        return guid;
                    }
                }

                directory = Directory.GetParent(directory)?.FullName.Replace("\\", "/");
                if (directory != null && directory.StartsWith(Application.dataPath))
                {
                    directory = "Assets" + directory.Substring(Application.dataPath.Length);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        private static List<string> _ignorePlatforms;

        public static bool NeedIgnorePlatform(string path)
        {
            if(_ignorePlatforms == null)
            {
                _ignorePlatforms = new List<string>();

#if !UNITY_ANDROID
                _ignorePlatforms.Add("Android");
#endif

#if !UNITY_IOS
                _ignorePlatforms.Add("iOS");
#endif

#if !UNITY_STANDALONE_WIN
                _ignorePlatforms.Add("Windows");
#endif
            }

            foreach (var ignorePlatform in _ignorePlatforms)
            {
                if (path.Contains(ignorePlatform))
                {
                    return true;
                }
            }

            return false;
        }

        internal static void GetEntryDependencies(List<string> dependenciesList, AssetResultData value)
        {
            foreach (var path in value.ReferencedObjects)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(path.guid);
                if (dependenciesList.Contains(assetPath))
                {
                    continue;
                }

                dependenciesList.Add(assetPath);

                if (IsAssetAddressable(assetPath))
                {
                    continue;
                }
            }
        }
    }
}