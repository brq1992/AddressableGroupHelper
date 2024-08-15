using AddressableAssetTool;
using AddressableAssetTool.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using AddressableAssetGroup = UnityEditor.AddressableAssets.Settings.AddressableAssetGroup;

public class AddresaableTempraryTester : MonoBehaviour
{
    static string assetFilter = "t:Object";
    private static List<string> _ignoreTypeList = new List<string> { ".tpsheet", ".cginc", ".cs", ".dll"};


    [MenuItem("Tools/AddressableAssetManager/Classify Asset to Group")]
    [MenuItem("Assets/AddressableAssetManager/Test/Classify Asset to Group")]
    public static void MoveAssetToProperGroup()
    {
        string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
        var rulesGUID = AssetDatabase.FindAssets(ruleFilter, 
            new[] { AddressaableToolKey.RuleSearchPath });

        List<RulePathData> ruleGUIDAfterSort = new List<RulePathData>(); 

        foreach(var item in rulesGUID)
        {
            ruleGUIDAfterSort.Add(new RulePathData(item, AssetDatabase.GUIDToAssetPath(item)));
        }


        ruleGUIDAfterSort.Sort((x, y) =>
        {
            return x.Path.Length.CompareTo(y.Path.Length);
        });

        foreach (var item in ruleGUIDAfterSort)
        {
            //Debug.LogError(" item " + item.Path);
            CreatOrMoveEntryIntoGroup(item.GUID);
        }

    }

    private static void CreatOrMoveEntryIntoGroup(string ruleGUID)
    {
        var guidPath = AssetDatabase.GUIDToAssetPath(ruleGUID);
        //Debug.LogError("file path " + guidPath);

        var dic = Path.GetDirectoryName(guidPath);
        //Debug.LogError("dic " + dic);

        var rootRule = (AddressableAssetRule)AssetDatabase.LoadAssetAtPath(guidPath, typeof(ScriptableObject));

        if(!rootRule.IsRuleUsed)
        {
            return;
        }

        var name = Path.GetFileNameWithoutExtension(guidPath);
        AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
        var group = setting.FindGroup(name);
        if (group == null)
        {
            group = setting.CreateGroup(name, false, false, false, new List<AddressableAssetGroupSchema>() { new BundledAssetGroupSchema(), 
                new ContentUpdateGroupSchema() });
        }
        var assetSchema = group.GetSchema<BundledAssetGroupSchema>();
        assetSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
        assetSchema.BundleMode = rootRule.PackModel == PackMode.PackSeparately ? BundledAssetGroupSchema.BundlePackingMode.PackSeparately : 
            BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        assetSchema.UseAssetBundleCrc = false;
        var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
        updateSchema.StaticContent = true;

        var assetGUIDs = AssetDatabase.FindAssets(assetFilter, new string[] { dic });
        foreach (var guid in assetGUIDs)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            //Debug.LogError("assetPath " + assetPath);
            if (!IsValidAsset(assetPath))
            {
                continue;
            }

            if (IsAFolder(assetPath))
            {
                //DoSthWhenFolder(assetPath);
                continue;
            }

            var addressableAssetRule = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(assetPath);
            if(addressableAssetRule != null)
            {
                continue;
            }

            //if(!IsAssetSameDicWithRule(Path.GetDirectoryName(assetPath), dic))
            //{
            //    //var rootRule = (AddressableAssetRule)AssetDatabase.LoadAssetAtPath(guidPath, typeof(ScriptableObject));
            //    continue;
            //}

            //Debug.LogError("add " + assetPath);
            var entry = setting.CreateOrMoveEntry(guid, group);
            var dir = Path.GetDirectoryName(assetPath);
            var file = Path.GetFileName(assetPath);
            entry.address = Path.Combine(dir, file).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }

    private static bool IsInRootDirectory(string assetPath, string rootDirectory)
    {
        string assetDirectory = Path.GetDirectoryName(assetPath);

        return string.Equals(assetDirectory, rootDirectory, System.StringComparison.OrdinalIgnoreCase);
    }

    private static void DoSthWhenFolder(string assetPath)
    {
        //var ruleGUIDs = AssetDatabase.FindAssets(AddreaableToolKey.ScriptObjAssetLabel, new string[] { assetPath });
        //foreach(var childRuleGUID in ruleGUIDs)
        //{
        //    var childRulePath = AssetDatabase.GUIDToAssetPath(childRuleGUID);
        //    Debug.LogError("childRulePath " + childRulePath);
        //}

        Debug.LogError("IsAFolder Pause" + assetPath);
    }

    private static bool IsAssetSameDicWithRule(string assetPath, string dic)
    {
        if(assetPath.Equals(dic, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    private static bool IsAFolder(string assetPath)
    {
        if(string.IsNullOrEmpty(Path.GetExtension(assetPath)))
        {
            return true;
        }
        return false;
    }

    static bool IsValidAsset(string pPath)
    {
        //if (AssetDatabase.IsValidFolder(pPath))
        //{
        //    return false;
        //}
        foreach (string str in _ignoreTypeList)
        {
            if (pPath.Contains(str))
            {
                return false;
            }
        }
        return true;
    }

    [MenuItem("Tools/AddressableAssetManager/Clear Group")]
    [MenuItem("Assets/AddressableAssetManager/Test/Clear Group")]
    public static void ClearGroup()
    {
        var setting = AddressableAssetSettingsDefaultObject.Settings;
        List<AddressableAssetGroup> groups = new List<AddressableAssetGroup>(setting.groups);
        var builtInDataIndex = groups.FindIndex(x => x.name.Contains("Built"));
        if(builtInDataIndex != -1)
        {
            groups.RemoveAt(builtInDataIndex);
        }

        while(groups.Count > 0)
        {
            setting.RemoveGroup(groups[0]);
            groups.RemoveAt(0);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddressableCache.CacheClear();
        BaseNodeCreator.Clear();

    }



    [MenuItem("Assets/Organize File")]
    private static void OrganizeAtlases()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            Debug.LogError("Invalid folder path.");
            return;
        }

        string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
        var rulesGUID = AssetDatabase.FindAssets(ruleFilter,
            new[] { folderPath });

        foreach(var guid in rulesGUID)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string dir = System.IO.Path.GetDirectoryName(assetPath);
            dir = dir.Replace("\\", "/");
            string filter = string.Format("t:Object");
            FindRootAssets(filter, dir);
        }
    }

    //dir must be the root directory of the rule. 
    static List<string> FindRootAssets(string filter, string rootDir)
    {
        //Debug.Log("Root dir: " + rootDir);
        List<string> guids = new List<string>();
        string[] assetGuids = AssetDatabase.FindAssets(filter, new[] { rootDir });
        List<string> assetPaths = new List<string>();

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string childDir = System.IO.Path.GetDirectoryName(assetPath);
            childDir = childDir.Replace("\\", "/");
            if (childDir == rootDir && !IsAFolder(assetPath))
            {
                //Debug.Log("Found asset: " + assetPath);
                if (assetPath.EndsWith(".asset"))
                {
                    var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                    if (rootRule && rootRule.IsRuleUsed)
                    {
                        continue;
                    }
                    else
                    {
                        assetPaths.Add(assetPath);
                        guids.Add(guid);
                    }
                }
                else
                {
                    assetPaths.Add(assetPath);
                    guids.Add(guid);
                }
            }
            else
            {
                string relativePath = assetPath.Substring(rootDir.Length);
                int slashCount = relativePath.Split('/').Length - 1;
                if (slashCount == 1)
                {
                    //Debug.Log("Found dir: " + assetPath);
                    List<string> childGuids = FindChildDirAssets(filter, assetPath);
                    if (childGuids != null)
                        guids.AddRange(childGuids);
                }

            }
        }
        //Debug.LogError("Root dir: " + rootDir);
        //foreach (var asset in guids)
        //{
        //    string assetPath = AssetDatabase.GUIDToAssetPath(asset);
        //    Debug.LogError("Valid Item: " + assetPath);
        //}
        return guids;
    }

    private static Dictionary<(string, string), List<string>> _filterDirGuidDic = new Dictionary<(string, string), List<string>>();

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
            if (childDir == dir && !IsAFolder(assetPath) )
            {
                //Debug.Log("Found asset: " + assetPath);
                if (assetPath.EndsWith(".asset"))
                {
                    var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                    if (rootRule && rootRule.IsRuleUsed)
                    {
                        return new List<string>();
                    }
                    else
                    {
                        assetPaths.Add(assetPath);
                        guids.Add(guid);
                    }
                }
                else
                {
                    assetPaths.Add(assetPath);
                    guids.Add(guid);
                }
            }
            else
            {
                string relativePath = assetPath.Substring(dir.Length);
                int slashCount = relativePath.Split('/').Length - 1;
                if (slashCount == 1 && IsAFolder(assetPath))
                {
                    //Debug.Log("Found child dir: " + assetPath);
                    List<string> childGuids = FindChildDirAssets(filter, assetPath);
                    if (childGuids != null)
                        guids.AddRange(childGuids);
                }
            }
        }
        if (!_filterDirGuidDic.ContainsKey((filter, dir)))
        {
            _filterDirGuidDic.Add((filter, dir), guids);
        }
        return guids;
    }
}
