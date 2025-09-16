using System;
using UnityEngine;

[RequireComponent(typeof(UIActivator))]
public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [SerializeField] private bool _isOpenMenu;

    private UIActivator _uiActivator;

    public Action<bool> OnOpenMenuStateChange;
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
        PlayerInputActionsManager.instance.OnOpenMenuInput += HandleOpenMenuInput;
        PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;

        //OpenUiManager.instance.OnOpenUiChange += HandleOpenUiChange;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnOpenMenuInput -= HandleOpenMenuInput;
        PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
    }

    private void HandleEscInput()
    {
        //if (_isOpenMenu) ChangeStateUi(!_isOpenMenu);
    }

    private void HandleOpenMenuInput()
    {
        ChangeStateUi(!_isOpenMenu);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //การทำงานของ menu ต่างๆ
    }

    public void ChangeStateUi(bool isState)
    {
        if (_uiActivator.CheckChangeStateUi(_isOpenMenu, isState))
        {
            _isOpenMenu = isState;
            OnOpenMenuStateChange?.Invoke(isState);
        }
    }
}
