

namespace AddressableAssetTool.Graph
{
    internal class AddressableBaseGroupFactory
    {
        internal static GraphBaseGroup GetGroup(UnityEngine.Object obj, AddressableDependenciesGraph addressableDependenciesGraph)
        {
            if (obj is AddressableAssetRule rule)
            {
                if (rule.PackModel == PackMode.PackSeparately)
                    return new AddressableShowSingleGroup(obj, addressableDependenciesGraph);
                else
                    return new AddressablePackTogetherGroup(obj, addressableDependenciesGraph);
            }
            return new AddressableAssetGroup(obj, addressableDependenciesGraph);
        }
    }
}