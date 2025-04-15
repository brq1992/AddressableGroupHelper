
using AssetUsageFinder;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace AddressableAssetTool.Graph
{
    public class AddressableDependenciesGraph : GraphWindow
    {



        [MenuItem("Tools/AddressableAssetManager/Preparing Dependency Graph Data")]
        public static void CreateAddressableDependenciesGraphWindowWithData()
        {
            CreateGraph(null);
        }

        [MenuItem("Tools/AddressableAssetManager/Open Dependency Graph Window")]
        public static void CreateAddressableDependenciesGraphWindowWithoutData()
        {
            var window = GetWindow<AddressableDependenciesGraph>();
            window.titleContent = new GUIContent("Addressable Dependency Graph Without Preparing All data");
        }

        [MenuItem("Tools/AddressableAssetManager/CheckReference")]
        public static void CheckReference()
        {
            CreateGraph(CalculateCircularReference);
        }

        private static void CreateGraph(Action action)
        {
            if(!BaseNodeCreator.NewNodeInit)
            {
                var context = AddressableAssetSettingsDefaultObject.Settings;
                var rule = new CheckBundleDupeDependenciesMultiIsolatedGroups();
                rule.ClearAnalysis();
                var result = rule.CheckDependencies(context);

                if(result == null)
                {
                    Debug.LogError(" Check Dependencies return null ");
                    return;
                }
                BaseNodeCreator.NewNodeInit = true;
                var getAssetRuleGuids = AddressabelUtilities.GetAssetRuleGuidsInFolder("Assets");

                BaseNodeCreator.ClearABGraphData();
                AddressableCache.CacheClear();

                int totalCount = getAssetRuleGuids.Length;
                float currentCount = 0;
                try
                {
                    foreach (var guid in getAssetRuleGuids)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(assetPath);
                        if (asset != null && asset.IsRuleUsed)
                        {
                            var baseNodeCreator = GroupNodeCreatorFactory.GetCreator(asset);
                            baseNodeCreator.CreateNode(guid, null, result);
                            currentCount++;
                            EditorUtility.DisplayProgressBar("Addressable Dependencies Graph", "Caculating Asset Dependencies, please wait...", currentCount / totalCount);
                        }
                    }
                }
                catch (Exception e)
                {
                    com.igg.core.IGGDebug.LogError(e.ToString());
                }
            }

            action?.Invoke();

            EditorUtility.ClearProgressBar();
        }

        private static void CalculateCircularReference()
        {
            var circularReferences = BaseNodeCreator.ABResourceGraph.GetAllCircularReferences();
            com.igg.core.IGGDebug.LogError("Circular References:");
            foreach (var circularPath in circularReferences)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var guid in circularPath)
                {
                    builder.Append(AssetDatabase.GUIDToAssetPath(guid) + " -> ");
                }
                com.igg.core.IGGDebug.LogError(builder.ToString());
            }
        }

        [MenuItem("Tools/AddressableAssetManager/CheckReferenceDepth")]
        public static void CheckReferenceDepth()
        {
            CreateGraph(CalculateDepth);
        }

        private static void CalculateDepth()
        {
            IEnumerable<ResourceNode> nodes = BaseNodeCreator.ABResourceGraph.GetAllNodes();
            foreach (var node in nodes)
            {
                List<string> paths = new List<string>();
                int depth = BaseNodeCreator.ABResourceGraph.GetReferenceDepth(node.ResourceId, out paths);
                string output = string.Join("-> ", paths);
                if (depth > 3)
                {
                    com.igg.core.IGGDebug.LogError($"Resource {AssetDatabase.GUIDToAssetPath(node.ResourceId)} depth: {--depth} paths {output}");
                }
            }
        }

        [MenuItem("Tools/AddressableAssetManager/ClearReferenceData")]
        private static void ClearReferenceData()
        {
            BaseNodeCreator.NewNodeInit = false;
            BaseNodeCreator.ClearABGraphData();
            AddressableCache.CacheClear();
        }


        public static void CheckCommonFeatureReference()
        {
            CreateGraph(CalculateCommonReference);
        }

        private static void CalculateCommonReference()
        {
            var commonPath = "";
        }

        public override void AddElements(Object[] objs)
        {
            foreach(var obj in objs)
            {
                var itemPath = AssetDatabase.GetAssetPath(obj);
                if(AssetDatabase.IsValidFolder(itemPath))
                {
                    var getAssetRuleGuids = AddressabelUtilities.GetAssetRuleGuidsInFolder(itemPath);
                    foreach(var guid in getAssetRuleGuids)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetRule>(assetPath);
                        if(asset != null && asset.IsRuleUsed)
                        {
                            AddElement(asset);
                        }
                    }
                    continue;
                }

                var assetRule = obj as AddressableAssetRule;
                if(assetRule == null || !assetRule.IsRuleUsed)
                {
                    continue;
                }

                var objPath = AssetDatabase.GetAssetPath(obj);

                //Prevent readding same object
                if (SelectedObjects.Contains(obj))
                {
                    string name = obj.name;
                    com.igg.core.IGGDebug.Log("Object " + name + " already loaded " + objPath);
                    return;
                }


                //assetPath will be empty if obj is null or isn't an asset (a scene object)
                if (obj == null || string.IsNullOrEmpty(objPath))
                {
                    com.igg.core.IGGDebug.Log("objPath is NullorEmpty");
                    return;
                }

                Object mainObject = AssetDatabase.LoadMainAssetAtPath(objPath);

                if (mainObject == null)
                {
                    com.igg.core.IGGDebug.Log("Object doesn't exist anymore");
                    return;
                }

                var adGroup = AddressableBaseGroupFactory.GetGroup(obj, this, BaseNodeCreator.NewNodeInit);// new AddressableBaseGroup();
                _addressableGroups.Add(adGroup);

                //adGroup._assetRulePath = objPath;

                com.igg.core.IGGDebug.Log("add obj " + obj.name + " to selectlist " + objPath);
                SelectedObjects.Add(obj);

                adGroup.groupNode = new Group { title = obj.name };

                //string[] dependencies = adGroup.GetDependencies(); // AssetDatabase.GetDependencies(adGroup.assetPath, false);

                //ExtroctMethod(adGroup, mainObject, m_GraphView, UpdateGroupDependencyNodePlacement, this);

                adGroup.DrawGroup(m_GraphView, UpdateGroupDependencyNodePlacement, this);

                //adGroup.mainNode = CreateNode(adGroup, mainObject, adGroup.assetPath, true, dependencies.Length, m_GUIDNodeLookup);
                //adGroup.mainNode.userData = 0;

                //Rect position = new Rect(0, 0, 0, 0);
                //adGroup.mainNode.SetPosition(position);

                //if (!m_GraphView.Contains(adGroup.groupNode))
                //{
                //    m_GraphView.AddElement(adGroup.groupNode);
                //}

                //m_GraphView.AddElement(adGroup.mainNode);

                //adGroup.groupNode.AddElement(adGroup.mainNode);

                /////*adGroup.*/CreateDependencyNodes(adGroup, dependencies, adGroup.mainNode, adGroup.groupNode, 1);

                //adGroup.m_AssetNodes.Add(adGroup.mainNode);

                //adGroup.groupNode.capabilities &= ~Capabilities.Deletable;

                //adGroup.groupNode.Focus();

                //adGroup.mainNode.RegisterCallback<GeometryChangedEvent, AddressableBaseGroup>(
                //    UpdateGroupDependencyNodePlacement, adGroup
                //);
            }
        }

        internal override void AddElement(object asset)
        {
            AddElement((AddressableAssetRule)asset);
        }

        public void AddElement(AddressableAssetRule obj)
        {
            var objPath = AssetDatabase.GetAssetPath(obj);

            //Prevent readding same object
            if (SelectedObjects.Contains(obj))
            {
                string name = obj.name;
                com.igg.core.IGGDebug.Log("Rule " + name + " already loaded " + objPath);
                return;
            }


            //assetPath will be empty if obj is null or isn't an asset (a scene object)
            if (obj == null || string.IsNullOrEmpty(objPath))
            {
                com.igg.core.IGGDebug.Log("objPath is NullorEmpty");
                return;
            }

            Object mainObject = AssetDatabase.LoadMainAssetAtPath(objPath);

            if (mainObject == null)
            {
                com.igg.core.IGGDebug.Log("Rule doesn't exist anymore");
                return;
            }

            if(!obj.IsRuleUsed)
            {
                com.igg.core.IGGDebug.Log("Rule isn't used!");
                return;
            }

            var adGroup = AddressableBaseGroupFactory.GetGroup(obj, this, true);// new AddressableBaseGroup();
            _addressableGroups.Add(adGroup);

            //adGroup._assetRulePath = objPath;

            //com.igg.core.IGGDebug.Log("add obj " + obj.name + " to selectlist " + objPath);
            SelectedObjects.Add(obj);

            //adGroup.groupNode = new Group { title = obj.name };

            adGroup.DrawGroup(m_GraphView, UpdateGroupDependencyNodePlacement, this);
        }

        private static Node CreateNode(AddressableGraphBaseGroup AddressableGroup, Object obj, string assetPath, bool isMainNode, 
            int dependencyAmount, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            Node resultNode;
            string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            if (m_GUIDNodeLookup.TryGetValue(assetGUID, out resultNode))
            {
                return resultNode;
            }

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var assetGuid, out long _))
            {
                var objNode = new Node
                {
                    title = obj.name,
                    style =
                {
                    width = kNodeWidth
                }
                };

                objNode.extensionContainer.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 0.8f);

                #region Select button
                objNode.titleContainer.Add(new Button(() =>
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                })
                {
                    style =
            {
                        height = 16.0f,
                        alignSelf = Align.Center,
                        alignItems = Align.Center
                    },
                    text = "Select"
                });
                #endregion

                #region Padding
                var infoContainer = new VisualElement
                {
                    style =
                    {
                    paddingBottom = 4.0f,
                    paddingTop = 4.0f,
                    paddingLeft = 4.0f,
                    paddingRight = 4.0f
                }
                };
                #endregion

                #region Asset Path, maybe removed to improve visibility with large amount of assets
//                infoContainer.Add(new Label
//                {
//                    text = assetPath,
//#if UNITY_2019_1_OR_NEWER
//                    style = { whiteSpace = WhiteSpace.Normal }
//#else
//                                style = { wordWrap = true }
//#endif
//                });
                #endregion

                #region Asset type
                var typeName = obj.GetType().Name;
                if (isMainNode)
                {
                    var prefabType = PrefabUtility.GetPrefabAssetType(obj);
                    if (prefabType != PrefabAssetType.NotAPrefab)
                        typeName = $"{prefabType} Prefab";
                }

                var typeLabel = new Label
                {
                    text = $"Type: {typeName}",
                };
                infoContainer.Add(typeLabel);

                objNode.extensionContainer.Add(infoContainer);
                #endregion

                var typeContainer = new VisualElement
                {
                    style =
                    {
                    paddingBottom = 4.0f,
                    paddingTop = 4.0f,
                    paddingLeft = 4.0f,
                    paddingRight = 4.0f,
                    backgroundColor = GetColorByAssetType(obj)
        }
                };

                objNode.extensionContainer.Add(typeContainer);

                #region Node Icon, replaced with color 
                //Texture assetTexture = AssetPreview.GetAssetPreview(obj);
                //if (!assetTexture)
                //    assetTexture = AssetPreview.GetMiniThumbnail(obj);

                //if (assetTexture)
                //{
                //    AddDivider(objNode);

                //    objNode.extensionContainer.Add(new Image
                //    {
                //        image = assetTexture,
                //        scaleMode = ScaleMode.ScaleToFit,
                //        style =
                //        {
                //            paddingBottom = 4.0f,
                //            paddingTop = 4.0f,
                //            paddingLeft = 4.0f,
                //            paddingRight = 4.0f
                //        }
                //    });
                //} 
                #endregion

                // Ports
                //if (!isMainNode) {
                Port realPort = objNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Object));
                realPort.portName = "Dependent";
                objNode.inputContainer.Add(realPort);
                //}

                if (dependencyAmount > 0)
                {
#if UNITY_2018_1
                Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, typeof(Object));
#else
                    Port port = objNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Object));
#endif
                    port.portName = dependencyAmount + " Dependencies";
                    objNode.outputContainer.Add(port);
                    objNode.RefreshPorts();
                }

                resultNode = objNode;

                resultNode.RefreshExpandedState();
                resultNode.RefreshPorts();
                resultNode.capabilities &= ~Capabilities.Deletable;
                resultNode.capabilities |= Capabilities.Collapsible;
            }
            m_GUIDNodeLookup[assetGUID] = resultNode;
            return resultNode;
        }

        private static void CreateDependencyNodes(AddressableGraphBaseGroup AddressableGroup, string[] dependencies, Node parentNode, Group groupNode, int depth, 
            GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            //IGGDebug.Log(depth);

            foreach (string dependencyString in dependencies)
            {
                Object dependencyAsset = AssetDatabase.LoadMainAssetAtPath(dependencyString);
                string[] deeperDependencies = AddressableCache.GetDependencies(dependencyString, false);

                var typeName = dependencyAsset.GetType().Name;


                Node dependencyNode = CreateNode(AddressableGroup, dependencyAsset, AssetDatabase.GetAssetPath(dependencyAsset),
                    false, deeperDependencies.Length, m_GUIDNodeLookup);

                if (!AddressableGroup.m_AssetNodes.Contains(dependencyNode))
                {
                    dependencyNode.userData = depth;
                }

                CreateDependencyNodes(AddressableGroup, deeperDependencies, dependencyNode, groupNode, depth + 1, m_GraphView, m_GUIDNodeLookup);

                //if the node doesnt exists yet, put it in the group
                if (!m_GraphView.Contains(dependencyNode))
                {
                    m_GraphView.AddElement(dependencyNode);

                    AddressableGroup.m_DependenciesForPlacement.Add(dependencyNode);
                    groupNode.AddElement(dependencyNode);
                }
                else
                {
                    //TODO: if it already exists, put it in a separate group for shared assets
                    //Check if the dependencyNode is in the same group or not
                    //if it's a different group move it to a new shared group
                    /*
                    if (SharedToggle.value) {
                        if (!AddressableGroup.m_AssetNodes.Contains(dependencyNode)) {
                            if (AddressableGroup.SharedGroup == null) {
                                AddressableGroup.SharedGroup = new AddressableGroup();

                                AddressableGroups.Add(AddressableGroup.SharedGroup);
                                AddressableGroup.SharedGroup.assetPath = AddressableGroup.assetPath;

                                AddressableGroup.SharedGroup.groupNode = new Group { title = "Shared Group" };

                                AddressableGroup.SharedGroup.mainNode = dependencyNode;
                                AddressableGroup.SharedGroup.mainNode.userData = 0;
                            }

                            if (!m_GraphView.Contains(AddressableGroup.SharedGroup.groupNode)) {
                                m_GraphView.AddElement(AddressableGroup.SharedGroup.groupNode);
                            }

                            //add the node to the group and remove it from the previous group
                            AddressableGroup.m_AssetNodes.Remove(dependencyNode);
                            //AddressableGroup.groupNode.RemoveElement(dependencyNode);
                            AddressableGroup.m_DependenciesForPlacement.Remove(dependencyNode);

                            AddressableGroup.SharedGroup.m_DependenciesForPlacement.Add(dependencyNode);

                            if (!AddressableGroup.SharedGroup.groupNode.ContainsElement(dependencyNode)) {
                                AddressableGroup.SharedGroup.groupNode.AddElement(dependencyNode);
                            }

                            AddressableGroup.SharedGroup.m_AssetNodes.Add(dependencyNode);
                        }
                    }*/
                }

                Edge edge = CreateEdge(dependencyNode, parentNode, m_GraphView);

                AddressableGroup.m_AssetConnections.Add(edge);
                AddressableGroup.m_AssetNodes.Add(dependencyNode);
            }
        }

        static Edge CreateEdge(Node dependencyNode, Node parentNode, GraphView m_GraphView)
        {
            Edge edge = new Edge
            {
                input = dependencyNode.inputContainer[0] as Port,
                output = parentNode.outputContainer[0] as Port,
            };
            edge.input?.Connect(edge);
            edge.output?.Connect(edge);

            dependencyNode.RefreshPorts();

            m_GraphView.AddElement(edge);

            edge.capabilities &= ~Capabilities.Deletable;

            return edge;
        }

        public static StyleColor GetColorByAssetType(Object obj)
        {
            var typeName = obj.GetType().Name;
            //IGGDebug.Log(obj.GetType());
            switch (typeName)
            {
                case "MonoScript":
                    return Color.black;
                case "Material":
                    return new Color(0.1f, 0.5f, 0.1f);   //green
                case "Texture2D":
                    return new Color(0.5f, 0.1f, 0.1f); //red
                case "RenderTexture":
                    return new Color(0.8f, 0.1f, 0.1f); //red
                case "Shader":
                    return new Color(0.1f, 0.1f, 0.5f); //dark blue
                case "ComputeShader":
                    return new Color(0.1f, 0.1f, 0.5f); //dark blue
                case "GameObject":
                    return new Color(0f, 0.8f, 0.7f); //light blue
                case "AnimationClip":
                    return new Color(1, 0.7f, 1); //pink
                case "AnimatorController":
                    return new Color(1, 0.7f, 0.8f); //pink
                case "AudioClip":
                    return new Color(1, 0.8f, 0); //orange
                case "AudioMixerController":
                    return new Color(1, 0.8f, 0); //orange
                case "Font":
                    return new Color(0.9f, 1, 0.9f); //light green
                case "TMP_FontAsset":
                    return new Color(0.9f, 1, 0.9f); //light green
                case "Mesh":
                    return new Color(0.5f, 0, 0.5f); //purple
                case "TerrainLayer":
                    return new Color(0.5f, 0.8f, 0f);   //green
                default:
                    break;
            }

            return CustomColor(typeName);
            //return new Color(0.24f, 0.24f, 0.24f, 0.8f);
        }

        static StyleColor CustomColor(string assetType)
        {
            switch (assetType)
            {
                case "GearObject":
                    return new Color(0.9f, 0, 0.9f); //pink
                case "TalentObject":
                    return new Color(0.9f, 0, 0.9f); //
                case "AbilityInfo":
                    return new Color(0.9f, 0, 0.9f); //
                case "HealthSO":
                    return new Color(0.9f, 0, 0.9f); //
                default:
                    break;
            }

            //standard color
            return new Color(0.24f, 0.24f, 0.24f, 0.8f);
        }


        #region Edge Info Window






        #endregion

        #region Graph Mouse Event

        public override void OnMouseUp(MouseUpEvent evt)
        {
            // click not happen on Edge
            if (m_GraphView.Contains(_infoWindow))
            {
                m_GraphView.Remove(_infoWindow);
            }
        }

        #endregion

        //internal void AddAndPosGroupNode(Group groupNode)
        //{
        //    Rect pos = BaseLayout.GetNewGroupNodePosition(_groups);
        //    groupNode.SetPosition(pos);
        //    _groups.Add(groupNode);
        //}


    }
}

