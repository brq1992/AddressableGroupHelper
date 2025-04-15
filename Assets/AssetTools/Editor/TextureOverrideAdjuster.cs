using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.U2D;

public class TextureOverrideAdjuster : MonoBehaviour
{
    [MenuItem("AssetTools/Adjust Texture & Atlas Overrides")]
    private static void AdjustTextureAndAtlasOverrides()
    {
        // 获取所有Texture2D资源的GUID
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");

        // 获取所有Sprite Atlas资源的GUID
        string[] atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas");

        // 调整Texture2D资源
        AdjustTextureOverrides(textureGuids);

        // 调整Sprite Atlas资源
        AdjustAtlasOverrides(atlasGuids);

        AssetDatabase.Refresh();
        Debug.Log("Texture and Atlas overrides adjustment completed.");
    }

    private static void AdjustTextureOverrides(string[] guids)
    {
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter != null)
            {
                // 获取默认平台的maxSize
                int defaultMaxSize = textureImporter.maxTextureSize;

                // 获取Android和iOS平台的设置
                TextureImporterPlatformSettings androidSettings = textureImporter.GetPlatformTextureSettings("Android");
                TextureImporterPlatformSettings iosSettings = textureImporter.GetPlatformTextureSettings("iPhone");

                bool androidOverridden = androidSettings.overridden;
                bool iosOverridden = iosSettings.overridden;

                int minSize = defaultMaxSize;

                if (androidOverridden && iosOverridden)
                {
                    // 同时启用了Android和iOS的override
                    minSize = Mathf.Min(defaultMaxSize, androidSettings.maxTextureSize, iosSettings.maxTextureSize);
                    SetMaxSizeForPlatform(textureImporter, "Android", minSize);
                    SetMaxSizeForPlatform(textureImporter, "iPhone", minSize);
                }
                else if (androidOverridden)
                {
                    // 只启用了Android的override
                    minSize = Mathf.Min(defaultMaxSize, androidSettings.maxTextureSize);
                    SetMaxSizeForPlatform(textureImporter, "Android", minSize);
                }
                else if (iosOverridden)
                {
                    // 只启用了iOS的override
                    minSize = Mathf.Min(defaultMaxSize, iosSettings.maxTextureSize);
                    SetMaxSizeForPlatform(textureImporter, "iPhone", minSize);
                }

                // 设置Default平台的maxSize为最小值
                if (minSize < defaultMaxSize)
                {
                    textureImporter.maxTextureSize = minSize;
                    Debug.Log($"Set Default max size to {minSize} for {textureImporter.assetPath}");
                }

                // 重新导入修改过的资源
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    private static void AdjustAtlasOverrides(string[] guids)
    {
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

            if (atlas != null)
            {
                // 获取默认平台的maxSize
                int defaultMaxSize = atlas.GetPlatformSettings("DefaultTexturePlatform").maxTextureSize;

                // 获取Android和iOS平台的设置
                var androidSettings = atlas.GetPlatformSettings("Android");
                var iosSettings = atlas.GetPlatformSettings("iPhone");

                bool androidOverridden = androidSettings.overridden;
                bool iosOverridden = iosSettings.overridden;

                int minSize = defaultMaxSize;

                if (androidOverridden && iosOverridden)
                {
                    // 同时启用了Android和iOS的override
                    minSize = Mathf.Min(defaultMaxSize, androidSettings.maxTextureSize, iosSettings.maxTextureSize);
                    SetMaxSizeForAtlas(atlas, "Android", minSize);
                    SetMaxSizeForAtlas(atlas, "iPhone", minSize);
                }
                else if (androidOverridden)
                {
                    // 只启用了Android的override
                    minSize = Mathf.Min(defaultMaxSize, androidSettings.maxTextureSize);
                    SetMaxSizeForAtlas(atlas, "Android", minSize);
                }
                else if (iosOverridden)
                {
                    // 只启用了iOS的override
                    minSize = Mathf.Min(defaultMaxSize, iosSettings.maxTextureSize);
                    SetMaxSizeForAtlas(atlas, "iPhone", minSize);
                }

                // 设置Default平台的maxSize为最小值
                if (minSize < defaultMaxSize)
                {
                    var defaultSettings = atlas.GetPlatformSettings("DefaultTexturePlatform");
                    defaultSettings.maxTextureSize = minSize;
                    atlas.SetPlatformSettings(defaultSettings);
                    Debug.Log($"Set Default max size to {minSize} for Sprite Atlas at {path}");
                }

                EditorUtility.SetDirty(atlas); // 标记Atlas为脏，以便保存更改
            }
        }
    }

    private static void SetMaxSizeForPlatform(TextureImporter textureImporter, string platform, int maxSize)
    {
        TextureImporterPlatformSettings platformSettings = textureImporter.GetPlatformTextureSettings(platform);

        if (platformSettings.overridden)
        {
            platformSettings.maxTextureSize = maxSize;
            textureImporter.SetPlatformTextureSettings(platformSettings);
            Debug.Log($"Set {platform} max size to {maxSize} for {textureImporter.assetPath}");
        }
    }

    private static void SetMaxSizeForAtlas(SpriteAtlas atlas, string platform, int maxSize)
    {
        var platformSettings = atlas.GetPlatformSettings(platform);

        if (platformSettings.overridden)
        {
            platformSettings.maxTextureSize = maxSize;
            atlas.SetPlatformSettings(platformSettings);
            Debug.Log($"Set {platform} max size to {maxSize} for Sprite Atlas");
        }
    }
}
