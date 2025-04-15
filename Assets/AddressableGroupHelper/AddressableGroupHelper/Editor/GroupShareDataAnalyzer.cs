using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;

namespace AddressableAssetTool
{
    internal abstract class GroupShareDataAnalyzer
    {
        protected bool _includeIndirect = false;

        protected AddressableAssetShareConfig t;

        public GroupShareDataAnalyzer(AddressableAssetShareConfig t)
        {
            this.t = t;
            _includeIndirect = t.ShowIndirectReferencies;
        }

        public bool ShowIndirectReferencies { get { return _includeIndirect; }
            set { _includeIndirect = value; } }

        internal abstract void AddAssetPath(string path, AddressableAssetEntry item);

        internal abstract void ClearData();

        internal abstract Dictionary<string, ShareEntry> GetColloction();
    }
}