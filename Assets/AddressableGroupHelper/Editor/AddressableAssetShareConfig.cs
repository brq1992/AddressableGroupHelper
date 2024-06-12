
using System.Collections.Generic;
using UnityEngine;

namespace AddressableAssetTool
{
    [System.Serializable]
    public class AddressableAssetShareConfig : ScriptableObject
    {
        public List<AddressableAssetRule> AddressableAssetRules = new List<AddressableAssetRule>();
    }
}