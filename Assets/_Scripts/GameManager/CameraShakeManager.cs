using DG.Tweening;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;

    public GameObject playerGameObject;
    public Transform shakeParent;
    private Vector3 currentStrength;
    private Vector3 currentDuration;
    private Vector3 originalPosition;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = shakeParent.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShakeRotation()
    {
        shakeParent.DOShakePosition(
    duration: 0.5f,
    strength: new Vector3(10f, 10f, 0f),
    vibrato: 10,
    randomness: 90,
    fadeOut: true
    );
    }

    public void ShakePlayerAttack()
    {
        shakeParent.DOKill(true);
        //Debug.Log("ShakePlayerAttack");

        shakeParent.transform.DOShakePosition(
    duration: 0.2f,
    strength: new Vector3(0.1f, 0.1f, 0f),
    vibrato: 5,
    randomness: 0,
    fadeOut: true
    ).OnComplete(() =>
    {
        // ใช้ DOMove/DOLocalMove เพื่อย้ายกลับไปตำแหน่งเริ่มต้นอย่างนุ่มนวล
        // (หรือจะตั้งค่า duration เป็น 0f เพื่อย้ายกลับทันทีก็ได้)
        shakeParent.DOLocalMove(originalPosition, 0.1f);
    }); ;
    }

    public void ShakePlayerTakeDamage()
    {
        shakeParent.DOKill(true);
        //Debug.Log("ShakePlayerTakeDamage");

        shakeParent.transform.DOShakePosition(
    duration: 0.2f,
    strength: new Vector3(0.5f, 0.5f, 0f),
    vibrato: 10,
    randomness: 0,
    fadeOut: true
    ).OnComplete(() =>
    {
        // ใช้ DOMove/DOLocalMove เพื่อย้ายกลับไปตำแหน่งเริ่มต้นอย่างนุ่มนวล
        // (หรือจะตั้งค่า duration เป็น 0f เพื่อย้ายกลับทันทีก็ได้)
        shakeParent.DOLocalMove(originalPosition, 0.1f);
    }); ;
    }
}
