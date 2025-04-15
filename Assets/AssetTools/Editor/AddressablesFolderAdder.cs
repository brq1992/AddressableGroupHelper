using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressablesFolderAdder
{
    [MenuItem("Tools/Addressables/Add Entire Folder as Addressable")]
    public static void AddFolderAsAddressable()
    {

        // 设置文件夹路径
        string folderPath = "Assets/Prefabs/LowPolyRPGWeapons_Lite"; // 替换为你的文件夹路径
        string groupName = "TestAddFile"; // 替换为目标Group的名称

        // 获取AddressableAssetSettings
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        // 查找或创建目标Group
        var group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, false, null);

        // 检查文件夹是否存在
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            // 检查是否已经有这个文件夹的AddressableAssetEntry
            var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(folderPath));
            if (entry == null)
            {
                // 将文件夹本身作为一个Addressable条目添加到Group中
                entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(folderPath), group);
                entry.address = Path.GetFileName(folderPath); // 设置Address
            }
            else
            {
                Debug.LogWarning("This folder is already an Addressable.");
            }

            // 保存更改
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();

            Debug.Log("Folder added to Addressable Group successfully!");
        }
        else
        {
            Debug.LogError("Invalid folder path.");
        }
    }
}
