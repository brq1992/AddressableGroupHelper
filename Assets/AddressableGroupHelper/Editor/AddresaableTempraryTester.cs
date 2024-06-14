using AddressableAssetTool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class AddresaableTempraryTester : MonoBehaviour
{
    static string assetFilter = "t:Object";
    private static List<string> _ignoreTypeList = new List<string> { ".tpsheet", ".cginc", ".cs", ".dll", ".asset" };


    [MenuItem("Tools/AddressableAssetManager/Classify Asset to Group")]
    [MenuItem("Assets/AddressableAssetManager/Test/Classify Asset to Group")]
    public static void MoveAssetToProperGroup()
    {
        string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddreaableToolKey.ScriptObjAssetLabel);
        var rulesGUID = AssetDatabase.FindAssets(ruleFilter, 
            new[] { AddreaableToolKey.RuleSearchPath });

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
            Debug.LogError(" item " + item.Path);
            DS(item.GUID);
        }

    }

    private static void DS(string ruleGUID)
    {
        var guidPath = AssetDatabase.GUIDToAssetPath(ruleGUID);
        //Debug.LogError("file path " + guidPath);

        var dic = Path.GetDirectoryName(guidPath);
        //Debug.LogError("dic " + dic);

        var rootRule = (AddressableAssetRule)AssetDatabase.LoadAssetAtPath(guidPath, typeof(ScriptableObject));
        var name = Path.GetFileNameWithoutExtension(guidPath);
        AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
        var group = setting.FindGroup(name);
        if (group == null)
        {
            group = setting.CreateGroup(name, false, false, false, new List<AddressableAssetGroupSchema>() { new BundledAssetGroupSchema(), new ContentUpdateGroupSchema() });
        }
        var assetSchema = group.GetSchema<BundledAssetGroupSchema>();
        assetSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
        assetSchema.BundleMode = rootRule._packModel == PackMode.PackSeparately ? BundledAssetGroupSchema.BundlePackingMode.PackSeparately : 
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

    }
}
