
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;


namespace AddressableAssetTool
{
    [System.Serializable]

    public class AddressableAssetRule : ScriptableObject
    {
        internal PackMode _packModel;
        internal bool _isRuleUsed;
        internal string _gruopName;

        internal List<AddressableAssetGroup> addressableAssetGroups;
        internal int groupIndex;

        internal void ApplyDefaults()
        {
            _isRuleUsed = true;
            _packModel = PackMode.PackTogether;

            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (addressableAssetProfileSettings == null)
            {
                EditorUtility.DisplayDialog("Warning", "Can't find AddressableAssetSettings, " +
                    "please make sure Addressable has import into project.", "ok");
                return;
            }

            addressableAssetGroups = new List<AddressableAssetGroup>(addressableAssetProfileSettings.groups);

        }

        public void UpdateData()
        {
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (addressableAssetProfileSettings == null)
            {
                EditorUtility.DisplayDialog("Warning", "Can't find AddressableAssetSettings, " +
                    "please make sure Addressable has import into project.", "ok");
                return;
            }

            addressableAssetGroups = new List<AddressableAssetGroup>(addressableAssetProfileSettings.groups);
        }

        internal AddressableAssetGroup GetCurrentGroup()
        {
            return AddressableAssetSettingsDefaultObject.Settings.groups[groupIndex];
        }
    }

    public static class AddreaableToolKey
    {
        internal static string ScriptObjAssetLabel = "AddressableAssetRules";
        internal static string RuleSearchPath = "Assets";
    }
}
