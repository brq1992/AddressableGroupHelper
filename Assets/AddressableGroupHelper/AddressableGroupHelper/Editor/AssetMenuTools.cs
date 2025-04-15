using AddressableAssetTool;
using AddressableAssetTool.Graph;
using com.igg.ui;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace com.igg.editor
{
    public class AssetMenuTools
    {
        [MenuItem("Tools/AddressableAssetManager/AssetMenuTools/CheckSpriteReferenceByWhat?")]
        public static void CheckSpriteReferenceByAtlas()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            //var dependencies = AssetDatabase.GetDependencies(folderPath, true);
            //IGGDebug.LogError(string.Join("--", dependencies));
            //return;

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                com.igg.core.IGGDebug.LogError("Invalid folder path.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

            foreach (var guid in guids)
            {
                var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(guid), true);
                com.igg.core.IGGDebug.LogError(string.Join("--", dependencies));
            }

        }

        [MenuItem("Tools/AddressableAssetManager/AssetMenuTools/Check Sprite ReferencesBy In Scene")]
        private static void CheckSpriteReferences()
        {
            // Get all root objects in the scene
            GameObject[] rootObjects = GetAllRootObjectsInCurrentScene();

            List<GameObject> referencingObjects = new List<GameObject>();

            // Check each object to see if it references the selected sprite
            foreach (GameObject obj in rootObjects)
            {
                CheckObjectAndChildrenForSprite(obj, referencingObjects);
            }

            if (referencingObjects.Count > 0)
            {
                foreach (GameObject go in referencingObjects)
                {
                    com.igg.core.IGGDebug.Log($"- {go.name} (Path: {GetGameObjectPath(go)})");
                }
            }
            else
            {
                com.igg.core.IGGDebug.LogError($"no game object");
            }
        }

        private static void CheckObjectAndChildrenForSprite(GameObject obj, List<GameObject> referencingObjects)
        {
            // Check if the object has a SpriteRenderer component
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                //referencingObjects.Add(obj);
                com.igg.core.IGGDebug.Log($"- {obj.name} (Path: {GetGameObjectPath(obj)})" + spriteRenderer.sprite.name);

            }

            // Check if the object has an Image component (for UI elements)
            UnityEngine.UI.Image uiImage = obj.GetComponent<UnityEngine.UI.Image>();
            if (uiImage != null)
            {
                string spriteName = uiImage.sprite == null ? "null" : uiImage.sprite.name;
                //referencingObjects.Add(obj);
                com.igg.core.IGGDebug.Log($"- {obj.name} (Path: {GetGameObjectPath(obj)})" + spriteName);
            }

            // Recursively check all children
            foreach (Transform child in obj.transform)
            {
                CheckObjectAndChildrenForSprite(child.gameObject, referencingObjects);
            }
        }

        // Helper method to get the full path of a GameObject in the hierarchy
        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        // Helper method to get all root objects in the current scene
        private static GameObject[] GetAllRootObjectsInCurrentScene()
        {
            List<GameObject> rootObjects = new List<GameObject>();
            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                rootObjects.Add(obj);
            }
            return rootObjects.ToArray();
        }

        [MenuItem("Tools/AddressableAssetManager/AssetMenuTools/Check CommonPrefab Whether Refer NonCommon Resource")]
        static void CheckCommonPrefabWhetherReferNonCommonResource()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                com.igg.core.IGGDebug.LogError("Invalid folder path.");
                return;
            }

            var abGraph = BaseNodeCreator.ABResourceGraph;

            var getAssetRuleGuids = AddressabelUtilities.GetAssetRuleGuidsInFolder(folderPath);

            try
            {
                foreach (var guid in getAssetRuleGuids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(assetPath);
                    if (asset != null && asset.IsRuleUsed)
                    {
                        var node = abGraph.GetNode(guid);
                        if(node != null)
                        {
                            var Entries = node.AddressableAssetEntries;
                            foreach(var entry in Entries)
                            {
                                foreach(var dependency in entry.Value)
                                {
                                    if (dependency.EndsWith(".png") || dependency.EndsWith(".spriteatlas"))
                                    {
                                        if (!dependency.Contains("Common"))
                                        {
                                            com.igg.core.IGGDebug.LogError(entry.Key.AssetPath + " --> " + dependency);
                                        }
                                    }
                                }
                            }
                        }
                        //EditorUtility.DisplayProgressBar("Addressable Dependencies Graph", "Caculating Asset Dependencies, please wait...", currentCount / totalCount);
                    }
                }
            }
            catch (Exception e)
            {
                com.igg.core.IGGDebug.LogError(e.ToString());
            }
        }

        [MenuItem("Tools/AssetCheckTools/Check Prefabs Whether Refer NonCommon Resource")]
        static void CheckCommonPrefabWhetherReferNonCommonResourceInNormalWay()
        {
            string[] rulesGUID = GetFolderAssets();

            try
            {
                foreach (var guid in rulesGUID)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (asset != null)
                    {
                        List<string> allDependencies = new List<string>();
                        string[] indirectDps;
                        var prefabType = PrefabUtility.GetPrefabAssetType(asset);
                        if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                        {
                            indirectDps = AddressableCache.GetVariantDependencies(assetPath, false);
                            //AddressabelUtilities.GetEntryDependencies(allDependencies, indirectDps, false);
                        }
                        else
                        {
                            indirectDps = AddressableCache.GetDependencies(assetPath, false);
                            //AddressabelUtilities.GetEntryDependencies(allDependencies, indirectDps, false);
                        }

                        string[] directDependencies = indirectDps;// AddressableCache.GetVariantDependencies(assetPath, false);

                        foreach (var dependency in directDependencies)
                        {
                            if (dependency.EndsWith(".png") || dependency.EndsWith(".spriteatlas"))
                            {
                                if (!dependency.Contains("Common"))
                                {
                                    //look up for Sprite First, then Atlas
                                    if (dependency.EndsWith(".png"))
                                    {
                                        var dependenceAsset = AssetDatabase.LoadAssetAtPath<Sprite>(dependency);
                                        if (PrefabHasReference<Image, Sprite>(asset as GameObject, dependenceAsset, out var referImage))
                                        {
                                            string referGameObjectPath = GetGameObjectPath(referImage.gameObject);
                                            com.igg.core.IGGDebug.LogError(assetPath + " --> " + dependency + " Path: " + referGameObjectPath);
                                        }
                                        else if (PrefabHasReference<UIGeneralWindowConfig, Sprite>(asset as GameObject, dependenceAsset, out var referWindow))
                                        {
                                            string referGameObjectPath = GetGameObjectPath(referWindow.gameObject);
                                            com.igg.core.IGGDebug.LogError(assetPath + " --> " + dependency + " Path: " + referGameObjectPath);
                                        }
                                        else if (PrefabHasReference<UIGeneralWidgetConfig, Sprite>(asset as GameObject, dependenceAsset, out var referWidget))
                                        {
                                            string referGameObjectPath = GetGameObjectPath(referWidget.gameObject);
                                            com.igg.core.IGGDebug.LogError(assetPath + " --> " + dependency + " Path: " + referGameObjectPath);
                                        }
                                        else
                                        {
                                            com.igg.core.IGGDebug.LogError(assetPath + " <--> " + dependency);
                                        }
                                    }

                                    if (dependency.EndsWith(".spriteatlas"))
                                    {
                                        var dependenceAsset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(dependency);
                                        if (PrefabHasReference<Image, SpriteAtlas>(asset as GameObject, dependenceAsset, out var referImage))
                                        {
                                            string referGameObjectPath = GetGameObjectPath(referImage.gameObject);
                                            com.igg.core.IGGDebug.LogError(assetPath + " --> " + dependency + " Path: " + referGameObjectPath);
                                        }
                                        else if (PrefabHasReference<UIGeneralWindowConfig, SpriteAtlas>(asset as GameObject, dependenceAsset, out var referWindow))
                                        {
                                            string referGameObjectPath = GetGameObjectPath(referWindow.gameObject);
                                            com.igg.core.IGGDebug.LogError(assetPath + " --> " + dependency + " Path: " + referGameObjectPath);
                                        }
                                        else if (PrefabHasReference<UIGeneralWidgetConfig, SpriteAtlas>(asset as GameObject, dependenceAsset, out var referWidget))
                                        {
                                            string referGameObjectPath = GetGameObjectPath(referWidget.gameObject);
                                            com.igg.core.IGGDebug.LogError(assetPath + " --> " + dependency + " Path: " + referGameObjectPath);
                                        }
                                        else
                                        {
                                            com.igg.core.IGGDebug.LogError(assetPath + " <--> " + dependency);
                                        }
                                    }

                                }
                            }
                        }
                        //EditorUtility.DisplayProgressBar("Addressable Dependencies Graph", "Caculating Asset Dependencies, please wait...", currentCount / totalCount);
                    }
                }
            }
            catch (Exception e)
            {
                com.igg.core.IGGDebug.LogError(e.ToString());
            }
        }

        private static string[] GetFolderAssets()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                com.igg.core.IGGDebug.LogError("Invalid folder path.");
                return null;
            }

            string ruleFilter = string.Format("t:Object");
            string[]  rulesGUID = AssetDatabase.FindAssets(ruleFilter,
                new[] { folderPath });
            return rulesGUID;
        }

        public static bool PrefabHasReference<T, K>(GameObject prefab, K target, out Transform referComponent) where T : Component where K : Object 
        {
            referComponent = null;
            T[] components = prefab.GetComponentsInChildren<T>(true);

            foreach (T component in components)
            {
                SerializedObject so = new SerializedObject(component);
                SerializedProperty sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == target)
                        {
                            referComponent = component.transform;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        static bool PrefabHasReference(GameObject prefab, Object target, out Component referComponent)
        {
            referComponent = null;
            Component[] components = prefab.GetComponentsInChildren<Component>(true);

            foreach (Component component in components)
            {
                SerializedObject so = new SerializedObject(component);
                SerializedProperty sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == target)
                        {
                            referComponent = component;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [MenuItem("Tools/AssetCheckTools/Check CommonPrefabs Whether Refer Big UI Pic")]
        static void CheckCommonPrefabWhetherReferBigUIDesignPic()
        {
            const int dimenssion = 1000 * 1000;
            string[] rulesGUID = GetFolderAssets();

            try
            {
                foreach (var guid in rulesGUID)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (asset != null)
                    {
                        string[] directDependencies = AddressableCache.GetDependencies(assetPath, true);
                        foreach (var dependency in directDependencies)
                        {
                            if (dependency.EndsWith(".png"))
                            {
                                var uiAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(dependency);
                                if(uiAsset != null && (uiAsset.width * uiAsset.height >= dimenssion))
                                {
                                    com.igg.core.IGGDebug.LogError(assetPath + " <--> " + dependency);
                                }
                                //else
                                //{
                                //    com.igg.core.IGGDebug.LogError("Load Texture2D failed " + dependency);
                                //}

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                com.igg.core.IGGDebug.LogError(e.ToString());
            }
        }


        static string CommonSpriteFolder = "Assets/Art/UI/UICommon";

        [MenuItem("Assets/AssetTools/Move Sprite To UICommon")]
        static void MakeSpriteCommon()
        {
            Object selectedObject = Selection.activeObject;
            if (selectedObject == null || !(selectedObject is Sprite))
            {
                com.igg.core.IGGDebug.LogError("Please select a sprite!");
                return;
            }

            string spritePath = AssetDatabase.GetAssetPath(selectedObject);
            string parentDirectory = System.IO.Path.GetDirectoryName(spritePath);
            string parentDirectoryName = Path.GetFileName(parentDirectory);

            string targetDirectory = "Assets/Art/UI/UICommon/" + parentDirectoryName;

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
                AssetDatabase.Refresh();
            }

            string targetPath = Path.Combine(targetDirectory, selectedObject.name + ".png");

            AssetDatabase.MoveAsset(spritePath, targetPath);
            AssetDatabase.Refresh();

            com.igg.core.IGGDebug.Log($"Move Sprite to：{targetPath}");

        }

        [MenuItem("Assets/AssetTools/Move Folder To UICommon")]
        static void MakeFolderToCommon()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                com.igg.core.IGGDebug.LogError("Invalid folder path.");
                return;
            }
            string folderName = Path.GetFileName(folderPath);

            string parentDirectory = System.IO.Path.GetDirectoryName(folderPath);

            string targetPath = Path.Combine("Assets/Art/UI/UICommon/", folderName);

            //IGGDebug.Log($"Move Sprite to：{targetPath}");
            AssetDatabase.MoveAsset(folderPath, targetPath);
            AssetDatabase.Refresh();

            com.igg.core.IGGDebug.Log($"Move Sprite to：{targetPath}");

        }

        [MenuItem("Assets/AssetTools/Move Feature UI Folder To Art")]
        static void MoveFolderToArt()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                com.igg.core.IGGDebug.LogError("Invalid folder path.");
                return;
            }

            var removePre = folderPath.Replace("Assets/Addressables/", "");

            string targetPath = Path.Combine("Assets/Art/", removePre);
            string parentDic = targetPath.Substring(0, targetPath.LastIndexOf("/"));

            if (!Directory.Exists(parentDic))
            {
                Directory.CreateDirectory(parentDic);
            }

            //IGGDebug.Log($"Move Sprite to：{targetPath}");
            AssetDatabase.MoveAsset(folderPath, targetPath);
            AssetDatabase.Refresh();

            com.igg.core.IGGDebug.Log($"Move Folder to：{targetPath}");

        }

    }
}
