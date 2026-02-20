using UnityEngine;

public class FragileWall : MonoBehaviour
{
    public GameObject breakVFX; // เอฟเฟกต์หินแตก (ถ้ามี)

    public void BreakWall()
    {
        if (breakVFX != null)
        {
            ObjectPoolingManager.Instance.Spawn(breakVFX, transform.position);
        }
        // คืนค่าเข้า Pool หรือทำลายทิ้ง
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}