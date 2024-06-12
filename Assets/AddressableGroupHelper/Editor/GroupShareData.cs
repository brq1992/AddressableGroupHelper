using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;

namespace AddressableAssetTool
{
    internal class GroupShareData
    {
        private Dictionary<string, string> _pathDic;
        private AddressableAssetGroup group;

        public GroupShareData(AddressableAssetGroup group)
        {
            this.group = group;
        }

        internal void AddAssetPath(string path)
        {
            _pathDic = new Dictionary<string, string>();
            if(!_pathDic.ContainsKey(path))
            {
                _pathDic.Add(path, path);
            }
        }
    }
}