
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ResourceManagement.ResourceManager;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Text.RegularExpressions;

public class AddressableDiagnol : MonoBehaviour
{
    private HashSet<string> loadedBundleNames = new HashSet<string>();
    void OnEnable()
    {
        Addressables.ResourceManager.RegisterDiagnosticCallback(OnDiagnosticEvent);
    }

    //private void OnDiagnosticEvent(DiagnosticEventContext diagnosticEventContext)
    //{
    //    if (diagnosticEventContext.Type == DiagnosticEventType.AsyncOperationCreate)
    //    {

    //        int a = 0;
    //    }
    //}

    private void OnDiagnosticEvent(AsyncOperationHandle handle, DiagnosticEventType type, int arg3, object arg4)
    {
        Debug.Log("Registering diagnostic callback...");
        if (type == DiagnosticEventType.AsyncOperationCreate)
        {
            if (TryExtractBundle(handle.DebugName, out var result))
            {
                Debug.LogError(result);
            }
        }

        if (type == DiagnosticEventType.AsyncOperationComplete)
        {
            int a = 0;
        }

        if (type == DiagnosticEventType.AsyncOperationReferenceCount)
        {
            if (TryExtractBundle(handle.DebugName, out var result))
            {
                Debug.LogError(result);
            }
        }

        if (type == DiagnosticEventType.AsyncOperationDestroy)
        {
            if (TryExtractBundle(handle.DebugName, out var result))
            {
                Debug.LogError(result);
            }
        }
    }

    static bool TryExtractBundle(string input, out string extractedValue)
    {
        string pattern = @"\b[a-zA-Z0-9_]+\.bundle\b";
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            extractedValue = match.Value; 
            return true;
        }

        extractedValue = null;
        return false;
    }

    //void OnDisable()
    //{
    //    Addressables.ResourceManager.UnregisterDiagnosticCallback(OnDiagnosticEvent);
    //}
}
