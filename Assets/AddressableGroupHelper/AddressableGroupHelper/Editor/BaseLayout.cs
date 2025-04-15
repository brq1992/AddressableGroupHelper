
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AddressableAssetTool.Graph
{
    internal class BaseLayout
    {
        static internal Rect GetNewNodePostion(List<GraphBaseGroup> addressableGroups)
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

        internal static Rect GetNewGroupNodePosition(int Count)
        {
            float angle = Count * Mathf.PI * (3 - Mathf.Sqrt(5));
            float radius = AddressaableToolKey.GroupRadius * Mathf.Sqrt(Count);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Rect(x, y, 0, 0);
        }

        internal static Rect GetNewGroupNodePosition(List<Node> groups)
        {
            int n = groups.Count;
            //IGGDebug.LogError("n " + n);
            float angle = n * Mathf.PI * (3 - Mathf.Sqrt(5));
            float radius = AddressaableToolKey.GroupRadius * Mathf.Sqrt(n);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Rect(x, y, 0, 0);
        }
    }
}