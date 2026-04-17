using UnityEngine;
using UnityEngine.UI;

public class IngredientsMenuItemUI : MonoBehaviour
{
    public Image ingredientsMenuImage;

    // 1. ช่องนั้นมีไอเทม ใส่รูป ingredient นั้น
    public void Setup(Sprite itemSprite)
    {
        ingredientsMenuImage.sprite = itemSprite;
        ingredientsMenuImage.gameObject.SetActive(true);
    }

    // 2. ช่องเปล่า ปิด ingredientsMenuImage
    public void SetupEmpty()
    {
        ingredientsMenuImage.gameObject.SetActive(false);
    }
}