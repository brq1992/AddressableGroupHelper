

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using static UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema;


namespace AddressableAssetTool
{
    [System.Serializable]

    public class AddressableAssetRule : ScriptableObject
    {
        public BundledAssetGroupSchema.BundlePackingMode PackModel;
        public bool IsRuleUsed = true;
        public BundledAssetGroupSchema.BundleCompressionMode BundleCompressionMode = BundledAssetGroupSchema.BundleCompressionMode.Uncompressed;
        public string Lable = string.Empty;
        internal string _gruopName;

        internal List<AddressableAssetGroup> addressableAssetGroups;
        internal int groupIndex;

        internal void ApplyDefaults()
        {
            IsRuleUsed = true;
            PackModel = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            BundleCompressionMode = BundleCompressionMode.Uncompressed;
            Lable = string.Empty;
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

        internal bool HasConnenct(string dependencyString, out bool isDependence, out string dependencePath)
        {
            bool connnect = false;
            isDependence = false;
            dependencePath = null;
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(this.name);
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    if (dependencyString.Equals(item.AssetPath))
                    {
                        connnect = true;
                        isDependence = true;
                        dependencePath = item.AssetPath;
                        continue;
                    }
                }
            }

            return connnect;
        }

        internal bool IsReliance(string assetPath, out string[] dependentPaths)
        {
            List<string> list = new List<string>();
            bool connnect = false;
            dependentPaths = null;
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableAssetProfileSettings.FindGroup(this.name);
            if (group != null)
            {
                foreach (var item in group.entries)
                {
                    // Temporarily disable this code since the base prefab will refer to internal assets in the same bundle
                    // that will cause variant prefab get the wroing dependencies
                    var prefabType = PrefabUtility.GetPrefabAssetType(item.MainAsset);
                    if (prefabType == PrefabAssetType.Variant || prefabType == PrefabAssetType.Regular)
                    {
                        List<string> dependenciesList = new List<string>();
                        var directDependencies = AddressableCache.GetVariantDependencies(item.AssetPath, false);
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        var dependenciesAfterFilter = dependenciesList.ToArray();

                        foreach (var path in dependenciesAfterFilter)
                        {
                            if (assetPath.Equals(path))
                            {
                                connnect = true;
                                list.Add(item.AssetPath);
                            }
                        }
                    }
                    else
                    {
                        List<string> dependenciesList = new List<string>();
                        var directDependencies = AddressableCache.GetDependencies(item.AssetPath, false);
                        AddressabelUtilities.GetEntryDependencies(dependenciesList, directDependencies, false);
                        var dependenciesAfterFilter = dependenciesList.ToArray();
                        foreach (var path in dependenciesAfterFilter)
                        {
                            if (assetPath.Equals(path))
                            {
                                connnect = true;
                                list.Add(item.AssetPath);
                            }
                        }
                    }
                }
                dependentPaths = list.ToArray();
            }
            return connnect;
        }
    }

    public static class AddressaableToolKey
    {
        internal static string ScriptObjAssetLabel = "AddressableAssetRules";
        internal static string FeatureDependenciesLabel = "FeatureDependenciesScriptableObject";
        internal static string RuleSearchPath = "Assets";
        internal static string RuleAssetExtension = ".AddressableRule";
        internal static Vector2 Size = new Vector2(250, 200);
        internal static float NodeRadius = 150;
        internal static float GroupRadius = 440f;
        internal static string PrefabVariantName = "_New";
    }
}
