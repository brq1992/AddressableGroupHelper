

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace AddressableAssetTool
{
    internal class GroupShareDataNormalAnalyzer : GroupShareDataAnalyzer
    {
        private Dictionary<string, ShareEntry> keyValuePairs;

        public GroupShareDataNormalAnalyzer(AddressableAssetShareConfig t) : base (t)
        {
            keyValuePairs = new Dictionary<string, ShareEntry>();
        }


        internal override void AddAssetPath(string path, AddressableAssetEntry item)
        {
            if (!keyValuePairs.ContainsKey(path))
            {
                keyValuePairs.Add(path, new ShareEntry(item));
            }
            else
            {
                keyValuePairs[path].AddItem(item);
            }
        }

        internal override Dictionary<string, ShareEntry> GetColloction()
        {
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            var rules = t.AssetbundleGroups;
            foreach (var rule in rules)
            {
                //Debug.LogError(rule1.name);
                var group = setting.FindGroup(rule.name);
                //Debug.LogError(group.name);
                foreach (var item in group.entries)
                {
                    GetPrefabBaseParentDependencies(item);

                    var guid = item.guid;
                    string guidToPah = AssetDatabase.GUIDToAssetPath(guid);
                    var paths = AddressableCache.GetDependencies(guidToPah, _includeIndirect); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                    foreach (var path in paths)
                    {
                        //UnityEngine.Debug.LogError("add " + path);
                        AddAssetPath(path, item);
                    }
                }
            }


            Dictionary<string, ShareEntry> shareEngryDic = new Dictionary<string, ShareEntry>();
            foreach (var item in keyValuePairs)
            {
                if (item.Value.GetUniqueCount() > 1)
                {
                    shareEngryDic.Add(item.Key, item.Value);
                }
            }
            return shareEngryDic;
        }

        private void GetPrefabBaseParentDependencies(AddressableAssetEntry item)
        {
            var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
            if (prefabType == PrefabAssetType.Variant)
            {
                var basePrefab = PrefabUtility.GetCorrespondingObjectFromSource(item.MainAsset);
                if (basePrefab != null)
                {
                    var basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
                    var basePrefabDepenPaths = AddressableCache.GetDependencies(basePrefabPath, _includeIndirect); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                    foreach (var path in basePrefabDepenPaths)
                    {
                        AddAssetPath(path, item);
                    }

                }
            }
        }

        internal override void ClearData()
        {
            keyValuePairs.Clear();
        }
    }
}