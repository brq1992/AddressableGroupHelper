using AddressableAssetTool;
using AddressableAssetTool.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace com.igg.editor
{
    public class AddresaableGroupTools
    {
        [MenuItem("Tools/AddressableAssetManager/Tools/ClearGroups")]
        public static void ClearAllGroups()
        {
            ClearAllGroups(null);
        }

        public static void ClearAllGroups(Action action)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup> groups = new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup>(setting.groups);
            string builtInName = "Built In Data";
            int builtInDataIndex = RemoveGoupFromList(groups, builtInName);

            RemoveGoupFromList(groups, "Default Local Group");

            AssetDatabase.StartAssetEditing();

            while (groups.Count > 0)
            {
                setting.RemoveGroupNoStopEditing(groups[0]);
                groups.RemoveAt(0);
            }
            setting.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            action?.Invoke();
        }

        private static int RemoveGoupFromList(List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup> groups, string builtInName)
        {
            var builtInDataIndex = groups.FindIndex(x => x.name.Contains(builtInName));
            if (builtInDataIndex != -1)
            {
                groups.RemoveAt(builtInDataIndex);
            }

            return builtInDataIndex;
        }


        [MenuItem("Tools/AddressableAssetManager/Tools/MakeRulesToGroups")]
        public static void RuleToAddressableGroups()
        {
            string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
            var rulesGUIDs = AssetDatabase.FindAssets(ruleFilter, new[] { AddressaableToolKey.RuleSearchPath });

            AssetDatabase.StartAssetEditing();
            finishedCount = 0;
           
            var actions = new List<Action<bool, Action>>();
            foreach (var ruleGuid in rulesGUIDs)
            {
                var guidPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
                var rootRule = AssetDatabase.LoadAssetAtPath(guidPath, typeof(ScriptableObject)) as AddressableAssetRule;

                if (rootRule == null || !rootRule.IsRuleUsed)
                    continue;

                if (rootRule.AddEntryByFolder)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
                    var dic = Path.GetDirectoryName(assetPath);
                    string filter = "t:Object";
                    List<string> diChildGuids = AddressabelUtilities.FindDirectChildren(filter, dic).ToList();
                    
                   
                    foreach (var diChildGuid in diChildGuids)
                    {
                        string diChildGuidPath = AssetDatabase.GUIDToAssetPath(diChildGuid);
                        var list = AddresaableGroupBuildUtilities.GetChildEntry(diChildGuid, diChildGuidPath);
                        if (list.Count < 1)
                        {
                            OnFinish(null);
                            continue;
                        }

                        var groupData = new AddressableGroupData(BundledAssetGroupSchema.BundlePackingMode.PackTogether, rootRule);
                        var groupName = AddresaableGroupBuildUtilities.GetFolderEntryName(rootRule.name, diChildGuidPath);
                        Action<bool, Action> ac = CreateAction(AddressableAssetSettingsDefaultObject.Settings,
                groupName, BundledAssetGroupSchema.BundlePackingMode.PackTogether, false, () => { OnFinish(null); }, () => AddresaableGroupBuildUtilities.GetChildEntry(diChildGuid, diChildGuidPath));
                        
                        actions.Add(ac);
                    }
                }
                else
                {
                    var groupData = new AddressableGroupData(rootRule.PackModel, rootRule);
                    var groupName = Path.GetFileNameWithoutExtension(guidPath);

                    Action<bool, Action> ac = CreateAction(AddressableAssetSettingsDefaultObject.Settings, groupName, 
                        BundledAssetGroupSchema.BundlePackingMode.PackSeparately, false, () => { OnFinish(null); }, () => AddresaableGroupBuildUtilities.GetGroupChilds(ruleGuid));
                    actions.Add(ac);
                }
            }
            needLoadCount = actions.Count;
            foreach(var ac in actions)
            {
                ac.Invoke(false, null);
            }
        }

        private static Action<bool, Action> CreateAction(AddressableAssetSettings setting, string folderName, BundledAssetGroupSchema.BundlePackingMode bundlePackingMode,
             bool isUpdate, Action finishCallback, Func<List<string>> getChilds)
        {
            return (isUpdate, callback) => EditorCoroutineUtility.StartCoroutineOwnerless(CreateGroupFolder(AddressableAssetSettingsDefaultObject.Settings,
                folderName, BundledAssetGroupSchema.BundlePackingMode.PackTogether, false, () => { OnFinish(null); }, getChilds));
        }

        static int finishedCount = 0;
        static int needLoadCount = 0;
        static  void OnFinish(Action finishCallback)
        {
            ++finishedCount;
            if (finishedCount == needLoadCount)
            {
                AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                finishCallback?.Invoke();
            }
        }

        static IEnumerator CreateGroupFolder(AddressableAssetSettings setting, string folderName, BundledAssetGroupSchema.BundlePackingMode bundlePackingMode,
             bool isUpdate, Action finishCallback, Func<List<string>> getChilds)
        {
            var name = folderName;
            var group = setting.FindGroup(name);
            if (group == null)
            {
                group = setting.CreateGroupWithoutSetDirty(name, false, false, false, new List<AddressableAssetGroupSchema>() { new BundledAssetGroupSchema(),
                    new ContentUpdateGroupSchema() });
            }

            AddresaableGroupBuildUtilities.SetSchema(setting, bundlePackingMode, isUpdate, group);

            List<string> guids = getChilds.Invoke();
            var count = guids.Count;
            for (var i = 0; i < count; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (!Utilities.IsValidPath(path))
                {
                    continue;
                }

                if (Utilities.IsValidFolder(path))
                {
                    continue;
                }

                AddressableAssetEntry entry = AddresaableGroupBuildUtilities.CreateOrMoveEntry(setting, group, guid, path, i == count - 1);

                AddresaableGroupBuildUtilities.SetEntryLabel(entry);

                if (i < 150)
                    yield return null;
            }

            finishCallback?.Invoke();
        }

    }
}
