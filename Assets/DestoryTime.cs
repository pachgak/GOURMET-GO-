using UnityEngine;

public class DestoryTime : MonoBehaviour
{
    public float timeDestory = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, timeDestory);
    }

    private void OnEnable()
    {
        //ReturnObjectToPool();
    }

    private void ReturnObjectToPool()
    {
        ObjectPoolingManager.Instance.ReturnObjectToPool(gameObject);
    }
}
