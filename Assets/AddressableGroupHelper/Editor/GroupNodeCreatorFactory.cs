using System;

namespace AddressableAssetTool.Graph
{
    internal class GroupNodeCreatorFactory
    {
        internal static BaseNodeCreator GetCreator(AddressableAssetRule asset)
        {
            if (asset.PackModel == PackMode.PackTogether)
            {
                return new PackTogetherNodeCreator(asset);
            }
            else
                return new PackSeparateNodeCreator(asset);
        }
    }
}