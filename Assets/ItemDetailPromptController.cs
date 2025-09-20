using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPromptController : MonoBehaviour
{
    // กำหนดค่า offset ที่สามารถปรับได้ใน Inspector
    public Vector2 offset = new Vector2(10, -10);

    // อ้างอิงถึง RectTransform ของ Panel
    public Image itemImage;
    public TMP_Text title;
    public TMP_Text description;

    private RectTransform rectTransform;
    private bool _isShowDetail;

    void Awake()
    {
        // รับ RectTransform ของตัว Panel เมื่อ script ทำงาน
        Toggle(false);
        rectTransform = GetComponent<RectTransform>();
        ResetDescription();
    }

    void Update()
    {
        DetailMove();
    }

    public void DetailMove()
    {
        // กำหนดตำแหน่งของ Panel ให้ตามเมาส์
        Vector3 newPosition = Input.mousePosition + (Vector3)offset;

        // คำนวณขอบเขตของ Panel ใน Screen Space
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float panelWidth = rectTransform.rect.width;
        float panelHeight = rectTransform.rect.height;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // ตรวจสอบและปรับตำแหน่ง Panel ในแกน X ถ้ามันออกนอกจอ
        if (newPosition.x + panelWidth / 2 > screenWidth)
        {
            newPosition.x = screenWidth - panelWidth / 2;
        }
        else if (newPosition.x - panelWidth / 2 < 0)
        {
            newPosition.x = panelWidth / 2;
        }

        // ตรวจสอบและปรับตำแหน่ง Panel ในแกน Y ถ้ามันเกินขอบล่าง
        // แต่ยังให้ขยับในแกน Y ได้ถ้ายังไม่เกินขอบ
        if (newPosition.y - panelHeight / 2 < 0)
        {
            newPosition.y = panelHeight / 2;
        }

        transform.position = newPosition;
    }

    public void ResetDescription()
    {
        itemImage.gameObject.SetActive(false);
        title.text = "";
        description.text = "";
    }

    public void SetDescription(Sprite sprite, string itemName, string itemDescription)
    {
        DetailMove();

        itemImage.gameObject.SetActive(true);
        itemImage.sprite = sprite;
        title.text = itemName;
        description.text = itemDescription;
    }

    public void Toggle(bool val)
    {
        Debug.Log($"{this.name} {val}");
        gameObject.SetActive(val);
    }
}