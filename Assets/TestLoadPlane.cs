
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
        Debug.LogError("LoadFighter");
        StartCoroutine(StarLoadFighter());

    }

    private IEnumerator StarLoadFighter()
    {
        fighterHandle = Addressables.LoadAssetAsync<GameObject>(FighterKey);
        yield return fighterHandle;

        if (fighterHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = fighterHandle.Result;
            _plane = Instantiate(obj, PlaneRoot);
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
