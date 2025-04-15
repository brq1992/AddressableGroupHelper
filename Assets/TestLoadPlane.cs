
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TestLoadPlane : MonoBehaviour
{
    public string FighterKey;
    public string BigPlaneKey;
    public Transform PlaneRoot;
    public RawImage RawImage;
    AsyncOperationHandle<GameObject> fighterHandle;
    AsyncOperationHandle<GameObject> bigPlaneHandle;
    private GameObject _plane;
    private GameObject _bigplane;

    public void LoadFighter()
    {
        //Debug.LogError("LoadFighter");
        StartCoroutine(StarLoadFighter());
        //LoadResource("Assets/ikrambandagi/Prefabs/Plane1.prefab");

    }

    public async void LoadResource(string key)
    {
        System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> locations = await Addressables.LoadResourceLocationsAsync(key).Task;

        var a = Addressables.LoadResourceLocations(key); //Debug.LogError(a.DebugName);
        if (a.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var loc in a.Result)
            {
                Debug.Log($"Dependency TransformInternalId: {Addressables.ResourceManager.TransformInternalId(loc)}");
                // loc 是 IResourceLocation
                // loc.Dependencies 里可能包含对应的 AB 的 location
                foreach (var dep in loc.Dependencies)
                {
                    // dep.InternalId 通常包含对应 AB 的路径信息
                    Debug.Log($"Dependency InternalId: {dep.InternalId}");
                   
                }
            }
        }

        if (locations.Count > 0)
        {
            foreach (var location in locations)
            {
                string bundleName = location.InternalId;
                Debug.Log($"[Addressables Monitor] Resource '{key}' belongs to AssetBundle '{bundleName}'");
            }
        }

        Addressables.LoadAssetAsync<GameObject>(key).Completed += SAA;
    }

    private void SAA(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = handle.Result;
            _plane = Instantiate(obj, PlaneRoot);
            //GameObject.Instantiate(_plane.GetComponent<TestRefer>().Plane);
        }
        else
        {
            Debug.LogError("LoadFighter Failed!");
        }
    }

    private IEnumerator StarLoadFighter()
    {
        fighterHandle = Addressables.LoadAsset<GameObject>(FighterKey);
        yield return fighterHandle;

        if (fighterHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = fighterHandle.Result;
            _plane = Instantiate(obj, PlaneRoot);
            //GameObject.Instantiate(_plane.GetComponent<TestRefer>().Plane);
        }
        else
        {
            Debug.LogError("LoadFighter Failed!");
        }
    }

    public void ReleaseFighter()
    {
        GameObject.Destroy(_plane);
        Debug.LogError("ReleaseFighter");
        Addressables.Release(fighterHandle);
    }

    public void LoadBigPlane()
    {
        Debug.LogError("LoadBigPlane");
        StartCoroutine(StarBigPlane());

    }

    public void ReleaseBigPlane()
    {
        Debug.LogError("ReleaseBigPlane");
        GameObject.Destroy(_bigplane);
        Addressables.Release(bigPlaneHandle);
        //Addressables.LoadResourceLocationsAsync
    }

    private IEnumerator StarBigPlane()
    {
        bigPlaneHandle = Addressables.LoadAssetAsync<GameObject>(BigPlaneKey);
        yield return bigPlaneHandle;

        if (bigPlaneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = bigPlaneHandle.Result;
            _bigplane = Instantiate(obj, PlaneRoot);
        }
        else
        {
            Debug.LogError("LoadBigPlane Failed!");
        }
    }


    GameObject _image;
    private AsyncOperationHandle<GameObject> _imageHandle;
    public string PlaneKey;

    public void LoadRawImage()
    {
        Debug.LogError("LoadRawImage");
        StartCoroutine(StartLoadRawImage());
    }


    public void ReleaseRawImage()
    {
        Debug.LogError("ReleaseRawImage");
        GameObject.Destroy(_image);
        Addressables.Release(_imageHandle);
    }

    private IEnumerator StartLoadRawImage()
    {
        _imageHandle = Addressables.LoadAssetAsync<GameObject>(PlaneKey);
        yield return _imageHandle;

        if (_imageHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = _imageHandle.Result;
            _image = Instantiate(obj, PlaneRoot);
        }
        else
        {
            Debug.LogError("LoadBigPlane Failed!");
        }
    }

    public void ReleaseUnityResource()
    {
        Resources.UnloadUnusedAssets();
    }

    public void ReleaseGC()
    {
        GC.Collect();
    }


    void OnDestroy()
    {
        ReleaseBigPlane();
        ReleaseFighter();
        ReleaseRawImage();
    }
}
