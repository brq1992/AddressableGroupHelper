
using System.Collections.Generic;
using UnityEditor;

namespace com.igg.editor
{
    public class Utilities 
    {
        private static readonly List<string> _ignoreTypeList = new List<string> { ".tpsheet", ".cginc", ".cs", ".dll" };

        public static bool IsValidFolder(string pPath)
        {
            if (AssetDatabase.IsValidFolder(pPath))
            {
                return true;
            }
            return false;
        }

        public static bool IsValidPath(string pPath)
        {
            foreach (string str in _ignoreTypeList)
            {
                if (pPath.Contains(str))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
