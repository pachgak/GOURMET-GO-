using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPromptController : MonoBehaviour
{
    // ��˹���� offset �������ö��Ѻ��� Inspector
    public Vector2 offset = new Vector2(10, -10);

    // ��ҧ�ԧ�֧ RectTransform �ͧ Panel
    public Image itemImage;
    public TMP_Text title;
    public TMP_Text description;

    private RectTransform rectTransform;
    private bool _isShowDetail;

    void Awake()
    {
        // �Ѻ RectTransform �ͧ��� Panel ����� script �ӧҹ
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
        // ��˹����˹觢ͧ Panel ����������
        Vector3 newPosition = Input.mousePosition + (Vector3)offset;

        // �ӹǳ�ͺࢵ�ͧ Panel � Screen Space
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float panelWidth = rectTransform.rect.width;
        float panelHeight = rectTransform.rect.height;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // ��Ǩ�ͺ��л�Ѻ���˹� Panel �᡹ X ����ѹ�͡�͡��
        if (newPosition.x + panelWidth / 2 > screenWidth)
        {
            newPosition.x = screenWidth - panelWidth / 2;
        }
        else if (newPosition.x - panelWidth / 2 < 0)
        {
            newPosition.x = panelWidth / 2;
        }

        // ��Ǩ�ͺ��л�Ѻ���˹� Panel �᡹ Y ����ѹ�Թ�ͺ��ҧ
        // ���ѧ����Ѻ�᡹ Y �����ѧ����Թ�ͺ
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