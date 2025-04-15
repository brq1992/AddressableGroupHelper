
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using com.igg.network;
using System.Text;
using AddressableAssetTool;

namespace com.igg.editor
{
    public class AddresaableGroupBuildUtilities
    {
        public static void SetSchema(AddressableAssetSettings setting, AddressableAssetRule rootRule, bool isUpdate, UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group)
        {
            var assetSchema = group.GetSchema<BundledAssetGroupSchema>();
            assetSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            assetSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            if (rootRule != null)
            {
                assetSchema.BundleMode = rootRule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackTogether ? BundledAssetGroupSchema.BundlePackingMode.PackTogether :
                    BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            }
            assetSchema.UseAssetBundleCrc = false;
            assetSchema.BuildPath.SetVariableByName(setting, isUpdate ? AddressableAssetSettings.kRemoteBuildPath : AddressableAssetSettings.kLocalBuildPath);
            assetSchema.LoadPath.SetVariableByName(setting, isUpdate ? AddressableAssetSettings.kRemoteLoadPath : AddressableAssetSettings.kLocalLoadPath);
            assetSchema.Compression = isUpdate ? BundledAssetGroupSchema.BundleCompressionMode.LZMA : BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
            updateSchema.StaticContent = !isUpdate;

            if (AddressabelUtilities.NeedIgnorePlatform(group.name))
            {
                assetSchema.IncludeInBuild = false;
            }
        }

        public static void SetSchema(AddressableAssetSettings setting, BundledAssetGroupSchema.BundlePackingMode bundlePackingMode, bool isUpdate, UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group)
        {
            var assetSchema = group.GetSchema<BundledAssetGroupSchema>();
            assetSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            assetSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            assetSchema.BundleMode = bundlePackingMode;
            assetSchema.UseAssetBundleCrc = false;
            assetSchema.BuildPath.SetVariableByName(setting, isUpdate ? AddressableAssetSettings.kRemoteBuildPath : AddressableAssetSettings.kLocalBuildPath);
            assetSchema.LoadPath.SetVariableByName(setting, isUpdate ? AddressableAssetSettings.kRemoteLoadPath : AddressableAssetSettings.kLocalLoadPath);
            assetSchema.Compression = isUpdate ? BundledAssetGroupSchema.BundleCompressionMode.LZMA : BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
            updateSchema.StaticContent = !isUpdate;

            if (AddressabelUtilities.NeedIgnorePlatform(group.name))
            {
                assetSchema.IncludeInBuild = false;
            }
        }

        public static AddressableAssetEntry CreateOrMoveEntry(AddressableAssetSettings setting, UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group,
    string guid, string path, bool postEvent = false)
        {
            var entry = setting.CreateOrMoveEntry(guid, group, false, postEvent);
            var dir = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
            var address = Path.Combine(dir, file).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var finalAddress = address.Remove(0, "Assets/Addressables/".Length);
            entry.SetAddress(finalAddress, postEvent);
            return entry;
        }


        public static List<string> _platformNameList = new List<string> { "Windows", "Mac", "iOS", "Android" };
        public static void SetEntryLabel(AddressableAssetEntry entry)
        {
            bool hasSpecialLabel = false;
            foreach (SystemLanguage language in Enum.GetValues(typeof(SystemLanguage)))
            {
                if (entry.address.EndsWith($"/{language.ToString()}"))
                {
                    entry.SetLabel(language.ToString(), true, postEvent: false);
                    hasSpecialLabel = true;
                    break;
                }
            }
            foreach (string platform in _platformNameList)
            {
                if (entry.address.IndexOf($"/{platform}/") >= 0)
                {
                    entry.SetLabel(platform, true, postEvent: false);
                    hasSpecialLabel = true;
                    break;
                }
            }
            if (hasSpecialLabel)
            {
                entry.SetLabel(AddressableDownloadManager.LABEL_SPECIAL, true, postEvent: false);
            }
            else
            {
                entry.SetLabel(AddressableDownloadManager.LABEL_DEFAULT, true, postEvent: false);
            }
            entry.SetLabel(AddressableDownloadManager.LABEL_IN_BUILD, true, postEvent: false);
        }

        public static List<string> GetChildEntry(string guid, string guidPath)
        {
            if (Utilities.IsValidFolder(guidPath))
            {
                string filter = "t:Object";
                return FindChildDirAssets(filter, guidPath);
            }
            if (guidPath.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath(guidPath, typeof(ScriptableObject)) as AddressableAssetRule != null)
            {
                return new List<string>();
            }
            return new List<string>() { guid };
        }

        public static List<string> FindRootAssets(string filter, string rootDir)
        {
            //IGGDebug.Log("Root dir: " + rootDir);
            List<string> guids = new List<string>();
            string[] assetGuids = AssetDatabase.FindAssets(filter, new[] { rootDir });
            List<string> assetPaths = new List<string>();

            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string childDir = System.IO.Path.GetDirectoryName(assetPath);
                childDir = childDir.Replace("\\", "/");
                if (childDir == rootDir && !Utilities.IsValidFolder(assetPath))
                {
                    //IGGDebug.Log("Found asset: " + assetPath);
                    if (assetPath.EndsWith(".asset"))
                    {
                        var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                        if (rootRule && rootRule.IsRuleUsed)
                        {
                            continue;
                        }
                        else if(rootRule == false)
                        {
                            assetPaths.Add(assetPath);
                            guids.Add(guid);
                        }
                    }
                    else
                    {
                        if (!assetPath.EndsWith(".cs"))
                        {
                            assetPaths.Add(assetPath);
                            guids.Add(guid);
                        }
                    }
                }
                else
                {
                    string relativePath = assetPath.Substring(rootDir.Length);
                    int slashCount = relativePath.Split('/').Length - 1;
                    if (slashCount == 1)
                    {
                        List<string> childGuids = FindChildDirAssets(filter, assetPath);
                        if (childGuids != null)
                        {
                            assetPaths.AddRange(childGuids);
                            guids.AddRange(childGuids);
                        }
                    }

                }
            }
            return guids;
        }

        private static List<string> FindChildDirAssets(string filter, string dir)
        {
            List<string> guids = new List<string>();
            List<string> assetPaths = new List<string>();
            string[] assetGuids = AssetDatabase.FindAssets(filter, new[] { dir });
            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string childDir = System.IO.Path.GetDirectoryName(assetPath);
                childDir = childDir.Replace("\\", "/");
                if (childDir == dir && !Utilities.IsValidFolder(assetPath))
                {
                    //IGGDebug.Log("Found asset: " + assetPath);
                    if (assetPath.EndsWith(".asset"))
                    {
                        var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                        if (rootRule && rootRule.IsRuleUsed)
                        {
                            return new List<string>();
                        }
                        else if (rootRule == false)
                        {
                            assetPaths.Add(assetPath);
                            guids.Add(guid);
                        }
                    }
                    else
                    {
                        if (!assetPath.EndsWith(".cs"))
                        {
                            assetPaths.Add(assetPath);
                            guids.Add(guid);
                        }
                    }
                }
                else
                {
                    string relativePath = assetPath.Substring(dir.Length);
                    int slashCount = relativePath.Split('/').Length - 1;
                    if (slashCount == 1 && Utilities.IsValidFolder(assetPath))
                    {
                        //IGGDebug.Log("Found child dir: " + assetPath);
                        List<string> childGuids = FindChildDirAssets(filter, assetPath);
                        if (childGuids != null)
                        {
                            assetPaths.AddRange(childGuids);
                            guids.AddRange(childGuids);
                        }
                    }
                }
            }
            return guids;
        }

        public static string GetFolderEntryName(string name, string guidPath)
        {
            return new StringBuilder().Append(name).Append("_").Append(Path.GetFileNameWithoutExtension(guidPath)).ToString();
        }

        internal static List<string> GetGroupChilds(string ruleGuid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
            string guidDir = System.IO.Path.GetDirectoryName(assetPath);
            guidDir = guidDir.Replace("\\", "/");
            string filter = "t:Object";
            var guids = FindRootAssets(filter, guidDir);
            return guids;
        }
    }
}
