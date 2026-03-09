using DG.Tweening;
using UnityEngine;

public class RotateMove : MonoBehaviour, ISpeed
{
    public float speed = 20f;
    float ISpeed._speed { get => speed; set => speed = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
