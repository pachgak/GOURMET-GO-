using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILoadoutSkillItem : MonoBehaviour, IPointerClickHandler,
        IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image borderImage;
    [SerializeField]
    private Image skillImage;
    [SerializeField]
    private Image lockSkillImage;
    [SerializeField]
    private TMP_Text expText;

    [SerializeField]
    private Slider cooldowBar;
    [SerializeField]
    private TMP_Text cooldowText;

    private CanvasGroup _canvasGroup;

    [SerializeField]
    private bool lockSkill;


    public event Action<UILoadoutSkillItem> OnItemClicked,
        OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag,
        OnRightMouseBtnClick,
        OnPointEnterItem, OnPointExitItem;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (skillImage.sprite == null) ResetData();
        CooldownUpdate(0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetData()
    {
        lockSkill = true;

        skillImage.gameObject.SetActive(false);
        lockSkillImage.gameObject.SetActive(true);

    }

    public void SetData(Sprite skillSprite, int expPoint)
    {
        //skillImage.gameObject.SetActive(true);

        skillImage.sprite = skillSprite;
        lockSkillImage.sprite = skillSprite;

        expText.text = $"{expPoint}";
        expText.gameObject.SetActive(false);

        if (expPoint > 0)
        {
            lockSkill = false;
            skillImage.gameObject.SetActive(true);
            lockSkillImage.gameObject.SetActive(false);
        }
    }

    public void Select()
    {
        borderImage.enabled = true;
    }

    public void Deselect()
    {
        if (borderImage != null) borderImage.enabled = false;
    }

    public void ShowCurrentlyDragged()
    {
        _canvasGroup.alpha = 0.6f;
    }

    public void DeShowCurrentlyDragged()
    {
        _canvasGroup.alpha = 1f;
    }

    public void CooldownUpdate(float countdown)
    {
        cooldowBar.value = countdown;
        cooldowText.text = ((Mathf.Floor(countdown * 10f)) / 10f).ToString();
        if (cooldowBar.value <= cooldowBar.minValue) cooldowText.enabled = false;
        else cooldowText.enabled = true;
    }

    //Dang and Drop 

    public void OnPointerClick(PointerEventData pointerData)
    {
        if (lockSkill)
            return;
        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            OnRightMouseBtnClick?.Invoke(this);
        }
        else
        {
            OnItemClicked?.Invoke(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Skill OnBeginDrag");
        if (lockSkill)
            return;
        OnItemBeginDrag?.Invoke(this);
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (lockSkill)
            return;
        OnItemEndDrag?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (lockSkill)
            return;
        OnItemDroppedOn?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (lockSkill)
            return;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (lockSkill)
            return;
        OnPointEnterItem?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (lockSkill)
            return;
        OnPointExitItem?.Invoke(this);
    }
}
