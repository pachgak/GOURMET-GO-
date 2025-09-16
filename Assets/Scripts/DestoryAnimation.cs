using UnityEngine;

public class DestoryAnimation : MonoBehaviour
{
    public bool isDestroy = true;
    public GameObject destroyThis;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyAnimation()
    {
        if (!isDestroy) return;

        if (destroyThis != null) GameObject.Destroy(destroyThis);
        else GameObject.Destroy(gameObject);
    }
}
