using UnityEngine;

public class DestoryEndTime : MonoBehaviour , ITimeDestroy
{
    public float timeDestory = 0.1f;
    
    // Implement จาก Interface
    public float _lifeTime
    {
        get => timeDestory;
        set => timeDestory = value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Destroy(gameObject, timeDestory);

    }

    private void OnEnable()
    {
        StartLifeTime();
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ReturnObjectToPool));

        //PlayerInputActionsManager.instance.LoadBindingToPlayerContrlorsCS();
    }

    public void StartLifeTime()
    {
        // ยกเลิกคำสั่งเดิมก่อน ป้องกันบั๊กนับเวลาเบิ้ล
        CancelInvoke(nameof(ReturnObjectToPool));

        // ถ้า Object ยังเปิดใช้งานอยู่ ค่อยสั่งทำงาน
        if (gameObject.activeInHierarchy)
        {
            Invoke(nameof(ReturnObjectToPool), _lifeTime);
        }
    }

    private void ReturnObjectToPool()
    {
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}
