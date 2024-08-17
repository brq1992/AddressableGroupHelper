

using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AddressableAssetTool.Graph
{
    internal class AddressableBaseGroupFactory
    {
        internal static GraphBaseGroup GetGroup(UnityEngine.Object obj, AddressableDependenciesGraph addressableDependenciesGraph, bool dataReady)
        {
            if (!(obj is AddressableAssetRule rule))
                return null;
            if(dataReady)
            {
                if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
                    return new AddressableAssetRuleGroup(obj, addressableDependenciesGraph);
                else
                    return new AddressableHoleGroup(obj, addressableDependenciesGraph);
            }
            if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
                return new AddressableShowSingleGroup(obj, addressableDependenciesGraph);
            else
                return new AddressablePackTogetherGroup(obj, addressableDependenciesGraph);
        }
    }
}