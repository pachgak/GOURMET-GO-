using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoadAndLoadForPlayerControlCS : MonoBehaviour
{
    public InputActionAsset actions;

    public void OnEnable()
    {
        //Debug.Log("RebindSaveLoad : LoadBindingOverridesFromJson");
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }

    public void OnDisable()
    {
        //Debug.Log("RebindSaveLoad : SaveBindingOverridesAsJson");
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);

        PlayerInputActionsManager.instance.LoadBindingToPlayerContrlorsCS();
    }
}