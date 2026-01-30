using UnityEngine;

public class MouseFollowerLoadoutSkillUI : MonoBehaviour
{
    //[SerializeField]
    public Canvas canvas;

    //[SerializeField]
    private UILoadoutSkillItem item;

    public void Awake()
    {
        canvas = transform.parent.GetComponent<Canvas>();
        item = GetComponentInChildren<UILoadoutSkillItem>();
    }

    public void SetData(Sprite skillSprite, int expPoint)
    {
        item.SetData(skillSprite, expPoint);
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