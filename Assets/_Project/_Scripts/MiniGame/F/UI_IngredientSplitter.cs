using UnityEngine;

// บังคับว่าสคริปต์นี้ต้องแปะอยู่คู่กับ UI_Ingredient เสมอ ป้องกันการลืม
[RequireComponent(typeof(UI_Ingredient))]
public class UI_IngredientSplitter : MonoBehaviour
{
    [Header("References")]
    public UI_HalfIngredient halfPrefab;

    [Header("Left Half Settings")]
    public Vector2 leftVelocityMin = new Vector2(-200f, 0f);
    public Vector2 leftVelocityMax = new Vector2(-100f, 200f);
    public float leftRotationMin = 50f;
    public float leftRotationMax = 200f;

    [Header("Right Half Settings")]
    public Vector2 rightVelocityMin = new Vector2(100f, 300f);
    public Vector2 rightVelocityMax = new Vector2(200f, 500f);
    public float rightRotationMin = -200f;
    public float rightRotationMax = -50f;

    private UI_Ingredient _ingredient;

    void Awake()
    {
        // ดึงคอมโพเนนต์ที่อยู่บน GameObject เดียวกัน
        _ingredient = GetComponent<UI_Ingredient>();

        // สมัครรับข่าวสาร: ถ้าโดนฟันเมื่อไหร่ ให้เรียกฟังก์ชัน SpawnHalves
        _ingredient.OnHitDestroySelf += SpawnHalves;
    }

    void OnDestroy()
    {
        // ยกเลิกการติดตามเมื่อตัวมันเองถูกทำลาย (Best Practice)
        if (_ingredient != null)
        {
            _ingredient.OnHitDestroySelf -= SpawnHalves;
        }
    }

    private void SpawnHalves(UI_Ingredient targetIngredient)
    {
        // ดึงข้อมูลภาพและตำแหน่งมาจากตัวที่ส่งมา
        Sprite currentSprite = targetIngredient.IngredientImage.sprite;
        RectTransform targetRect = targetIngredient.Rect;

        // ดึงลำดับ (Sibling Index) ของวัตถุดิบตัวเต็มใน Parent
        int targetIndex = targetRect.GetSiblingIndex();

        // --- 1. สร้างซีกซ้าย ---
        GameObject leftHalf = Instantiate(halfPrefab.gameObject, targetRect.parent);
        RectTransform leftRect = leftHalf.GetComponent<RectTransform>();
        leftRect.anchoredPosition = targetRect.anchoredPosition;

        // บังคับแทรกให้อยู่ในลำดับเดียวกับวัตถุดิบตัวแม่
        leftRect.SetSiblingIndex(targetIndex);

        // สุ่มค่าจากตัวแปรที่เราตั้งไว้ใน Inspector
        Vector2 leftVelocity = new Vector2(
            UnityEngine.Random.Range(leftVelocityMin.x, leftVelocityMax.x),
            UnityEngine.Random.Range(leftVelocityMin.y, leftVelocityMax.y)
        );
        float leftRotation = UnityEngine.Random.Range(leftRotationMin, leftRotationMax);

        leftHalf.GetComponent<UI_HalfIngredient>().Setup(currentSprite, 0, leftVelocity, leftRotation);

        // --- 2. สร้างซีกขวา ---
        GameObject rightHalf = Instantiate(halfPrefab.gameObject, targetRect.parent);
        RectTransform rightRect = rightHalf.GetComponent<RectTransform>();
        rightRect.anchoredPosition = targetRect.anchoredPosition;

        // บังคับแทรกให้อยู่ในลำดับเดียวกับวัตถุดิบตัวแม่เช่นกัน
        // (แทรกทับกันไปเลย เพราะเดี๋ยวตัวแม่ก็จะถูก Destroy ทิ้งในเสี้ยววินาทีต่อมาแล้ว)
        rightRect.SetSiblingIndex(targetIndex);

        // สุ่มค่าจากตัวแปรที่เราตั้งไว้ใน Inspector
        Vector2 rightVelocity = new Vector2(
            UnityEngine.Random.Range(rightVelocityMin.x, rightVelocityMax.x),
            UnityEngine.Random.Range(rightVelocityMin.y, rightVelocityMax.y)
        );
        float rightRotation = UnityEngine.Random.Range(rightRotationMin, rightRotationMax);

        rightHalf.GetComponent<UI_HalfIngredient>().Setup(currentSprite, 1, rightVelocity, rightRotation);
    }
}