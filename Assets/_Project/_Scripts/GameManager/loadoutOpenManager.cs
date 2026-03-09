using System;
using UnityEngine;

[RequireComponent(typeof(UIActivator))]
public class loadoutOpenManager : MonoBehaviour
{
    public static loadoutOpenManager instance;

    [SerializeField] private bool _isOpenLoadout;
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
        PlayerInputActionsManager.instance.OnOpenLoadoutSkillInput += HandleInputDown;
        PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;
        //OpenUiManager.instance.OnOpenUiChange += HandleOpenUiChange;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnOpenLoadoutSkillInput -= HandleInputDown;
        PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
        // OpenUiManager.instance.OnOpenUiChange -= HandleOpenUiChange;
    }

    private void HandleEscInput()
    {
        if (_isOpenLoadout) ChangeStateUi(!_isOpenLoadout);
    }

    private void HandleInputDown()
    {
        ChangeStateUi(!_isOpenLoadout);
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
        Debug.Log($"ChangeStateUi set {isState}");
        if (_uiActivator.CheckChangeStateUi(_isOpenLoadout, isState))
        {
            Debug.Log($"ChangeStateUi : true");
            _isOpenLoadout = isState;
            OnOpenCookingStateChange?.Invoke(isState);


            CancelInvoke(nameof(CanInteractExit));
            if (!_canInteractExit) Invoke(nameof(CanInteractExit), 0.01f);
            else _canInteractExit = false;
        }
        else Debug.Log($"ChangeStateUi : false");

    }

    public void CanInteractExit()
    {
        _canInteractExit = true;
    }

    public void TestEvet()
    {
        Debug.Log($"TestEvet _isOpenCooking : {_isOpenLoadout}");
    }
}
