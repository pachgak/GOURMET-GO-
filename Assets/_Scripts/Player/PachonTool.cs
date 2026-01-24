using System;
using System.Collections;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour { }

public static class PachonTool
{
    private static CoroutineHelper _helper;

    private static CoroutineHelper Helper
    {
        get
        {
            if (_helper == null)
            {
                GameObject go = new GameObject("CoroutineHelper");
                _helper = go.AddComponent<CoroutineHelper>();
            }
            return _helper;
        }
    }

    public static void WaitForInstance<T>(Func<T> instanceGetter, Action<T> onReady) where T : class
    {
        Helper.StartCoroutine(DoWaitForInstance(instanceGetter, onReady));
    }

    private static IEnumerator DoWaitForInstance<T>(Func<T> instanceGetter, Action<T> onReady) where T : class
    {
        T instance = null;

        while ((instance = instanceGetter()) == null)
        {
            yield return null;
        }

        onReady(instance);
    }
}