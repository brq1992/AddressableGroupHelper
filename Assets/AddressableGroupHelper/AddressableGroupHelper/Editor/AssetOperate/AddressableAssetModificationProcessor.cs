
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;


namespace AddressableAssetTool
{
    public class AddressableAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        private static string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            //IGGDebug.LogError("Source path: " + sourcePath + ". Destination path: " + destinationPath + ".");

            OnMoveAsset(sourcePath, destinationPath);

            AddressableCache.CacheClear();
            return AssetMoveResult.DidNotMove;
        }

        private static void OnMoveAsset(string sourcePath, string destinationPath)
        {
            //only Move Asset that isn't from ScriptableObject
            if (sourcePath.EndsWith(".asset"))
            {
                return;
            }


            string sourceDir = Path.GetDirectoryName(sourcePath);
            sourceDir = sourceDir.Replace("\\", "/");
            AddressableAssetRule sourceRule = GetRule(sourceDir);

            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            destinationDirectory = destinationDirectory.Replace("\\", "/");
            //IGGDebug.LogError("Destination path: " + destinationDirectory);
            string assetGuid = AssetDatabase.AssetPathToGUID(sourcePath);
            AddressableAssetRule destinationRule = GetRule(destinationDirectory);

            //only Move Asset that has a AssetRule to mananged with it
            if (destinationRule != null && sourceRule != null)
            {
                MoveAssetToGroup(destinationRule, assetGuid, destinationPath);
            }
        }

        private static void MoveAssetToGroup(AddressableAssetRule applyRule, string destinationGuid, string destinationPath)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            var group = setting.FindGroup(applyRule.name);
            if (group != null)
            {
                var entry = setting.CreateOrMoveEntry(destinationGuid, group);
                var dir = Path.GetDirectoryName(destinationPath);
                var file = Path.GetFileName(destinationPath);
                entry.address = Path.Combine(dir, file).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        private static AddressableAssetRule GetRule(string destinationDirectory)
        {
            var ruleGuids = AssetDatabase.FindAssets(ruleFilter,
                new[] { destinationDirectory });
            foreach (var guid in ruleGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var rule = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(assetPath);
                if (rule != null && rule.IsRuleUsed)
                {
                    //IGGDebug.LogError("get " + assetPath);
                    return rule;
                }
            }
            int index = destinationDirectory.LastIndexOf('/');
            if (index != -1)
            {
                string parentDir = destinationDirectory.Substring(0, index);
                //IGGDebug.LogError("parentDir " + parentDir);
                return GetRule(parentDir);
            }
            return null;

        }
    }
}