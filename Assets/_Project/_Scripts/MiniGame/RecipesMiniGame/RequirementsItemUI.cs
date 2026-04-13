using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequirementsItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text countRequirementsTxt;

    public void Setup(Sprite itemIcon, int currentHave, int amountNeeded)
    {
        icon.sprite = itemIcon;
        countRequirementsTxt.text = $"{currentHave}/{amountNeeded}";

        // ถ้าของพอ ตัวหนังสือสีเขียว ถ้าไม่พอสีแดง
        if (currentHave >= amountNeeded)
            countRequirementsTxt.color = Color.green;
        else
            countRequirementsTxt.color = Color.red;
    }
}