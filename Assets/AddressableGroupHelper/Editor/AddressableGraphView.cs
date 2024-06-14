using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AddressableAssetTool.Graph
{
    internal class AddressableGraphView : GraphView
    {
        public AddressableGraphView()
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
        }
    }
}