
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AddressableAssetTool.Graph
{
    internal class BaseLayout
    {
        static internal Rect GetNewNodePostion(List<AddressableGraphBaseGroup> addressableGroups)
        {
            //count -1 is to get last node count
            int n = addressableGroups.Count - 1;
            float angle = n * Mathf.PI * (3 - Mathf.Sqrt(5));
            float radius = AddressaableToolKey.NodeRadius * Mathf.Sqrt(n);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Rect(x, y,0,0);
        }

        static internal Rect GetNewNodePostion(int currentCount)
        {
            int n = currentCount;
            float angle = n * Mathf.PI * (3 - Mathf.Sqrt(5));
            float radius = AddressaableToolKey.NodeRadius * Mathf.Sqrt(n);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Rect(x, y, 0, 0);
        }

        internal static Rect GetNewGroupNodePosition(List<Group> groups)
        {
            int n = groups.Count;
            float angle = n * Mathf.PI * (3 - Mathf.Sqrt(5));
            float radius = AddressaableToolKey.GroupRadius * Mathf.Sqrt(n);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Rect(x, y, 0, 0);
        }
    }
}