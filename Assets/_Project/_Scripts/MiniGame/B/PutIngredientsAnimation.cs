using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inventory.Model;

public class PutIngredientsAnimation : MonoBehaviour
{

    [Tooltip("Prefab ของวัตถุดิบ (ต้องมี Image Component)")]
    public GameObject ingredientUIPrefab;

    [Tooltip("จุดที่วัตถุดิบเกิด (เช่น ด้านบนขอบจอ)")]
    public RectTransform dropStartPoint;

    [Tooltip("จุดที่วัตถุดิบตกลงไป (เช่น กลางหม้อต้ม)")]
    public RectTransform potTargetPoint;

    [Tooltip("Parent ที่ต้องการให้วัตถุดิบไปโผล่ (ควรเป็น RectTransform ใน UI Canvas)")]
    public RectTransform spawnParent; // <--- เพิ่มตัวแปรตรงนี้

    [Header("Animation Settings")]
    public float dropDuration = 0.6f;     // เวลาในการตก
    public float delayBetweenDrops = 0.15f; // ระยะเวลาห่างระหว่างการโยนแต่ละชิ้น
    public float randomSpreadX = 50f;     // ความกว้างในการกระจายซ้ายขวาตอนโยน

    [Header("References")]
    private MiniGameBManager _miniGameBManager;

    private void Awake()
    {
        _miniGameBManager = GetComponent<MiniGameBManager>();
    }

    private void OnEnable()
    {
        if (_miniGameBManager != null)
        {
            _miniGameBManager.OnRecipeSetup += HandleRecipeSetup;
        }
    }

    private void OnDisable()
    {
        if (_miniGameBManager != null)
        {
            _miniGameBManager.OnRecipeSetup -= HandleRecipeSetup;
        }
    }

    private void HandleRecipeSetup(CookingRecipeSO recipe)
    {
        if (recipe == null) return;
        if (recipe.ingredients == null || recipe.ingredients.Count == 0) return;

        // เช็คเผื่อลืมลากใส่ (รวม spawnParent ด้วย)
        if (ingredientUIPrefab == null || dropStartPoint == null || potTargetPoint == null || spawnParent == null) return;

        // เริ่ม Coroutine ทยอยโยนวัตถุดิบ
        StartCoroutine(DropIngredientsRoutine(recipe.ingredients));
    }

    private IEnumerator DropIngredientsRoutine(List<ItemQuantity> ingredients)
    {
        // หน่วงเวลานิดนึงรอให้ UI เปิดเสร็จก่อน
        yield return new WaitForSeconds(0.2f);

        foreach (var itemQty in ingredients)
        {
            if (itemQty.item == null || itemQty.item.ItemImage == null) continue;

            for (int i = 0; i < itemQty.quantity; i++)
            {
                DropSingleIngredient(itemQty.item.ItemImage);

                // รอแป๊บนึงก่อนโยนชิ้นต่อไป
                yield return new WaitForSeconds(delayBetweenDrops);
            }
        }
    }

    private void DropSingleIngredient(Sprite ingredientSprite)
    {
        // 1. ดึง Object จาก Pool และให้ไปอยู่ใต้ spawnParent
        GameObject ingObj = ObjectPoolingManager.Instance.Spawn(ingredientUIPrefab, spawnParent);
        RectTransform ingRect = ingObj.GetComponent<RectTransform>();
        Image ingImage = ingObj.GetComponent<Image>();

        // 2. รีเซ็ตสถานะ
        ingRect.DOKill();
        ingImage.DOKill();
        ingRect.localScale = Vector3.zero; // เริ่มจากมองไม่เห็น
        ingImage.color = Color.white;
        ingImage.sprite = ingredientSprite;

        // สุ่มจุดเกิดซ้าย-ขวา เล็กน้อย เพื่อไม่ให้มันตกมาทับกันเป๊ะๆ
        float randomOffsetX = Random.Range(-randomSpreadX, randomSpreadX);
        ingRect.position = dropStartPoint.position;
        ingRect.anchoredPosition += new Vector2(randomOffsetX, 0);

        // สุ่มมุมหมุนเริ่มต้น
        ingRect.localRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));

        // 3. เริ่มทำ Animation ด้วย DOTween (Sequence)
        Sequence dropSeq = DOTween.Sequence();

        // 3.1 Pop เด้งขยายตัวขึ้นมาที่จุดเกิด
        dropSeq.Append(ingRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack));

        // 3.2 ตกลงไปที่หม้อ (แกน Y) พร้อมกับค่อยๆ ขยับแกน X ไปตรงกลางหม้อ
        dropSeq.Append(ingRect.DOMoveY(potTargetPoint.position.y, dropDuration).SetEase(Ease.InQuad));
        dropSeq.Join(ingRect.DOMoveX(potTargetPoint.position.x, dropDuration).SetEase(Ease.InOutSine));

        // 3.3 หมุนติ้วๆ ระหว่างตก
        dropSeq.Join(ingRect.DORotate(new Vector3(0, 0, Random.Range(90f, 180f)), dropDuration, RotateMode.LocalAxisAdd));

        // 3.4 หดตัวเล็กลงเหมือนจมลงไปในน้ำตอนถึงเป้าหมาย
        dropSeq.Append(ingRect.DOScale(0f, 0.15f).SetEase(Ease.InBack));

        // 4. เมื่อ Animation จบ ให้คืน Object กลับเข้า Pool
        dropSeq.OnComplete(() =>
        {
            ObjectPoolingManager.Instance.Respawn(ingObj);
        });
    }

    private void OnDrawGizmosSelected()
    {
        // เช็คว่ามีการใส่จุด dropStartPoint ไว้หรือยัง
        if (dropStartPoint != null)
        {
            // ตั้งสีของเส้น Gizmos (ใช้สีฟ้า จะได้สว่างๆ ตัดกับฉากหลัง)
            Gizmos.color = Color.cyan;

            // แปลงค่า randomSpreadX จากหน่วย UI (พิกเซล) ให้กลายเป็นหน่วย World Space 
            // โดยการคูณกับสเกลของหน้าจอ (lossyScale) เพื่อให้เส้น Gizmos ขนาดยืดหดตามจอเป๊ะๆ
            float worldSpreadX = randomSpreadX * dropStartPoint.lossyScale.x;

            Vector3 centerPos = dropStartPoint.position;
            Vector3 leftPoint = centerPos - new Vector3(worldSpreadX, 0, 0);
            Vector3 rightPoint = centerPos + new Vector3(worldSpreadX, 0, 0);

            // 1. วาดเส้นแนวนอนบอกระยะกว้างสุดของการสุ่ม (ซ้ายไปขวา)
            Gizmos.DrawLine(leftPoint, rightPoint);

            // 2. วาดเส้นแนวตั้งเล็กๆ ที่ขอบซ้ายและขวา (คล้ายๆ ขีดไม้บรรทัด) จะได้ดูง่ายขึ้น
            float tickSize = 10f * dropStartPoint.lossyScale.y;
            Gizmos.DrawLine(leftPoint + Vector3.up * tickSize, leftPoint - Vector3.up * tickSize);
            Gizmos.DrawLine(rightPoint + Vector3.up * tickSize, rightPoint - Vector3.up * tickSize);

            // 3. วาดจุดวงกลมเล็กๆ ตรงกลาง (จุด Center จริงๆ)
            Gizmos.DrawWireSphere(centerPos, 5f * dropStartPoint.lossyScale.x);
        }
    }

}