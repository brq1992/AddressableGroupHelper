
using UnityEngine;


namespace AddressableAssetTool
{
    [System.Serializable]

    public class AddressableAssetRule : ScriptableObject
    {
        internal PackMode _packModel;
        internal bool _isRuleUsed;

        internal void ApplyDefaults()
        {
            _isRuleUsed = true;
            _packModel = PackMode.PackTogether;
        }
    }
}
