
using AssetUsageFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace AddressableAssetTool.Graph
{
    public class GraphWindow : EditorWindow
    {
        protected GraphView m_GraphView;
        protected VisualElement _infoWindow;
        protected Toggle AlignmentToggle;

        protected Node _currentHilightNode;
        public readonly Dictionary<string, Node> m_GUIDNodeLookup = new Dictionary<string, Node>();


        protected readonly List<Object> SelectedObjects = new List<Object>();
        internal readonly List<GraphBaseGroup> _addressableGroups = new List<GraphBaseGroup>();
        protected readonly List<Node> _groups = new List<Node>();
        protected const float kNodeWidth = 250.0f;

        public virtual void OnMouseUp(MouseUpEvent evt)
        {
            throw new NotImplementedException();
        }

        public virtual void AddElements(UnityEngine.Object[] objectReferences)
        {
            throw new NotImplementedException();
        }

        public void OnEnable()
        {
            CreateGraph();
        }
        internal void ShowInfoWindow(MouseUpEvent evt)
        {
            VisualElement element = evt.currentTarget as VisualElement;
            if (element == null)
            {
                return;
            }
            Edge edge = evt.currentTarget as Edge;
            if (edge == null)
            {
                return;
            }
            var data = edge.userData as List<EdgeUserData>;
            if (data == null)
            {
                return;
            }


            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Dependence: \\n");
            for (int i = 0; i < data.Count; i++)
            {
                string value = data[i].ParentPath + " -> " + data[i].Dependence + "\\n";
                stringBuilder.Append(value);
            }

            //IGGDebug.LogError(data.ParentPath + "  " + data.Dependence);
            _infoWindow.Q<Label>("info").text = stringBuilder.ToString();
            //_infoWindow.Q<Image>("info-image").image = EditorGUIUtility.IconContent("console.infoicon").image;

            //_infoWindow.Q<Label>("info").text = "Dependence: " + data.ParentPath + " -> " + data.Dependence;

            _infoWindow.Q<Button>("ShowReliance").userData = data;



            if (!m_GraphView.Contains(_infoWindow))
            {
                m_GraphView.Add(_infoWindow);
            }

            _infoWindow.style.left = m_GraphView.contentContainer.resolvedStyle.width - _infoWindow.resolvedStyle.width;
            _infoWindow.style.top = 20;


            //Vector2 mousePosition = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            //foreach (var edge in this.Query<Edge>().ToList())
            //{
            //    if (edge.worldBound.Contains(mousePosition))
            //    {
            //        com.igg.core.IGGDebug.LogError("erroe");
            //        evt.StopPropagation();
            //        break;
            //    }
            //}
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

            _infoWindow = CreateInfoWindow();

        }
        internal void AddAndPosMainNode(Node groupNode)
        {
            Rect pos = BaseLayout.GetNewGroupNodePosition(_groups);
            groupNode.SetPosition(pos);
            _groups.Add(groupNode);
        }
        internal void UpdateGroupDependencyNodePlacement(GeometryChangedEvent e, GraphBaseGroup baseGroup)
        {

            //((AddressableBaseGroup)baseGroup).mainNode.UnregisterCallback<GeometryChangedEvent, AddressableGraphBaseGroup>(
            //    UpdateGroupDependencyNodePlacement
            //);


            baseGroup.UnregisterCallback(UpdateGroupDependencyNodePlacement);

            ResetNodes(baseGroup);

            Rect pos = BaseLayout.GetNewNodePostion(_addressableGroups);
            //baseGroup.groupNode.SetPosition(pos);
            //baseGroup.mainNode.SetPosition(pos);
            baseGroup.SetPosition(pos);
        }

        private VisualElement CreateInfoWindow()
        {
            var container = new VisualElement();
            container.style.width = 250;
            //container.style.height = 150;
            container.style.backgroundColor = new StyleColor(Color.grey);
            container.style.position = Position.Absolute;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.height = StyleKeyword.Auto;

            var label = new Label("Inspector");
            label.name = "title";
            container.Add(label);

            var info = new Label("Dependence");
            info.name = "info";
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.overflow = Overflow.Visible;
            //info.style.flexShrink = 1; 
            info.style.fontSize = 12;
            info.style.flexShrink = 0;
            info.style.flexGrow = 1;
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.textOverflow = TextOverflow.Clip;
            container.Add(info);

            var showDependencyBtn = new Button();
            showDependencyBtn.text = "Show Reliance";
            showDependencyBtn.name = "ShowReliance";
            showDependencyBtn.clicked += OnClickShowReliance;
            container.Add(showDependencyBtn);


            return container;
        }

        protected void OnClickShowReliance()
        {
            //IGGDebug.LogError("show click!");
            var userData = _infoWindow.Q<Button>("ShowReliance").userData as List<EdgeUserData>;

            if (userData == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(userData[0].Dependence))
            {
                var obj = AssetDatabase.LoadAssetAtPath(userData[0].Dependence, typeof(Object));
                Selection.activeObject = obj;
                GuiManager.FileMenu(null);
            }

        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.75f),
                    paddingLeft = 5,
                    paddingRight = 5
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
                style = { marginLeft = 5, marginRight = 5 }
            });
            toolbar.Add(new Button(ClearGraph)
            {
                text = "Clear",
                style = { marginLeft = 5, marginRight = 5 }
            });

            AlignmentToggle = new Toggle
            {
                text = "Horizontal Layout",
                value = false
            };
            AlignmentToggle.RegisterValueChangedCallback(x =>
            {
                ResetAllNodes();
            });

            //toolbar.Add(AlignmentToggle); 
            var inputField = new TextField
            {
                style = { flexGrow = 1, marginLeft = 5, marginRight = 5, minWidth = 100, maxWidth = 250 }
            };

            var placeholder = new Label("Enter text here...")
            {
                style = { color = new Color(0.5f, 0.5f, 0.5f, 0.75f), position = Position.Absolute }
            };
            inputField.Add(placeholder);

            inputField.RegisterCallback<FocusInEvent>(evt =>
            {
                if (string.IsNullOrEmpty(inputField.value))
                    placeholder.visible = false;
            });

            inputField.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (string.IsNullOrEmpty(inputField.value))
                    placeholder.visible = true;
            });

            inputField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    Debug.Log(inputField.value);
                    HighlightNodeByName(inputField.value);
                    inputField.value = "";
                }
            });

            toolbar.Add(inputField);

            toolbar.Add(new Button(RemoveNode)
            {
                text = "Remove Node",
                style = { marginLeft = 5, marginRight = 5 }
            });

            return toolbar;
        }

        //TODO: consider assembly graph.
        private void RemoveNode()
        {
            if (m_GraphView.selection.Count < 1)
            {
                Debug.LogError("Please select a node");
                return;
            }

            Node node = m_GraphView.selection[0] as Node;

            if (node != null)
            {
                RemoveNode(node);
                return;
            }

            Group group = m_GraphView.selection[0] as Group;

            if (group != null)
            {
                var elements = group.containedElements.ToList();
                while(elements.Count > 0)
                {
                    Node groupNode = elements[0] as Node;
                    if (groupNode != null)
                    {
                        //group.Remove(element);
                        RemoveNode(groupNode);
                    }
                    else
                    {
                        //group.Remove(element);
                        m_GraphView.Remove(elements[0]);
                    }
                    elements.RemoveAt(0);
                }
                m_GraphView.RemoveElement(group);
            }

        }

        private void RemoveNode(Node node)
        {
            if (node != null)
            {
                Port inport = node.inputContainer[0] as Port;
                var ins = inport.connections;
                foreach (var edge in ins)
                {
                    m_GraphView.RemoveElement(edge);
                }

                Port outport = node.outputContainer[0] as Port;
                var outs = outport.connections;
                foreach (var edge in outs)
                {
                    m_GraphView.RemoveElement(edge);
                }

                m_GraphView.RemoveElement(node);
                return;
            }
        }

        protected void ClearGraph()
        {
            SelectedObjects.Clear();

            foreach (var assetGroup in _addressableGroups)
            {
                EmptyGroup(assetGroup);
            }

            m_GUIDNodeLookup.Clear();

            _addressableGroups.Clear();

            _groups.Clear();
        }

        private void AddElements()
        {
            Object[] objs = Selection.objects;
            //AddElements(objs);
        }

        void EmptyGroup(GraphBaseGroup assetGroup)
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


        internal void ResetNodes(GraphBaseGroup assetGroup)
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

            Rect mainNodeRect = assetGroup.GetMainNodePositoin();// assetGroup.mainNode.GetPosition();

            foreach (var node in assetGroup.m_DependenciesForPlacement)
            {
                int depth = (int)node.userData;
                //IGGDebug.Log(node.layout);
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

        private void HighlightNodeByName(string value)
        {
            if (_currentHilightNode != null)
            {
                _currentHilightNode.style.backgroundColor = AddressaableToolKey.DefaultNodeBackgroundColor;
                _currentHilightNode = null;
            }

            foreach (var node in m_GUIDNodeLookup.Values)
            {
                if (node.title.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    //Debug.LogError("find node " + node.title);
                    node.style.backgroundColor = AddressaableToolKey.NodeHilightColor;
                    CenterOnNode(node);
                    _currentHilightNode = node;
                    break;
                }
            }
        }

        private void CenterOnNode(Node node)
        {
            if (node != null && m_GraphView.Contains(node))
            {
                Vector2 nodePosition = node.GetPosition().position;

                Rect graphViewRect = m_GraphView.contentContainer.layout;
                Vector2 graphCenter = new Vector2(graphViewRect.width / 2, graphViewRect.height / 2);

                Vector3 newPosition = graphCenter - nodePosition;

                Vector3 currentScale = m_GraphView.viewTransform.scale;

                m_GraphView.UpdateViewTransform(newPosition, currentScale);
            }
        }

        internal virtual void AddElement(Object asset)
        {

        }
    }

    internal class AddressableGraphView : GraphView
    {
        private GraphWindow _window;

        public AddressableGraphView(GraphWindow graphWindow)
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
            VisualElement background = new VisualElement
            {
                style =
            {
                backgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f)
            }
            };
            Insert(0, background);

            background.StretchToParentSize();

            // Register drag-and-drop callbacks
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            RegisterCallback<MouseUpEvent>(OnMouseUp);

            _window = graphWindow;

        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            //IGGDebug.LogError(" OnMouseUp");
            _window.OnMouseUp(evt);
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();

            _window.AddElements(DragAndDrop.objectReferences);

            evt.StopPropagation();
        }
    }
}