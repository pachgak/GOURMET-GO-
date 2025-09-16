using System;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class OpenUiManager : MonoBehaviour
{
    public static OpenUiManager instance;

    [SerializeField] private bool _isUiOpening;
    [SerializeField] private GameObject _uiOpening;

    public Action<GameObject,bool> OnOpenUiChange;
    public Action<bool> OnUiOpeningStateChange;

    private void OnEnable()
    {
        //OnOpenUiChange += HandleOpenUiChange;
    }


    private void OnDisable()
    {
        //OnOpenUiChange -= HandleOpenUiChange;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void OpenUiChange(GameObject UiChange,bool isOpen)
    {

        if (isOpen)
        {
            _isUiOpening = true;
            OnUiOpeningStateChange?.Invoke(_isUiOpening);
            _uiOpening = UiChange;
        }
        else if (!isOpen && UiChange == _uiOpening)
        {
            _isUiOpening = false;
            OnUiOpeningStateChange?.Invoke(_isUiOpening);
            _uiOpening = null;
        }


        //StartCoroutine(WaitForAllUiChecking(UiChange, isOpen));
    }

    private System.Collections.IEnumerator WaitForAllUiChecking(GameObject UiChange, bool isOpen)
    {
        yield return null;

        if (isOpen)
        {
            _isUiOpening = true;
            OnUiOpeningStateChange?.Invoke(_isUiOpening);
            _uiOpening = UiChange;
        }

        else if (!isOpen && UiChange == _uiOpening)
        {
            _isUiOpening = false;
            OnUiOpeningStateChange?.Invoke(_isUiOpening);
            _uiOpening = null;
        }
    }
}
