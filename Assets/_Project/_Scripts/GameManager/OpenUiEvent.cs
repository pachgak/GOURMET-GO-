using System;
using UnityEngine;

public class OpenUiEvent : MonoBehaviour
{
    public GameObject[] enabledOpenUi;
    public GameObject[] disabledOpenUi;

    private bool _isOpenPot = false;

    public Action<bool> OnOpenPotStateChange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (_isOpenPot && Input.GetKeyDown(KeyCode.E))
        //{
        //    ChangeStateUi(false);
        //}
    }

    public void PotSwich()
    {
        ChangeStateUi(!_isOpenPot);
    }

    public void ChangeStateUi(bool isState)
    {
        _isOpenPot = isState;
        OnOpenPotStateChange?.Invoke(isState);

        //SetUi
        foreach (var ui in enabledOpenUi)
        {
            ui.SetActive(isState);
        }
        foreach (var ui in disabledOpenUi)
        {
            ui.SetActive(!isState);
        }
    }
}
