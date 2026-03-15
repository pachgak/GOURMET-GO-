using System;
using UnityEngine;

public class miniGameActveUI : MonoBehaviour
{
    public static miniGameActveUI instance;

    [SerializeField] private bool _isOpenMinigame;

    private UIActivator _uiActivator;
    private MiniGameUIManager _miniGameUIManager;

    //public UIActivatorCompack uIActivatorCompack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        _uiActivator = GetComponent<UIActivator>();
        _miniGameUIManager = GetComponent<MiniGameUIManager>();
    }


    private void OnEnable()
    {
        PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;
        //OpenUiManager.instance.OnOpenUiChange += HandleOpenUiChange;

        _miniGameUIManager.OnCloseMiniGame += HandleEscInput;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
        // OpenUiManager.instance.OnOpenUiChange -= HandleOpenUiChange;

        _miniGameUIManager.OnCloseMiniGame -= HandleEscInput;
    }

    //UI Active
    private void HandleEscInput()
    {
        if (_isOpenMinigame) ChangeStateUi(!_isOpenMinigame);
    }

    public void ChangeStateUi(bool isState)
    {
        if (_uiActivator.CheckChangeStateUi(_isOpenMinigame, isState))
        {
            _isOpenMinigame = isState;
        }
    }
}

