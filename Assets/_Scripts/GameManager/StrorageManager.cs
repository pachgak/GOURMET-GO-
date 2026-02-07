using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(UIActivator))]
public class StrorageManager : MonoBehaviour
{
    public static StrorageManager instance;

    [SerializeField] private bool _isOpenStrorage;
    [SerializeField] private bool _canInteractExit = false;

    private UIActivator _uiActivator;

    public Action<bool> OnOpenStrorageStateChange;

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
        if (PlayerInputActionsManager.instance) Debug.LogWarning("WTF");
        PlayerInputActionsManager.instance.OnInteractInputDown += HandleInteractInputDown;
        PlayerInputActionsManager.instance.OnEscInput += HandleEscInput;
        //OpenUiManager.instance.OnOpenUiChange += HandleOpenUiChange;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnInteractInputDown -= HandleInteractInputDown;
        PlayerInputActionsManager.instance.OnEscInput -= HandleEscInput;
        // OpenUiManager.instance.OnOpenUiChange -= HandleOpenUiChange;
    }

    private void HandleEscInput()
    {

        if (_isOpenStrorage) ChangeStateUi(!_isOpenStrorage);
    }

    private void HandleInteractInputDown()
    {

        if (_isOpenStrorage && _canInteractExit) ChangeStateUi(!_isOpenStrorage);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //การทำงานของ inventory ต่างๆ
    }

    // แก้ไขฟังก์ชันนี้: แยก Logic การเปลี่ยนสถานะออกมา
    public void ChangeStateUi(bool isState)
    {
        // ถ้าสั่ง "ปิด" (false) ให้รอ 1 เฟรมก่อน เพื่อกัน InteractByPoint ทำงานซ้อน
        if (isState == false)
        {
            StartCoroutine(ChangeStateUiRoutine(false));
        }
        else
        {
            // ถ้าสั่ง "เปิด" (true) ให้ทำทันที
            ApplyChangeStateUi(true);
        }
    }

    // Coroutine สำหรับหน่วงเวลาปิด
    private IEnumerator ChangeStateUiRoutine(bool isState)
    {
        yield return null; // รอจนจบเฟรม หรือขึ้นเฟรมใหม่
        ApplyChangeStateUi(isState);
    }

    // ย้าย Logic เดิมมาไว้ที่นี่
    private void ApplyChangeStateUi(bool isState)
    {
        Debug.Log($"ChangeStateUi set {isState}");
        if (_uiActivator.CheckChangeStateUi(_isOpenStrorage, isState))
        {
            Debug.Log($"ChangeStateUi : true");
            _isOpenStrorage = isState;
            OnOpenStrorageStateChange?.Invoke(isState);

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
        Debug.Log($"TestEvet _isOpenStrorage : {_isOpenStrorage}");
    }
}