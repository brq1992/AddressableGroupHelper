
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Graphs;

namespace AddressableAssetTool
{
    internal class ShareEntry
    {
        private List<AddressableAssetEntry> entries;
        private Dictionary<string, int> groupCount;
        private int uniqueCount = 0;

        public ShareEntry(AddressableAssetEntry item)
        {
            entries = new List<AddressableAssetEntry>() { item };
            groupCount = new Dictionary<string, int>();
            AddItem(item);
        }

        internal void AddItem(AddressableAssetEntry item)
        {
            if(!entries.Contains(item))
            {
                entries.Add(item);
            }

            string groupName = item.parentGroup.name;
            if (!groupCount.ContainsKey(groupName))
            {
                groupCount.Add(groupName, 0);
                uniqueCount++;
            }
        }

        internal List<AddressableAssetEntry> Entries()
        {
            return entries;
        }

        internal int GetTotalCount()
        {
            return entries.Count;
        }

        internal int GetUniqueCount()
        {
            //com.igg.core.IGGDebug.LogError("uniqueCount " + uniqueCount);
            return uniqueCount;
        }
    }
}