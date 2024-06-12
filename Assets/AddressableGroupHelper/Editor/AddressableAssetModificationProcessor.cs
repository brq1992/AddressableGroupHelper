
using System.IO;
using UnityEditor;
using UnityEngine;


namespace AddressableAssetTool
{
    public class AddressableAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        private static AssetMoveResult OnWillMoveAssetTest(string sourcePath, string destinationPath)
        {
            AssetMoveResult assetMoveResult = AssetMoveResult.DidNotMove;
            Debug.LogError("Source path: " + sourcePath + ". Destination path: " + destinationPath + ".");
            if(!sourcePath.EndsWith(AddreaableToolKey.RuleAssetExtension))
            {
                return assetMoveResult;
            }
            var directory = Path.GetDirectoryName(sourcePath);
            Debug.LogError(directory);



            return AssetMoveResult.DidNotMove;
        }
    }
}