using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollowerSkillUI : MonoBehaviour
{
    //[SerializeField]
    public Canvas canvas;

    //[SerializeField]
    private UISkillBarItem item;

    public void Awake()
    {
        canvas = transform.parent.GetComponent<Canvas>();
        item = GetComponentInChildren<UISkillBarItem>();
    }

    public void SetData(Sprite skillSprite, int uesdCount, Sprite typeSprite, float countdown, float maxCooldown)
    {
        item.SetData(skillSprite, uesdCount, typeSprite, countdown, maxCooldown);
    }
    void Update()
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            Input.mousePosition,
            canvas.worldCamera,
            out position
                );
        transform.position = canvas.transform.TransformPoint(position);
    }
    
    public void Toggle(bool val)
    {
        gameObject.SetActive(val);
    }
}
