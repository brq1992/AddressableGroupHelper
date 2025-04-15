
using System.Collections.Generic;
using UnityEngine;

namespace AddressableAssetTool
{
    [System.Serializable]
    public class AddressableAssetShareConfig : ScriptableObject
    {
        public List<AddressableAssetRule> AssetbundleGroups = new List<AddressableAssetRule>();

        public List<AddressableAssetRule> CommonAssetbundleGroups = new List<AddressableAssetRule>();

        [HideInInspector]
        public bool ShowIndirectReferencies = true;
    }
}