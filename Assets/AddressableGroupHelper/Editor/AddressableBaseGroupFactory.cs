using System;

namespace AddressableAssetTool.Graph
{
    internal class AddressableBaseGroupFactory
    {
        internal static AddressableBaseGroup GetGroup(UnityEngine.Object obj, AddressableDependenciesGraph addressableDependenciesGraph)
        {
            if(obj is AddressableAssetRule)
            {
                return new AddressableHoleGroup(obj, addressableDependenciesGraph);
                return new AddressableAssetRuleGroup(obj, addressableDependenciesGraph);
            }
            return new AddressableBaseGroup(obj, addressableDependenciesGraph);
        }
    }
}