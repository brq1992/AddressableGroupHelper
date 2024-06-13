

namespace AddressableAssetTool
{
    internal class GroupShareDataAnalyzerFactory
    {
        internal static GroupShareDataAnalyzer GetAnalyzer(AddressableAssetShareConfig t)
        {
            if(t.CommonAssetbundleGroups.Count != 0)
            {
                return new GroupShareDataWithCommonDataAnalyzer(t);
            }
            return new GroupShareDataNormalAnalyzer(t);
        }
    }
}