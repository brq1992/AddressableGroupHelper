using AddressableAssetTool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class AddresaableTempraryTester : MonoBehaviour
{
    static string assetFilter = "t:Object";
    private static List<string> _ignoreTypeList = new List<string> { ".tpsheet", ".cginc", ".cs", ".dll", ".asset" };

    [MenuItem("Assets/AddressableAssetManager/Test/Classify Asset to Group")]
    public static void MoveAssetToProperGroup()
    {
        string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddreaableToolKey.ScriptObjAssetLabel);
        var ruleGUIDs = AssetDatabase.FindAssets(ruleFilter, 
            new[] { AddreaableToolKey.RuleSearchPath });

        string assetFilter = "t:Object";
        foreach(var item in ruleGUIDs)
        {
            DS(item);

        }

    }

    private static void DS(string ruleGUID)
    {
        var guidPath = AssetDatabase.GUIDToAssetPath(ruleGUID);
        //Debug.LogError("file path " + guidPath);

        var dic = Path.GetDirectoryName(guidPath);
        //Debug.LogError("dic " + dic);

        var rootRule = (AddressableAssetRule)AssetDatabase.LoadAssetAtPath(guidPath, typeof(ScriptableObject));
        if(rootRule._packModel == PackMode.PackTogether)
        {
            var assetGUIDs = AssetDatabase.FindAssets(assetFilter, new string[] { dic });

        }
        else
        {
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



                AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
                AddressableAssetGroup group = rootRule.GetCurrentGroup();

                var entry = setting.CreateOrMoveEntry(guid, group);
            }
        }


        
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


        foreach (var group in groups)
        {
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
            group.GatherAllAssets(entries, true, true, true);
            
            for(int i = 0; i< entries.Count; i++)
            {
                var entry = entries[i];
                group.RemoveAssetEntry(entry, false);
                Debug.LogError("remove " + entry.address);
            }

            AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, group, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

       
    }
}
