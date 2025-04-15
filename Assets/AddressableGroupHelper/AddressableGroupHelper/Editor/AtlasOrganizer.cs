
using UnityEngine;

using UnityEditor;
using System.IO;
using AddressableAssetTool;

public class AtlasOrganizer
{
    [MenuItem("Assets/Organize Atlases")]
    private static void OrganizeAtlases()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            Debug.LogError("Invalid folder path.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { folderPath });

        if (guids.Length == 0)
        {
            Debug.Log("No Atlas files found in the selected folder.");
            return;
        }

        string atlasFolderPath = Path.Combine(folderPath, "Atlas");

        if (!AssetDatabase.IsValidFolder(atlasFolderPath))
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
                Debug.Log($"Moved {fileName} to {atlasFolderPath}");
            }
            else
            {
                Debug.LogError($"Failed to move {fileName} to {atlasFolderPath}");
            }
        }

        Object atlasFolderObject = AssetDatabase.LoadAssetAtPath<Object>(atlasFolderPath);
        Selection.activeObject = atlasFolderObject;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        //AddressableAssetRuleInspector.CreateAddressableAssetRule();
    }
}

