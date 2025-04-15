
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using com.igg.editor;
using System.Linq;
using com.igg.network;
using System.Text;
using System.Reflection;
using UnityEngine.U2D;
using UnityEditor.U2D;
using com.igg.core;

namespace AddressableAssetTool.Graph
{
    public class RuleToAddressableGenerator : IIGGTool
    {
        public string ToolName => "Addressable Generator(new)";

        public IGGToolCategory Category => IGGToolCategory.Build;

        public string Description => "Rule to Addressable Generator";

        private const string scriptExtension = ".cs";
        private static List<AddressableBuildData> _buildDataList;
        private static readonly List<string> _ignoreTypeList = new List<string> { ".tpsheet", ".cginc", scriptExtension, ".dll" };
        private static IGGToolsWindowEditor _window;
        private static Dictionary<(string, string), List<string>> _filterDirGuidDic = new Dictionary<(string, string), List<string>>();
        private Vector2 _scrollPos;
        private static int _previousCount;
        private string[] _labelsContent;
        private static List<string> platformNameList = new List<string> { "Windows", "Mac", "iOS", "Android" };
        private int _count = 0;
        public void Init(IGGToolsWindowEditor window)
        {
            _window = window;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            _labelsContent = settings.GetLabels().ToArray();

            string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
            var ruleGuids = AssetDatabase.FindAssets(ruleFilter, new[] { AddressaableToolKey.RuleSearchPath });
            _buildDataList = new List<AddressableBuildData>();

            // Clear default group
            if (settings.DefaultGroup.entries.Count != 0)
            {
                for (int i = settings.DefaultGroup.entries.Count - 1; i >= 0; i--)
                {
                    var entry = settings.DefaultGroup.entries.ElementAt(i);
                    settings.RemoveAssetEntry(entry.guid);
                }
                Debug.LogError("Default group is not empty, auto clear it!");
            }

            foreach (var ruleGuid in ruleGuids)
            {
                var guidPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
                var rootRule = AssetDatabase.LoadAssetAtPath(guidPath, typeof(ScriptableObject)) as AddressableAssetRule;

                if (rootRule == null || !rootRule.IsRuleUsed)
                    continue;

                if (rootRule.AddEntryByFolder)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
                    var dic = System.IO.Path.GetDirectoryName(assetPath);
                    string filter = "t:Object";
                    List<string> diChildGuids = AddressabelUtilities.FindDirectChildren(filter, dic).ToList();
                    foreach (var diChildGuid in diChildGuids)
                    {
                        string diChildGuidPath = AssetDatabase.GUIDToAssetPath(diChildGuid);
                        var list = GetChildEntry(diChildGuid, diChildGuidPath);
                        if (list.Count < 1)
                        {
                            continue;
                        }

                        var groupData = new AddressableGroupData(BundledAssetGroupSchema.BundlePackingMode.PackTogether, rootRule);
                        var groupName = GetFolderEntryName(rootRule.name, diChildGuidPath);
                        Action<bool, Action> createAction = CreateAction(groupName, groupData, ruleGuid, rootRule, () => GetChildEntry(diChildGuid, diChildGuidPath));
                        Action<Action> clearAction = ClearAction(groupName, groupData, ruleGuid, rootRule);
                        FindRuleGroupAndUpdateStatus(settings, groupName, () => GetChildEntry(diChildGuid, diChildGuidPath), groupData);

                        _buildDataList.Add(new AddressableBuildData
                        {
                            name = groupName,
                            groupData = groupData,
                            buildAction = createAction,
                            ClearAction = clearAction
                        });
                    }
                }
                else
                {
                    var groupData = new AddressableGroupData(rootRule.PackModel, rootRule);
                    var groupName = Path.GetFileNameWithoutExtension(guidPath);


                    Func<List<string>> getChildsFunc = ContainAtlas(ruleGuid) ? () => GetAtlasGroupChilds(ruleGuid) : () => GetGroupChilds(ruleGuid);
                    Action<bool, Action> createAction = CreateAction(groupName, groupData, ruleGuid, rootRule, getChildsFunc);
                    Action<Action> clearAction = ClearAction(groupName, groupData, ruleGuid, rootRule);

                    FindRuleGroupAndUpdateStatus(settings, groupName, getChildsFunc, groupData);

                    _buildDataList.Add(new AddressableBuildData
                    {
                        name = groupName,
                        groupData = groupData,
                        buildAction = createAction,
                        ClearAction = clearAction
                    });
                }
            }
        }



        private bool ContainAtlas(string ruleGuid)
        {
            string[] atlasGuids = GetAtlasGuids(ruleGuid);
            if (atlasGuids.Length > 1)
            {
                IGGDebug.LogError(AssetDatabase.GUIDToAssetPath(ruleGuid) + " altas count more than 1 : " + atlasGuids.Length);
            }
            return atlasGuids.Length > 0;
        }

        private static string[] GetAtlasGuids(string ruleGuid)
        {
            var path = AssetDatabase.GUIDToAssetPath(ruleGuid);
            string dir = Path.GetDirectoryName(path);
            return AssetDatabase.FindAssets("t:SpriteAtlas", new string[] { dir });
        }

        private static void FindRuleGroupAndUpdateStatus(AddressableAssetSettings settings, string groupName, Func<List<string>> getChilds, AddressableGroupData groupData)
        {
            var group = settings.FindGroup(groupName);

            if (group != null)
            {
                var results = new List<AddressableAssetEntry>();
                group.GatherAllAssets(results, true, true, false);

                var validResults = results.Where(item => !item.IsFolder &&
                    (!item.AssetPath.EndsWith(".asset") || AssetDatabase.LoadAssetAtPath(item.AssetPath, typeof(ScriptableObject)) as AddressableAssetRule == null)).ToList();

                var addressableCount = validResults.Count;
                var guids = getChilds.Invoke();
                var selfCalculateCount = guids.Count;

                if (addressableCount == selfCalculateCount)
                {
                    groupData.status = GroupStatus.Finished.ToString();
                }
                else if (selfCalculateCount == 0)
                {
                    groupData.status = GroupStatus.Error.ToString();
                }
                else
                {
                    groupData.status = string.Format("{0}: {1}/{2}", GroupStatus.UnFinished.ToString(), addressableCount, selfCalculateCount);
                }
            }
            else
            {
                var guids = getChilds.Invoke();
                groupData.status = string.Format("{0}: 0/{1}", GroupStatus.NotStart.ToString(), guids.Count);
            }
        }

        private Action<bool, Action> CreateAction(string groupName, AddressableGroupData groupData, string guid, AddressableAssetRule rootRule, Func<List<string>> getChilds)
        {
            return (isUpdate, callback) => EditorCoroutineUtility.StartCoroutine(CreateGroupFolder(AddressableAssetSettingsDefaultObject.Settings,
                groupName, groupData, guid, rootRule, isUpdate, callback, getChilds), this);
        }

        private Action<Action> ClearAction(string groupName, AddressableGroupData groupData, string guid, AddressableAssetRule rootRule)
        {
            return (callback) => EditorCoroutineUtility.StartCoroutine(ClearGroup(AddressableAssetSettingsDefaultObject.Settings,
                new List<string>() { groupName }, callback), this);
        }


        private IEnumerator ClearGroup(AddressableAssetSettings settings, List<string> groupNames, Action finishCallback)
        {
            foreach (var groupName in groupNames)
            {
                var group = settings.FindGroup(groupName);
                if (group != null)
                {
                    settings.RemoveGroupNoStopEditing(group);
                }
                else
                {
                    ClearGroupAndSchema(settings, groupName);

                }
                yield return null;
            }
            finishCallback?.Invoke();
        }

        private static void ClearGroupAndSchema(AddressableAssetSettings settings, string groupName)
        {
            string groupPath = settings.GroupFolder + "/" + groupName + ".asset";
            CheckAndDeleteAssets(groupPath);
            string bundleSchemaPath = settings.GroupSchemaFolder + "/" + groupName + "_BundledAssetGroupSchema.asset";
            CheckAndDeleteAssets(bundleSchemaPath);
            string contentSchemaPath = settings.GroupSchemaFolder + "/" + groupName + "_ContentUpdateGroupSchema.asset";
            CheckAndDeleteAssets(contentSchemaPath);
        }

        private static void CheckAndDeleteAssets(string groupPath)
        {
            if (File.Exists(groupPath))
            {
                if (!AssetDatabase.DeleteAsset(groupPath))
                {
                    com.igg.core.IGGDebug.LogError("delete asset failed! " + groupPath);
                }
            }
        }

        public void Dispose()
        {
            _buildDataList.Clear();
            _buildDataList = null;
        }

        public void OnGUI()
        {
            if (_buildDataList == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();
                    for (int i = 0; i < _buildDataList.Count; i++)
                    {
                        var buildData = _buildDataList[i];
                        EditorGUILayout.BeginHorizontal();
                        {
                            bool changed = false;
                            if (GUILayout.Button($"{(i + 1)}: {buildData.name}", EditorStyles.label, GUILayout.MinWidth(180), GUILayout.MaxWidth(450)))
                            {
                                EditorGUIUtility.PingObject(buildData.groupData.Rule);
                            }
                            var t = buildData.groupData.Rule;

                            EditorGUI.BeginChangeCheck();


                            t.PackModel = (BundledAssetGroupSchema.BundlePackingMode)EditorGUILayout.EnumPopup(buildData.groupData.PackingMode, GUILayout.MinWidth(100), GUILayout.MaxWidth(150));
                            //t.BundleCompressionMode = (BundledAssetGroupSchema.BundleCompressionMode)EditorGUILayout.EnumPopup(t.BundleCompressionMode,
                            //    GUILayout.MinWidth(100), GUILayout.MaxWidth(150));

                            if (EditorGUI.EndChangeCheck())
                            {
                                changed = true;
                            }

                            if (changed)
                            {
                                var settings = AddressableAssetSettingsDefaultObject.Settings;
                                var group = settings.FindGroup(t.name);
                                if (group != null)
                                {
                                    var schema = group.GetSchema<BundledAssetGroupSchema>();
                                    schema.BundleMode = t.PackModel;
                                    foreach (var entry in group.entries)
                                    {
                                        entry.SetLabel(t.Lable, true, postEvent: false);
                                    }

                                    //settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
                                    //AssetDatabase.SaveAssets();
                                }
                                EditorUtility.SetDirty(t);
                            }

                            //EditorGUILayout.LabelField($"{(i + 1)}: {buildData.name}", GUILayout.MinWidth(180), GUILayout.MaxWidth(450));
                            EditorGUILayout.LabelField(buildData.groupData.status, GUILayout.MinWidth(110), GUILayout.MaxWidth(180));

                            if (GUILayout.Button(buildData.name, GUILayout.MinWidth(180), GUILayout.MaxWidth(450)))
                            {
                                buildData.buildAction(false, () => { EditorApplication.ExecuteMenuItem("File/Save Project"); });

                            }

                            if (GUILayout.Button("Clear", GUILayout.MinWidth(60), GUILayout.MaxWidth(100)))
                            {
                                string groupName = buildData.name;
                                var settings = AddressableAssetSettingsDefaultObject.Settings;
                                var group = settings.FindGroup(groupName);
                                if (group != null)
                                {
                                    var results = new List<AddressableAssetEntry>();
                                    group.GatherAllAssets(results, true, true, false);
                                    for (int j = 0; j < results.Count; j++)
                                    {
                                        group.RemoveAssetEntry(results[j], j == results.Count - 1);
                                        if (results[j].AssetPath.EndsWith(scriptExtension))
                                        {
                                            com.igg.core.IGGDebug.LogError("Can't remove scripts from group! ");
                                        }
                                    }
                                    buildData.groupData.status = GroupStatus.UnFinished.ToString();
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Create All"))
                    {
                        CreateAll();
                        EditorApplication.ExecuteMenuItem("File/Save Project");
                    }
                    if (GUILayout.Button("Clear All"))
                    {
                        if (!EditorUtility.DisplayDialog("Warning", "Do you want to clear all the groups? ", "Yes", "No"))
                        {
                            EditorGUILayout.EndVertical();
                            return;
                        }

                        _filterDirGuidDic.Clear();
                        AddresaableGroupTools.ClearAllGroups(null);

                        EditorApplication.ExecuteMenuItem("File/Save Project");
                    }

                    if (GUILayout.Button("CreateGruop&CheckDuplicateAssets"))
                    {
                        CreateAll(CreateGroupFinished);
                    }

                    if (GUILayout.Button("CreateGruop & CheckDuplicate & BuildAB"))
                    {
                        CreateAll(() =>
                        {
                            DuplicateCheckAndBuildAB();
                        });
                    }

                    if (GUILayout.Button("CheckDuplicate"))
                    {
                        CheckBundleDupeDependenciesMultiIsolatedGroups.RunCheckDuplicateBundleDependencies();
                    }

                    if (GUILayout.Button("CheckDuplicate & BuildAB"))
                    {
                        DuplicateCheckAndBuildAB();
                    }

                    void DuplicateCheckAndBuildAB()
                    {
                        CheckBundleDupeDependenciesMultiIsolatedGroups.RunCheckDuplicateBundleDependencies(() =>
                        {
                            string bundlePath = Application.streamingAssetsPath + "/Bundles";
                            DirectoryInfo di = new DirectoryInfo(bundlePath);
                            if (di.Exists)
                            {
                                di.Delete(true);
                            }
                            AddressableAssetSettings.CleanPlayerContent();
                            AddressableAssetSettings.BuildPlayerContent();
                            AssetDatabase.SaveAssets();
                        });
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateGroupFinished()
        {
            CheckBundleDupeDependenciesMultiIsolatedGroups.RunCheckDuplicateBundleDependencies();
        }

        public void CreateAll(Action allFinishCallback = null, bool isUpdate = false)
        {
            _filterDirGuidDic.Clear();
            if (!isUpdate)
            {
                AddresaableGroupTools.ClearAllGroups(() => {
                    StartCreateAll(allFinishCallback, isUpdate);
                });
            }
            else
            {
                StartCreateAll(allFinishCallback, isUpdate);
            }
        }

        private void StartCreateAll(Action allFinishCallback, bool isUpdate)
        {
            _count = 0;
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < _buildDataList.Count; i++)
            {
                var buildData = _buildDataList[i];
                buildData.buildAction(isUpdate, () => { OnFinish(allFinishCallback); });
            }
        }

        public void CreateHotfixGroup(Action allFinishCallback)
        {
            string rulePath = "Assets/Addressables/Misc/Misc.asset";
            var rootRule = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(rulePath);
            string ruleGUID = AssetDatabase.AssetPathToGUID(rulePath);
            var groupData = new AddressableGroupData(rootRule.PackModel, rootRule);
            var groupName = Path.GetFileNameWithoutExtension(rulePath);

            Action<bool, Action> createAction = CreateAction(groupName, groupData, ruleGUID, rootRule, () => GetGroupChilds(ruleGUID));
            createAction.Invoke(false, allFinishCallback);
        }

        void OnFinish(Action finishCallback)
        {
            ++_count;
            if (_count == _buildDataList.Count)
            {
                AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                finishCallback?.Invoke();
            }
        }

        IEnumerator CreateGroupFolder(AddressableAssetSettings setting, string folderName, AddressableGroupData groupData, string ruleGuid,
                                        AddressableAssetRule rootRule, bool isUpdate, Action finishCallback, Func<List<string>> getChilds)
        {
            var name = folderName;
            var group = setting.FindGroup(name);
            // When hotfix a build, only new group will mark as remote group. 
            if (!isUpdate || group != null)
            {
                isUpdate = false;
            }
            if (group == null)
            {
                try
                {
                    List<AddressableAssetGroupSchema> schemasToCopy = new List<AddressableAssetGroupSchema>();
                    Type[] types = new Type[]
                    {
                        typeof(BundledAssetGroupSchema),
                        typeof(ContentUpdateGroupSchema)
                    };


                    group = setting.CreateGroupWithoutSetDirty(name, false, false, true, schemasToCopy, types);
                    //group = setting.CreateGroupWithoutSetDirty(name, false, false, false, new List<AddressableAssetGroupSchema>() { new BundledAssetGroupSchema(), new ContentUpdateGroupSchema() });
                }
                catch (Exception ex)
                {
                    com.igg.core.IGGDebug.LogError("Create Error: " + ex.ToString());
                    if (!isUpdate)
                    {
                        ClearGroupAndSchema(setting, name);
                    }
                    groupData.status = GroupStatus.Error.ToString();
                    _window?.Repaint();
                    finishCallback?.Invoke();
                    yield break;
                }
            }

            //yield return null;

            SetSchema(setting, groupData.PackingMode, isUpdate, group, rootRule);

            List<string> guids = getChilds.Invoke();
            var count = guids.Count;
            for (var i = 0; i < count; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (!IsValidPath(path))
                {
                    continue;
                }

                if (IsValidFolder(path))
                {
                    continue;
                }

                if (isUpdate && setting.FindAssetEntry(guid) != null)
                {
                    continue;
                }

                AddressableAssetEntry entry = CreateOrMoveEntry(setting, group, guid, path, i == count - 1);

                SetEntryLabel(entry);

                groupData.status = $"Status: {i + 1} / {count}";

                if (i < 150)
                    yield return null;
            }
            groupData.status = GroupStatus.Finished.ToString();

            _window?.Repaint();
            finishCallback?.Invoke();
        }

        private static List<string> GetChildEntry(string guid, string guidPath)
        {
            if (IsValidFolder(guidPath))
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

        private string GetFolderEntryName(string name, string guidPath)
        {
            return new StringBuilder().Append(name).Append("_").Append(Path.GetFileNameWithoutExtension(guidPath)).ToString();
        }

        private static AddressableAssetEntry CreateOrMoveEntry(AddressableAssetSettings setting, UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group,
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

        private static void SetEntryLabel(AddressableAssetEntry entry)
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
            foreach (string platform in platformNameList)
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

        private static void SetSchema(AddressableAssetSettings setting, BundledAssetGroupSchema.BundlePackingMode bundlePackingMode, bool isUpdate, UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group, AddressableAssetRule rule)
        {
            var assetSchema = group.GetSchema<BundledAssetGroupSchema>();
            assetSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            assetSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            assetSchema.BundleMode = bundlePackingMode;
            assetSchema.UseAssetBundleCrc = false;
            assetSchema.BuildPath.SetVariableByName(setting, isUpdate ? AddressableAssetSettings.kRemoteBuildPath : AddressableAssetSettings.kLocalBuildPath);
            assetSchema.LoadPath.SetVariableByName(setting, isUpdate ? AddressableAssetSettings.kRemoteLoadPath : AddressableAssetSettings.kLocalLoadPath);
            assetSchema.Compression = rule.BundleCompressionMode == AddressableAssetRule.CompressionMode.Uncompressed ? BundledAssetGroupSchema.BundleCompressionMode.Uncompressed : (isUpdate ? BundledAssetGroupSchema.BundleCompressionMode.LZMA : BundledAssetGroupSchema.BundleCompressionMode.LZ4);
            assetSchema.UseUnityWebRequestForLocalBundles = rule.BundleCompressionMode == AddressableAssetRule.CompressionMode.Uncompressed ? true : false;
            UnityEngine.ResourceManagement.Util.SerializedType type = new UnityEngine.ResourceManagement.Util.SerializedType();
#if UNITY_ANDROID
            if (!isUpdate)
            {
                type.Value = typeof(UnityEngine.ResourceManagement.ResourceProviders.PlayAssetDeliveryProvider);
            }
            else
            {
                type.Value = typeof(UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider);
            }
#else
            type.Value = typeof(UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider);
#endif
            FieldInfo field = assetSchema.GetType().GetField("m_AssetBundleProviderType", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(assetSchema, type);
            var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
            updateSchema.StaticContent = !isUpdate;

            if (AddressabelUtilities.NeedIgnorePlatform(group.name))
            {
                assetSchema.IncludeInBuild = false;
            }
        }

        private static List<string> GetGroupChilds(string ruleGuid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
            string guidDir = System.IO.Path.GetDirectoryName(assetPath);
            guidDir = guidDir.Replace("\\", "/");
            string filter = "t:Object";
            var guids = FindRootAssets(filter, guidDir);
            return guids;
        }

        private static List<string> GetAtlasGroupChilds(string ruleGuid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(ruleGuid);
            string guidDir = System.IO.Path.GetDirectoryName(assetPath);
            guidDir = guidDir.Replace("\\", "/");
            string filter = "t:Object";
            var guids = FindRootAssets(filter, guidDir);

            List<string> atlasObjects = GetAssetsInAtlas(ruleGuid);
            guids.AddRange(atlasObjects);
            return guids;
        }

        private static List<string> GetAssetsInAtlas(string ruleGuid)
        {
            List<string> atlasObjects = new List<string>();
            string[] atlasGuids = GetAtlasGuids(ruleGuid);
            for (int i = 0; i < atlasGuids.Length; i++)
            {
                string atlasPath = AssetDatabase.GUIDToAssetPath(atlasGuids[i]);
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlas != null)
                {
                    UnityEngine.Object[] packedObjects = atlas.GetPackables();
                    foreach (var obj in packedObjects)
                    {
                        if (obj is Sprite || obj is Texture2D)
                        {
                            string guid = GetGuidFromObj(obj);
                            atlasObjects.Add(guid);
                        }
                        else if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                        {
                            string path = AssetDatabase.GetAssetPath(obj);
                            string[] assetGuids = AssetDatabase.FindAssets("t:Sprite", new[] { path });
                            atlasObjects.AddRange(assetGuids.ToList());
                        }
                        else
                        {
                            IGGDebug.LogError($"ContainAtlas get wrong tyep: {obj.name} ({obj.GetType()})");
                        }
                    }
                }
            }

            return atlasObjects;
        }

        private static string GetGuidFromObj(UnityEngine.Object obj)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(objPath);
            return guid;
        }

        //dir must be the root directory of the rule. 
        static List<string> FindRootAssets(string filter, string rootDir)
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
                if (childDir == rootDir && !IsAFolder(assetPath))
                {
                    //IGGDebug.Log("Found asset: " + assetPath);
                    if (assetPath.EndsWith(".asset"))
                    {
                        var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                        if (rootRule && rootRule.IsRuleUsed)
                        {
                            continue;
                        }
                        else//if(rootRule == false)
                        {
                            if (!assetPath.EndsWith(scriptExtension) && rootRule == false)
                            {
                                assetPaths.Add(assetPath);
                                guids.Add(guid);
                            }
                        }
                    }
                    else
                    {
                        if (!assetPath.EndsWith(scriptExtension))
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
                if (childDir == dir && !IsAFolder(assetPath))
                {
                    //IGGDebug.Log("Found asset: " + assetPath);
                    if (assetPath.EndsWith(".asset"))
                    {
                        var rootRule = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as AddressableAssetRule;
                        if (rootRule && rootRule.IsRuleUsed)
                        {
                            return new List<string>();
                        }
                        else// if (rootRule == false)
                        {
                            if (!assetPath.EndsWith(scriptExtension) && rootRule == false)
                            {
                                assetPaths.Add(assetPath);
                                guids.Add(guid);
                            }
                        }
                    }
                    else
                    {
                        if (!assetPath.EndsWith(scriptExtension))
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
                    if (slashCount == 1 && IsAFolder(assetPath))
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
            if (!_filterDirGuidDic.ContainsKey((filter, dir)))
            {
                _filterDirGuidDic.Add((filter, dir), guids);
            }
            return guids;
        }

        bool IsValidPath(string pPath)
        {
            //if (AssetDatabase.IsValidFolder(pPath))
            //{
            //    return false;
            //}
            foreach (string str in _ignoreTypeList)
            {
                if (pPath.EndsWith(str))
                {
                    return false;
                }
            }
            return true;
        }

        static bool IsValidFolder(string pPath)
        {
            if (AssetDatabase.IsValidFolder(pPath))
            {
                return true;
            }
            return false;
        }

        private static bool IsAFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return true;
            }
            return false;
        }

        [MenuItem("Tools/AddressableAssetManager/Rule to Group/TotalEntryCount")]
        public static void CaculateEntryCount()
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            int count = 0;
            foreach (var group in setting.groups)
            {
                count += group.entries.Count;
            }
            com.igg.core.IGGDebug.LogError("Previou " + _previousCount + " Current " + count);
            _previousCount = count;
        }
    }

    public class AddressableBuildData
    {
        public string name;
        public AddressableGroupData groupData;
        public Action<bool, Action> buildAction;
        public Action<Action> ClearAction;
    }

    public class AddressableGroupData
    {
        public string status = "Status: Not Start";
        internal int _currentIndex = -1;
        public BundledAssetGroupSchema.BundlePackingMode PackingMode;
        public AddressableAssetRule Rule { get; private set; }

        public AddressableGroupData(BundledAssetGroupSchema.BundlePackingMode packingMode, AddressableAssetRule rule)
        {
            PackingMode = packingMode;
            Rule = rule;
        }
    }

    enum GroupStatus
    {
        NotStart,
        UnFinished,
        Finished,
        Error
    }
}