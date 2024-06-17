using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

namespace AddressableAssetTool
{
    internal class GroupShareDataWithCommonDataAnalyzer : GroupShareDataAnalyzer
    {
        private Dictionary<string, ShareEntry> _directDependenciesDic;
        private Dictionary<string, object> commonDicWithDirectDependencies;
        private Dictionary<string, ShareEntry> _inDirectDependenciesDic;

        public GroupShareDataWithCommonDataAnalyzer(AddressableAssetShareConfig t) : base(t)
        {
            _directDependenciesDic = new Dictionary<string, ShareEntry>();
            commonDicWithDirectDependencies = new Dictionary<string, object>();
            _inDirectDependenciesDic = new Dictionary<string, ShareEntry>();
        }

        internal override void AddAssetPath(string path, AddressableAssetEntry item)
        {
            if (!_directDependenciesDic.ContainsKey(path))
            {
                _directDependenciesDic.Add(path, new ShareEntry(item));
            }
            else
            {
                _directDependenciesDic[path].AddItem(item);
            }
        }

        internal override void ClearData()
        {
        }

        internal override Dictionary<string, ShareEntry> GetColloction()
        {
            AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
            

            //find common
            var commonBundleGroups = t.CommonAssetbundleGroups;
            foreach (var commonItem in commonBundleGroups)
            {
                var group = setting.FindGroup(commonItem.name);
                foreach (var item in group.entries)
                {
                    var guid = item.guid;
                    string guidToPah = AssetDatabase.GUIDToAssetPath(guid);
                    //var paths = AssetDatabase.GetDependencies(guidToPah, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                    //foreach (var path in paths)
                    //{
                    //    //UnityEngine.Debug.LogError("add " + path);
                    //    if (!commonDicWithDirectDependencies.ContainsKey(path))
                    //    {
                    //        commonDicWithDirectDependencies.Add(path, new ShareEntry(item));
                    //    }
                    //}

                    //add itself, since when the secoond para is false, AssetDatabase.GetDependencies(guidToPah, false) dont contain itself
                    if (!commonDicWithDirectDependencies.ContainsKey(guidToPah))
                    {
                        commonDicWithDirectDependencies.Add(guidToPah, new ShareEntry(item));
                    }
                }
            }


            var directDependenciesDic = new Dictionary<string, ShareEntry>();
            var rules = t.AssetbundleGroups;
            foreach (var rule in rules)
            {
                //Debug.LogError(rule1.name);
                var group = setting.FindGroup(rule.name);
                //Debug.LogError(group.name);
                foreach (var item in group.entries)
                {
                    var guid = item.guid;
                    string guidToPah = AssetDatabase.GUIDToAssetPath(guid);

                    var directPaths = AssetDatabase.GetDependencies(guidToPah, false); //AddressabelUtilities.GetDependPaths(AssetDatabase.GUIDToAssetPath(guid), _includeIndirect);
                    foreach (var path in directPaths)
                    {
                        if (commonDicWithDirectDependencies.ContainsKey(path))
                        {
                            continue;
                        }


                        //find duplicated asset in different group
                        if (!directDependenciesDic.ContainsKey(path))
                        {
                            directDependenciesDic.Add(path, new ShareEntry(item));
                        }
                        else
                        {
                            directDependenciesDic[path].AddItem(item);
                        }

                        RecurseDenpendencies(directDependenciesDic, item, path);
                    }
                }
            }



            Dictionary<string, ShareEntry> shareEngryDic = new Dictionary<string, ShareEntry>();

            foreach (var item in directDependenciesDic)
            {
                if(!commonDicWithDirectDependencies.ContainsKey(item.Key))
                {
                    if (item.Value.GetUniqueCount() > 1)
                    {
                        shareEngryDic.Add(item.Key, item.Value);
                    }
                    //shareEngryDic.Add(item.Key, item.Value);
                }
            }


            foreach (var item in _inDirectDependenciesDic)
            {
                if (item.Value.GetUniqueCount() > 1)
                {
                    shareEngryDic.Add(item.Key, item.Value);
                }
            }
            return shareEngryDic;

        }

        private void RecurseDenpendencies(Dictionary<string, ShareEntry> directDependenciesDic, AddressableAssetEntry item, string path)
        {
            var inDirectPaths = AssetDatabase.GetDependencies(path, false);
            foreach (var indirectPath in inDirectPaths)
            {
                if (directDependenciesDic.ContainsKey(indirectPath) || commonDicWithDirectDependencies.ContainsKey(indirectPath))
                {
                    continue;
                }

                if (!_inDirectDependenciesDic.ContainsKey(indirectPath))
                {
                    _inDirectDependenciesDic.Add(indirectPath, new ShareEntry(item));
                }
                else
                {
                    _inDirectDependenciesDic[indirectPath].AddItem(item);
                }

                RecurseDenpendencies(directDependenciesDic, item, indirectPath);
            }
        }
    }
}