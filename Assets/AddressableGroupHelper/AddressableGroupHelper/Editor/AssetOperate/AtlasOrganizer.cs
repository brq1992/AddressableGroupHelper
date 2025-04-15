
using UnityEngine;
using UnityEditor;
using System.IO;
using AddressableAssetTool;

public class AtlasOrganizer
{
    [MenuItem("Assets/Find Atlas, Move Into Folder, Create Group Rule")]
    private static void OrganizeAtlases()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            com.igg.core.IGGDebug.LogError("Invalid folder path.");
            return;
        }
        CreateFolerAndRules(folderPath);
    }

    private static void CreateFolerAndRules(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { folderPath });

        if (guids.Length == 0)
        {
            com.igg.core.IGGDebug.Log("No Atlas files found in the selected folder.");
            return;
        }

        string atlasFolderPath = Path.Combine(folderPath, "Atlas");

        if (!AssetDatabase.IsValidFolder(atlasFolderPath) || !Directory.Exists(atlasFolderPath))
        {
            AssetDatabase.CreateFolder(folderPath, "Atlas");
        }

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(assetPath);
            string newAssetPath = Path.Combine(atlasFolderPath, fileName);

            if (AssetDatabase.MoveAsset(assetPath, newAssetPath) == "")
            {
                com.igg.core.IGGDebug.Log($"Moved {fileName} to {atlasFolderPath}");
            }
            else
            {
                com.igg.core.IGGDebug.LogError($"Failed to move {assetPath} to {newAssetPath}");
            }
        }

        Object atlasFolderObject = AssetDatabase.LoadAssetAtPath<Object>(atlasFolderPath);
        Selection.activeObject = atlasFolderObject;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddressableAssetRuleInspector.CreateAddressableAssetRule();
    }

    [MenuItem("Tools/AddressableAssetManager/AssetMenuTools/Atlas/OrganizeAllAtlas")]
    private static void OrganizeAllAtlas()
    {
        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { "Assets" });

        if (guids.Length == 0)
        {
            com.igg.core.IGGDebug.Log("No Atlas files found.");
            return;
        }

        //Debug.LogError("dataPath " + Application.dataPath);

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(assetPath);
            string folerPath = Path.GetDirectoryName(assetPath);
            string ruleFilter = string.Format("t:ScriptableObject l:{0}", AddressaableToolKey.ScriptObjAssetLabel);
            string[] ruleGuids = AssetDatabase.FindAssets(ruleFilter, new[] { folerPath });
            if (ruleGuids.Length == 0)
            {
                CreateFolerAndRules(folerPath);
            }
        }
    }
}

