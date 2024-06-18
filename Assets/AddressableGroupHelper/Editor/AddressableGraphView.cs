
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AddressableAssetTool.Graph
{
    internal class AddressableGraphView : GraphView
    {
        private AddressableDependenciesGraph _window;

        public AddressableGraphView(AddressableDependenciesGraph graphWindow)
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
            //Debug.LogError(" OnMouseUp");
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