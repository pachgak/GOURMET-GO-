using System;
using UnityEditor.Timeline.Actions;
using UnityEngine;

[RequireComponent(typeof(UIActivator))]
public class CookingManager : MonoBehaviour
{
    public static CookingManager instance;

    [SerializeField] private bool _isOpenCooking;
    [SerializeField] private bool _canInteractExit = false;

    private UIActivator _uiActivator;

    public Action<bool> OnOpenCookingStateChange;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        _uiActivator = GetComponent<UIActivator>();
    }
    private void OnEnable()
    {
        PlayerInputActionsManager.instance.OnInteractInputDown += HandleInteractInputDown;
        PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;
        //OpenUiManager.instance.OnOpenUiChange += HandleOpenUiChange;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnOpenInventoryInput -= HandleInteractInputDown;
        PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
        // OpenUiManager.instance.OnOpenUiChange -= HandleOpenUiChange;
    }

    private void HandleEscInput()
    {
        if (_isOpenCooking) ChangeStateUi(!_isOpenCooking);
    }

    private void HandleInteractInputDown()
    {
        if (_isOpenCooking && _canInteractExit) ChangeStateUi(!_isOpenCooking);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //การทำงานของ inventory ต่างๆ
    }

    public void ChangeStateUi(bool isState)
    {
        if (_uiActivator.CheckChangeStateUi(_isOpenCooking, isState))
        {
            _isOpenCooking = isState;
            OnOpenCookingStateChange?.Invoke(isState);


            CancelInvoke(nameof(CanInteractExit));
            if (!_canInteractExit) Invoke(nameof(CanInteractExit), 0.01f);
            else _canInteractExit = false;
        }

        
    }

    public void CanInteractExit()
    {
        _canInteractExit = true;
    }
}

