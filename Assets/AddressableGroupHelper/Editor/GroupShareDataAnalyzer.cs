using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;

namespace AddressableAssetTool
{
    internal class GroupShareDataAnalyzer
    {
        private Dictionary<string, ShareEntry> keyValuePairs;

        public GroupShareDataAnalyzer()
        {
            keyValuePairs = new Dictionary<string, ShareEntry>();
        }

        internal void AddAssetPath(string path, AddressableAssetEntry item)
        {
            
            if(!keyValuePairs.ContainsKey(path))
            {
                keyValuePairs.Add(path, new ShareEntry(item));
            }
            else
            {
                keyValuePairs[path].AddItem(item);
            }
        }

        internal Dictionary<string, ShareEntry> GetColloction()
        {
            Dictionary<string, ShareEntry> shareEngryDic = new Dictionary<string, ShareEntry>();
            foreach(var item in keyValuePairs)
            {
                if(item.Value.GetUniqueCount() > 1)
                {
                    shareEngryDic.Add(item.Key, item.Value);
                }
            }
            return shareEngryDic;
        }
    }
}