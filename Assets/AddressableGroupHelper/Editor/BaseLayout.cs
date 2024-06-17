
using System.Collections.Generic;
using UnityEngine;

namespace AddressableAssetTool.Graph
{
    internal class BaseLayout
    {
        private List<AddressableBaseGroup> _addressableGroups;
        private Rect initPosition = new Rect(0, 0, 0, 0);
        private float _distanceBetweenRectangles = AddressaableToolKey.Size.x;

        public BaseLayout(List<AddressableBaseGroup> addressableGroups)
        {
            this._addressableGroups = addressableGroups;
        }

        internal Rect GetNewNodePostion()
        {
            //count -1 is to get last node count
            int n = _addressableGroups.Count - 1;
            float angle = n * Mathf.PI * (3 - Mathf.Sqrt(5));
            float radius = _distanceBetweenRectangles * Mathf.Sqrt(n);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Rect(x, y,0,0);
        }
    }
}