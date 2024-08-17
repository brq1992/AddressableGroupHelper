using System;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AddressableAssetTool.Graph
{
    internal class GroupNodeCreatorFactory
    {
        internal static BaseNodeCreator GetCreator(AddressableAssetRule asset)
        {
            //if (asset.PackModel == PackMode.PackTogether)
            //{
            //    return new PackTogetherNodeCreator(asset);
            //}
            //else
            //    return new PackSeparateNodeCreator(asset);


            if (asset.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackTogether)
            {

                return new NewPackTogetherNodeCreator(asset);
            }
            else
            {
                return new NewPackSeparateNodeCreator(asset);
            }
        }
    }
}