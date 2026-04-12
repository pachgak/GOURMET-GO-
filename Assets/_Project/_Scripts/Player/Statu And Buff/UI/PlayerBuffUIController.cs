using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffUIController : MonoBehaviour
{
    [Header("References")]
    public PlayerBuffManager buffManager;
    public Transform buffIconParent;
    public BuffUIItem buffUIItemPrefab;

    // ใช้ Dictionary ในการจำว่า บัพชื่อนี้ สร้าง UI Item ตัวไหนไว้ (เพื่อจะได้หาเจอไวๆ ตอนลบหรืออัปเดต)
    private Dictionary<string, BuffUIItem> activeUIItems = new Dictionary<string, BuffUIItem>();

    private void OnEnable()
    {
        if (buffManager != null)
        {
            buffManager.OnBuffAdded += HandleBuffAdded;
            buffManager.OnBuffRemoved += HandleBuffRemoved;
            buffManager.OnBuffUpdated += HandleBuffUpdated;
        }
    }

    private void OnDisable()
    {
        if (buffManager != null)
        {
            buffManager.OnBuffAdded -= HandleBuffAdded;
            buffManager.OnBuffRemoved -= HandleBuffRemoved;
            buffManager.OnBuffUpdated -= HandleBuffUpdated;
        }
    }

    private void HandleBuffAdded(BuffSO buff, float duration, int stack)
    {
        // ถ้ายังไม่มีไอคอนนี้บนจอ ให้สร้างใหม่
        if (!activeUIItems.ContainsKey(buff.buffName))
        {
            GameObject obj = Instantiate(buffUIItemPrefab.gameObject, buffIconParent);
            BuffUIItem uiItem = obj.GetComponent<BuffUIItem>();

            uiItem.Setup(buff.icon, duration, stack);
            activeUIItems.Add(buff.buffName, uiItem);
        }
        else
        {
            // ถ้ามีอยู่แล้ว แค่อัปเดตเวลาเฉยๆ
            HandleBuffUpdated(buff, duration, stack);
        }
    }

    private void HandleBuffUpdated(BuffSO buff, float duration, int stack)
    {
        // หาไอคอนบนจอให้เจอ แล้วสั่งเปลี่ยนเลขเวลา
        if (activeUIItems.TryGetValue(buff.buffName, out BuffUIItem uiItem))
        {
            uiItem.UpdateUI(duration, stack);
        }
    }

    private void HandleBuffRemoved(BuffSO buff)
    {
        // เมื่อบัพหมด สั่งทำลาย Game Object ไอคอนทิ้ง และลบออกจากความจำ
        if (activeUIItems.TryGetValue(buff.buffName, out BuffUIItem uiItem))
        {
            Destroy(uiItem.gameObject);
            activeUIItems.Remove(buff.buffName);
        }
    }
}