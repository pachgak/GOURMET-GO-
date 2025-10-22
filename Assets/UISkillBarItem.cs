using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISkillBarItem : MonoBehaviour
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
    private GameObject keyUI;
    private TMP_Text _keyTxt;

    private CanvasGroup _canvasGroup;

    private bool empty;
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

    public void SetData(Sprite skillSprite, int uesdCount , Sprite typeSprite , string keyText)
    {
        skillImage.gameObject.SetActive(true);

        skillImage.sprite = skillSprite;
        typeImage.sprite = typeSprite;

        _uesdCountTxt.text = $"{uesdCount}";
        //_keyTxt.text = keyText;
        empty = false;
    }

    public void ShowCurrentlyDragged()
    {
        _canvasGroup.alpha = 0.6f;
    }
    public void DeShowCurrentlyDragged()
    {
        _canvasGroup.alpha = 1f;
    }
}
