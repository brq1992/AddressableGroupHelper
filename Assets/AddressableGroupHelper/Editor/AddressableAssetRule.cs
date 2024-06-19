
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
        public PackMode PackModel;
        public bool IsRuleUsed = true;
        internal string _gruopName;

        internal List<AddressableAssetGroup> addressableAssetGroups;
        internal int groupIndex;

        internal void ApplyDefaults()
        {
            IsRuleUsed = true;
            PackModel = PackMode.PackTogether;

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

        internal bool HasConnenct(string dependencyString, out bool isDependence)
        {
            bool connnect = false;
            isDependence = false;
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(this.name);
            if(group != null)
            {
                foreach(var item in group.entries)
                {
                    if(dependencyString.Equals(item.AssetPath))
                    {
                        connnect = true;
                        isDependence = true;
                        continue;
                    }

                    //var paths = AssetDatabase.GetDependencies(item.AssetPath, false);
                    //foreach (var path in paths)
                    //{
                    //    if (dependencyString.Equals(path))
                    //    {
                    //        connnect = true;
                    //        isDependence = false;
                    //        continue;
                    //    }
                    //}
                }
            }

            return connnect;
        }

        internal bool IsReliance(string dependencyString)
        {
            bool connnect = false;
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(this.name);
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    var paths = AddressableCache.GetDependencies(item.AssetPath, false);// AssetDatabase.GetDependencies(item.AssetPath, false);
                    foreach (var path in paths)
                    {
                        if (dependencyString.Equals(path))
                        {
                            connnect = true;
                            continue;
                        }
                    }
                }
            }

            return connnect;
        }
    }

    public static class AddressaableToolKey
    {
        internal static string ScriptObjAssetLabel = "AddressableAssetRules";
        internal static string RuleSearchPath = "Assets";
        internal static string RuleAssetExtension = ".AddressableRule";
        internal static Vector2 Size = new Vector2(250, 200);
    }
}
