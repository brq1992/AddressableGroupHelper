using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace AddressableAssetTool.Graph
{
    public class AddressableDependenciesGraph : EditorWindow
    {
        public readonly Dictionary<string, Node> m_GUIDNodeLookup = new Dictionary<string, Node>();

        private GraphView m_GraphView;
        private readonly List<Object> SelectedObjects = new List<Object>();
        internal readonly List<AddressableBaseGroup> _addressableGroups = new List<AddressableBaseGroup>();
        private const float kNodeWidth = 250.0f;
        private Toggle AlignmentToggle;
        private BaseLayout _baseLayout;

        [MenuItem("Tools/AddressableAssetManager/Dependency Graph")]
        public static void CreateTestGraphViewWindow()
        {
            var window = GetWindow<AddressableDependenciesGraph>();
            window.titleContent = new GUIContent("Addressable Dependency Graph");
        }

        public void OnEnable()
        {
            CreateGraph();
        }

        void CreateGraph()
        {
            m_GraphView = new AddressableGraphView(this)
            {
                name = "Dependency Graph",
            };

            VisualElement toolbar = CreateToolbar();
            //VisualElement toolbar2 = CreateFilterbar();

            rootVisualElement.Add(toolbar);
            //rootVisualElement.Add(toolbar2);
            rootVisualElement.Add(m_GraphView);
            m_GraphView.StretchToParentSize();
            toolbar.BringToFront();
            //toolbar2.BringToFront();


            _baseLayout = new BaseLayout(_addressableGroups);
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement
            {
                style =
            {
                flexDirection = FlexDirection.Row,
                flexGrow = 0,
                backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.75f)
            }
            };

            var options = new VisualElement
            {
                style = { alignContent = Align.Center }
            };

            toolbar.Add(options);
            toolbar.Add(new Button(AddElements)
            {
                text = "Add Asset",
            });
            toolbar.Add(new Button(ClearGraph)
            {
                text = "Clear"
            });

            //AlignmentToggle = new Toggle();
            //AlignmentToggle.text = "Horizontal Layout";
            //AlignmentToggle.value = false;
            //AlignmentToggle.RegisterValueChangedCallback(x => {
            //    ResetAllNodes();
            //});
            //toolbar.Add(AlignmentToggle);

            return toolbar;
        }

        private void ClearGraph()
        {
            SelectedObjects.Clear();

            foreach (var assetGroup in _addressableGroups)
            {
                EmptyGroup(assetGroup);
            }

            m_GUIDNodeLookup.Clear();

            _addressableGroups.Clear();
        }

        void EmptyGroup(AddressableBaseGroup assetGroup)
        {
            if (assetGroup.m_AssetConnections.Count > 0)
            {
                foreach (var edge in assetGroup.m_AssetConnections)
                {
                    m_GraphView.RemoveElement(edge);
                }
            }
            assetGroup.m_AssetConnections.Clear();

            foreach (var node in assetGroup.m_AssetNodes)
            {
                m_GraphView.RemoveElement(node);
            }
            assetGroup.m_AssetNodes.Clear();

            assetGroup.m_DependenciesForPlacement.Clear();

            //if (assetGroup.SharedGroup != null) {
            //    EmptyGroup(assetGroup.SharedGroup);
            //}

            m_GraphView.RemoveElement(assetGroup.groupNode);

            assetGroup.groupNode = null;
        }


        public void AddElements(Object[] objs)
        {
            foreach (var obj in objs)
            {
                //Prevent readding same object
                if (SelectedObjects.Contains(obj))
                {
                    Debug.Log("Object already loaded");
                    return;
                }
                SelectedObjects.Add(obj);

                AddressableBaseGroup adGroup = AddressableBaseGroupFactory.GetGroup(obj, this);// new AddressableBaseGroup();
                _addressableGroups.Add(adGroup);



                adGroup.assetPath = AssetDatabase.GetAssetPath(obj);

                //assetPath will be empty if obj is null or isn't an asset (a scene object)
                if (obj == null || string.IsNullOrEmpty(adGroup.assetPath))
                    return;

                adGroup.groupNode = new Group { title = obj.name };

                Object mainObject = AssetDatabase.LoadMainAssetAtPath(adGroup.assetPath);

                if (mainObject == null)
                {
                    Debug.Log("Object doesn't exist anymore");
                    return;
                }

                //string[] dependencies = adGroup.GetDependencies(); // AssetDatabase.GetDependencies(adGroup.assetPath, false);

                //ExtroctMethod(adGroup, mainObject, m_GraphView, UpdateGroupDependencyNodePlacement, this);

                adGroup.DrawGroup(adGroup, mainObject, m_GraphView, UpdateGroupDependencyNodePlacement, this);

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

        private void AddElements()
        {
            Object[] objs = Selection.objects;


            AddElements(objs);


           
        }

         static void ExtroctMethod(AddressableBaseGroup adGroup, Object obj, GraphView m_GraphView, 
             EventCallback<GeometryChangedEvent, AddressableBaseGroup> UpdateGroupDependencyNodePlacement, AddressableDependenciesGraph graphWindow)
        {
            adGroup.assetPath = AssetDatabase.GetAssetPath(obj);

            //assetPath will be empty if obj is null or isn't an asset (a scene object)
            if (obj == null || string.IsNullOrEmpty(adGroup.assetPath))
                return;

            adGroup.groupNode = new Group { title = obj.name };


            Object mainObject = AssetDatabase.LoadMainAssetAtPath(adGroup.assetPath);

            if (mainObject == null)
            {
                Debug.Log("Object doesn't exist anymore");
                return;
            }

            string[] dependencies = adGroup.GetDependencies(); // AssetDatabase.GetDependencies(adGroup.assetPath, false);

            adGroup.mainNode = CreateNode(adGroup, mainObject, adGroup.assetPath, true, dependencies.Length, graphWindow.m_GUIDNodeLookup);
            adGroup.mainNode.userData = 0;

            Rect position = new Rect(0, 0, 0, 0);
            adGroup.mainNode.SetPosition(position);

            if (!m_GraphView.Contains(adGroup.groupNode))
            {
                m_GraphView.AddElement(adGroup.groupNode);
            }

            m_GraphView.AddElement(adGroup.mainNode);

            adGroup.groupNode.AddElement(adGroup.mainNode);

            CreateDependencyNodes(adGroup, dependencies, adGroup.mainNode, adGroup.groupNode, 1, m_GraphView, graphWindow.m_GUIDNodeLookup);

            adGroup.m_AssetNodes.Add(adGroup.mainNode);

            adGroup.groupNode.capabilities &= ~Capabilities.Deletable;

            adGroup.groupNode.Focus();

            adGroup.mainNode.RegisterCallback<GeometryChangedEvent, AddressableBaseGroup>(
                UpdateGroupDependencyNodePlacement, adGroup
            );
        }

        private static Node CreateNode(AddressableBaseGroup AddressableGroup, Object obj, string assetPath, bool isMainNode, 
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

        private static void CreateDependencyNodes(AddressableBaseGroup AddressableGroup, string[] dependencies, Node parentNode, Group groupNode, int depth, 
            GraphView m_GraphView, Dictionary<string, Node> m_GUIDNodeLookup)
        {
            //Debug.Log(depth);

            foreach (string dependencyString in dependencies)
            {
                Object dependencyAsset = AssetDatabase.LoadMainAssetAtPath(dependencyString);
                string[] deeperDependencies = AssetDatabase.GetDependencies(dependencyString, false);

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

        private void UpdateGroupDependencyNodePlacement(GeometryChangedEvent e, AddressableBaseGroup baseGroup)
        {
            
            baseGroup.mainNode.UnregisterCallback<GeometryChangedEvent, AddressableBaseGroup>(
                UpdateGroupDependencyNodePlacement
            );

            ResetNodes(baseGroup);

            Rect pos = _baseLayout.GetNewNodePostion();
            //baseGroup.groupNode.SetPosition(pos);
            //baseGroup.mainNode.SetPosition(pos);
            baseGroup.SetPosition(pos);
        }

        void ResetNodes(AddressableBaseGroup assetGroup)
        {
            // The current y offset in per depth
            var depthOffset = new Dictionary<int, float>();

            foreach (var node in assetGroup.m_DependenciesForPlacement)
            {
                int depth = (int)node.userData;

                if (!depthOffset.ContainsKey(depth))
                    depthOffset.Add(depth, 0.0f);

                if (AlignmentToggle.value)
                {
                    depthOffset[depth] += node.layout.height;
                }
                else
                {
                    depthOffset[depth] += node.layout.width;
                }
            }

            // Move half of the node into negative y space so they're on either size of the main node in y axis
            var depths = new List<int>(depthOffset.Keys);
            foreach (int depth in depths)
            {
                if (depth == 0)
                    continue;

                float offset = depthOffset[depth];
                depthOffset[depth] = (0f - offset / 2.0f);
            }

            Rect mainNodeRect = assetGroup.mainNode.GetPosition();

            foreach (var node in assetGroup.m_DependenciesForPlacement)
            {
                int depth = (int)node.userData;
                //Debug.Log(node.layout);
                if (AlignmentToggle.value)
                {
                    //node.SetPosition(new Rect(mainNodeRect.x + kNodeWidth * 1.5f * depth, mainNodeRect.y + depthOffset[depth], 0, 0));
                    node.SetPosition(new Rect(mainNodeRect.x + node.layout.width * 1.5f * depth, mainNodeRect.y + depthOffset[depth], 0, 0));
                }
                else
                {
                    node.SetPosition(new Rect(mainNodeRect.x + depthOffset[depth], mainNodeRect.y + node.layout.height * 1.5f * depth, 0, 0));
                    //node.SetPosition(new Rect(mainNodeRect.x + depthOffset[depth], mainNodeRect.y + kNodeWidth * 1.5f * depth, 0, 0));
                }

                if (AlignmentToggle.value)
                {
                    depthOffset[depth] += node.layout.height;
                }
                else
                {
                    depthOffset[depth] += node.layout.width;
                }
            }
        }

        void ResetAllNodes()
        {
            foreach (var assetGroup in _addressableGroups)
            {
                ResetNodes(assetGroup);
            }
        }

        public static StyleColor GetColorByAssetType(Object obj)
        {
            var typeName = obj.GetType().Name;
            //Debug.Log(obj.GetType());
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
    }
}

