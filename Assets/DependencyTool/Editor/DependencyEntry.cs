
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using Object = UnityEngine.Object;

namespace AddressableAssetTool
{
    public class DependencyEntry : MonoBehaviour
    {
        private const string buildInGroupName = "Built In Data";
        private static Graph<Object> _graph = new Graph<Object>();

        [MenuItem("Tools/CalcDepen")]
        public static void StartDependency()
        {
            var addressableAssetProfileSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (addressableAssetProfileSettings == null)
            {
                Debug.LogError("addressableAssetProfileSettings is null!");
                return;
            }
            //Debug.Log(addressableAssetProfileSettings.groups.Count);

            var list = new List<AddressableAssetGroup>(addressableAssetProfileSettings.groups);
            var index = list.FindIndex(x => x.name == buildInGroupName);
            //Debug.LogError(index + " " + list[index].Name);
            if (index != -1)
                list.RemoveAt(index);


            foreach (var item in list)
            {
                var results = item.entries;
                var schema = item.GetSchema<BundledAssetGroupSchema>();
                //Debug.Log("group name " + item.name + " schema.BundleMode " + schema.BundleMode);
                foreach (var entry in results)
                {
                    string assetPath = entry.AssetPath;
                    Debug.Log(assetPath);
                    var selectObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    GetAssetDependenciesByAssetPath(selectObj, assetPath);
                }
                Debug.Log("------------------------");
            }


            _graph.OutputAllNode();

        }


        [MenuItem("Tools/GetSelectAssetDependencies")]
        public static void GetAssetDependencies()
        {
            var selectObj = Selection.activeObject;
            if (selectObj == null)
            {
                EditorUtility.DisplayDialog("Warning", "Please select an asset in Project Window", "Ok");
                return;
            }

            //todo:check whether this selectObj is a GameObject from Hierachy View.
            var selectObjPath = AssetDatabase.GetAssetPath(selectObj);
            GetAssetDependenciesByAssetPath(selectObj, selectObjPath);
        }

        private static void GetAssetDependenciesByAssetPath(Object selectObj, string selectObjPath)
        {
            //var dependencies = AssetDatabase.GetDependencies(selectObjPath,false);
            int instanceId = selectObj.GetInstanceID();
            //Debug.LogError("rootId " + instanceId);
            var rootNode = _graph.AddNode(instanceId, selectObj);
            //Debug.LogError("root path " + selectObjPath);
            GetDependenciesAndAddNoes(selectObjPath, rootNode);



            //_graph.OutputAllNode();
        }

        static void GetDependenciesAndAddNoes(string assetPath, Node<Object> rootNode)
        {
            //var directDependencies = AddressableCache.GetDependencies(assetPath, false);
            //foreach (var path in directDependencies)
            //{
            //    //Debug.LogError("child path " + path);
            //    var directAssetObject = AssetDatabase.LoadAssetAtPath<Object>(path);
            //    var assetId = directAssetObject.GetInstanceID();
            //    var directDependencyNode = _graph.AddNode(assetId, directAssetObject);
            //    _graph.AddEdge(rootNode.Id, directDependencyNode.Id);
            //    //Debug.LogError("assetId "+ assetId);

            //    //get child
            //    GetDependenciesAndAddNoes(path, directDependencyNode);

            //}
        }
    }
}
