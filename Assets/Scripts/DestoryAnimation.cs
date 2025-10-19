using Unity.VisualScripting;
using UnityEngine;

public class DestoryAnimation : MonoBehaviour
{
    public bool isDestroy = true;
    public GameObject destroyThis;

    public enum MyEnum
    {
        TheWorld,Player,Stop
    }
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

    public void GetIntSetOOs(int valu )
    {
        Debug.Log("GetIntSet : " + valu );
    }

    public void Setsssssssss()
    {

    }
}
