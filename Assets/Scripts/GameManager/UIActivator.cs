using UnityEngine;

public class UIActivator : MonoBehaviour
{
    [SerializeField] private bool _isUiOpening;

    public GameObject[] enabledOpenUi;
    public GameObject[] disabledOpenUi;

    private void OnEnable()
    {
        OpenUiManager.instance.OnUiOpeningStateChange += HandleUiOpeningStateChange;
    }

    private void OnDisable()
    {
        OpenUiManager.instance.OnUiOpeningStateChange -= HandleUiOpeningStateChange;
    }

    private void HandleUiOpeningStateChange(bool isUiOpeningState)
    {
        _isUiOpening = isUiOpeningState;
    }

    private void Start()
    {
        ChangeStateUi(false);
    }

    public bool CheckChangeStateUi(bool currentState, bool isState)
    {
        if (!_isUiOpening || currentState)
        {
            ChangeStateUi(isState);

            return true;
        }
        else
        {
            return false;
        }
    }

    private void ChangeStateUi(bool isState)
    {

        foreach (var ui in enabledOpenUi)
        {
            ui.SetActive(isState);
        }
        foreach (var ui in disabledOpenUi)
        {
            ui.SetActive(!isState);
        }

        OpenUiManager.instance.OpenUiChange(gameObject, isState);
    }
}
