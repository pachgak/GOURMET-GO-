using UnityEngine;
using Inventory.Model;

public class MiniGameRecipeTester : MonoBehaviour
{
    [Header("Testing Data")]
    public CookingRecipeSO recipeToTest;
    public int targetScore = 50;
    public int cookCount = 1; // <--- 1. เพิ่มตัวแปรจำนวนที่ทำ

    [Header("References")]
    private MiniGameUIControler _uiManager;

    private void Start()
    {
        _uiManager = MiniGameFManager.Instance.GetComponent<MiniGameUIControler>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (MiniGameFManager.Instance != null && recipeToTest != null)
            {
                // 2. ส่ง cookCount เข้าไปใน SetupFromRecipe ด้วย
                MiniGameFManager.Instance.SetupFromRecipe(recipeToTest, targetScore, cookCount);

                if (_uiManager != null)
                {
                    _uiManager.OpenMiniGame();
                    Debug.Log($"[Tester] โหลดสูตร {recipeToTest.name} จำนวน {cookCount} ชิ้น!");
                }
                else
                {
                    Debug.LogWarning("[Tester] อย่าลืมลาก MiniGameUIManager มาใส่ในช่องด้วยนะครับ!");
                }
            }
        }
    }
}