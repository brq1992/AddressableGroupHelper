

using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace AddressableAssetTool.Graph
{
    internal class AddressableBaseGroupFactory
    {
        internal static GraphBaseGroup GetGroup(UnityEngine.Object obj, AddressableDependenciesGraph addressableDependenciesGraph, bool dataReady)
        {
            if (!(obj is AddressableAssetRule rule))
                return null;
            if (rule.AddEntryByFolder)
                return new DependencyPackFolderGroup(rule, addressableDependenciesGraph);
            if (rule.PackModel == BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
                return new DependencyPackSeparateGroup(obj, addressableDependenciesGraph);
            else
                return new DependencyPackTogetherGroup(obj, addressableDependenciesGraph);
        }
    }
}