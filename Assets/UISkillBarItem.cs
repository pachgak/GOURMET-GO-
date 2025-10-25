using Inventory.UI;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISkillBarItem : MonoBehaviour , IPointerClickHandler,
        IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image borderImage;
    [SerializeField]
    private Image skillImage;
    [SerializeField]
    private GameObject uesCountUI;
    private TMP_Text _uesdCountTxt;
    [SerializeField]
    private Image typeImage;
    [SerializeField]
    private Slider cooldowBar;
    [SerializeField]
    private TMP_Text cooldowText;
    [SerializeField]
    private GameObject keyUI;
    private TMP_Text _keyTxt;

    private CanvasGroup _canvasGroup;

    private bool empty;


    public event Action<UISkillBarItem> OnItemClicked,
        OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag,
        OnRightMouseBtnClick,
        OnPointEnterItem, OnPointExitItem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _uesdCountTxt = uesCountUI.GetComponentInChildren<TMP_Text>();
        _keyTxt = keyUI.GetComponentInChildren<TMP_Text>();


        ResetData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetData()
    {
        if (skillImage != null) skillImage.gameObject.SetActive(false);

        empty = true;
    }

    public void SetData(Sprite skillSprite, int uesdCount , Sprite typeSprite)
    {
        skillImage.gameObject.SetActive(true);

        skillImage.sprite = skillSprite;
        typeImage.sprite = typeSprite;

        _uesdCountTxt.text = $"{uesdCount}";
        //_keyTxt.text = keyText;
        empty = false;

        cooldowBar.maxValue = 0;
        cooldowBar.value = 0;
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

    public void OnPointerClick(PointerEventData pointerData)
    {
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
        if (empty)
            return;
        OnItemBeginDrag?.Invoke(this);
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        OnItemEndDrag?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedOn?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointEnterItem?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointExitItem?.Invoke(this);
    }
}
