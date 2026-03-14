using Inventory.Model;
using UnityEngine;

public class MiniGameRecipeTester : MonoBehaviour
{
    [Header("Testing Data")]
    public CookingRecipeSO recipeToTest;
    public int targetScore = 50;
    public int cookCount = 1;

    [Header("References")]
    // ลาก Manager ของเกมไหนก็ได้มาใส่ตรงนี้ (ลากเกมฟันก็ได้ ต้มก็ได้)
    public MiniGameBase targetMiniGame;

    [ContextMenu("LetCook")]
    public void LetCook()
    {
        if (targetMiniGame != null && recipeToTest != null)
        {
            // ส่งค่าให้ Base Class จัดการ
            targetMiniGame.SetupFromRecipe(recipeToTest, targetScore, cookCount);

            // ให้ UI เตรียมตัว และโยน targetMiniGame ไปให้ UI รับช่วงต่อ
            if (MiniGameUIManager.Instance != null)
            {
                MiniGameUIManager.Instance.OpenMiniGame(targetMiniGame);
            }
            else
            {
                Debug.LogError("หา MiniGameUIManager ในฉากไม่เจอ! อย่าลืมเอาไปแปะไว้ในฉากนะ");
            }
        }
    }
}