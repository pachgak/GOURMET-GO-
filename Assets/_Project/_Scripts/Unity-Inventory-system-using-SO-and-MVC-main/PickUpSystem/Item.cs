using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Item : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO itemSO { get; private set; }
    public SpriteRenderer itemDropImage;

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private float duration = 0.3f;

    private Collider _collider;
    private Rigidbody _rg;
    private NavMeshAgent _navAgent;

    private bool _canPickUp;

    private Vector3 originScale;

    public Action OnItemSetup;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _rg = GetComponent<Rigidbody>();
        _navAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        originScale = transform.localScale;
    }

    public void Setup(ItemSO _itemSO, int _quantity)
    {
        itemSO = _itemSO;
        Quantity = _quantity;

        UpProfind();
    }

    public void UpProfind()
    {

        itemDropImage.sprite = this.itemSO.ItemImage;
        _collider.enabled = true;
        if (_rg != null)
        {
            _rg.useGravity = true;
            _rg.isKinematic = false;
        }
        if (_navAgent != null)
        {
            _navAgent.isStopped = false;
        }


        GetComponent<InteractableBase>().message = $"PickUp : {this.itemSO.ItemName} x {Quantity}";
        transform.localScale = originScale;

        OnItemSetup?.Invoke();
    }

    public void Start()
    {

    }

    public void DestroyItem()
    {
        _collider.enabled = false;

        if (_rg != null)
        {
            _rg.useGravity = false;

            // ทำให้เป็น Kinematic ชั่วคราวด้วยก็ได้ เพื่อกันแรงกระแทกอื่นๆ (Optional)
            _rg.isKinematic = true;
        }
        if (_navAgent != null)
        {
            _navAgent.isStopped = true;
        }

        StartCoroutine(AnimateItemPickup());

    }

    private IEnumerator AnimateItemPickup()
    {
        audioSource.Play();

        // 1. หา Player ตรงนี้เลย ตามที่คุณต้องการ
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;

        // เก็บตำแหน่งเริ่มต้นของไอเทม
        Vector3 startPosition = transform.position;

        Vector3 playerOffset = Vector3.up;

        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration; // ค่า 0 ถึง 1

            // 2. Scale ลงเรื่อยๆ (เล็กลง)
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            // 3. บินเข้าหา Player (ถ้าหาเจอ)
            if (playerTransform != null)
            {
                // Lerp จากจุดเริ่ม ไปยังจุดที่ Player ยืนอยู่ ณ ปัจจุบัน
                transform.position = Vector3.Lerp(startPosition, playerTransform.position + playerOffset, t);
            }

            yield return null;
        }

        // ต้องมั่นใจว่า Scale เป็น 0 สนิท และอยู่กับตัว Player แล้วจริงๆ
        transform.localScale = endScale;
        if (playerTransform != null) transform.position = playerTransform.position;

        ReturnObjectToPool();
        //Destroy(gameObject);
    }

    private void ReturnObjectToPool()
    {
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}
