using System;
using UnityEngine;

[RequireComponent(typeof(UIActivator))]
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [SerializeField] private bool _isOpenInventory;

    private UIActivator _uiActivator;

    public Action<bool> OnOpenInventoryStateChange;

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
        PlayerInputActionsManager.instance.OnOpenInventoryInput += HandleOpenInventoryInput;
        PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;
        //OpenUiManager.instance.OnOpenUiChange += HandleOpenUiChange;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnOpenInventoryInput -= HandleOpenInventoryInput;
        PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
        // OpenUiManager.instance.OnOpenUiChange -= HandleOpenUiChange;
    }

    private void HandleEscInput()
    {
        if(_isOpenInventory) ChangeStateUi(!_isOpenInventory);
    }

    private void HandleOpenInventoryInput()
    {
        ChangeStateUi(!_isOpenInventory);
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
        if (_uiActivator.CheckChangeStateUi(_isOpenInventory, isState))
        {
            _isOpenInventory = isState;
            OnOpenInventoryStateChange?.Invoke(isState);
        }
    }
}
