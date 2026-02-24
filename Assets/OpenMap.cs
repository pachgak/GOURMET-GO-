using System;
using UnityEngine;

public class OpenMap : MonoBehaviour
{
    public static OpenMap instance;

    [SerializeField] private bool _isOpenMap;

    private UIActivator _uiActivator;

    public Action<bool> OnOpenMapStateChange;

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
    }


    private void OnEnable()
    {
        //PlayerInputActionsManager.instance.OnOpenInventoryInput += HandleOpenInventoryInput;
        //PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;
        //OpenUiManager.instance.OnOpenUiChange += HandleOpenUiChange;
    }

    private void OnDisable()
    {
        //PlayerInputActionsManager.instance.OnOpenInventoryInput -= HandleOpenInventoryInput;
        //PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
        // OpenUiManager.instance.OnOpenUiChange -= HandleOpenUiChange;
    }

    private void HandleEscInput()
    {
        if (_isOpenMap) ChangeStateUi(!_isOpenMap);
    }

    private void HandleOpenInventoryInput()
    {
        ChangeStateUi(!_isOpenMap);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            HandleOpenInventoryInput();
        }
        //การทำงานของ inventory ต่างๆ
    }

    public void ChangeStateUi(bool isState)
    {
        if (_uiActivator.CheckChangeStateUi(_isOpenMap, isState))
        {
            _isOpenMap = isState;
            OnOpenMapStateChange?.Invoke(isState);
        }
    }
}
