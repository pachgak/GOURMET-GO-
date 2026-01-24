using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool _canPickUp;

    private Vector3 originScale;

    public Action OnItemSetup;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        originScale = transform.localScale;
    }

    public void Setup(ItemSO _itemSO,int _quantity)
    {
        itemSO = _itemSO;
        Quantity = _quantity;

        UpProfind();
    }

    public void UpProfind()
    {
        
        itemDropImage.sprite = this.itemSO.ItemImage;
        _collider.enabled = true;
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
        StartCoroutine(AnimateItemPickup());

    }

    private IEnumerator AnimateItemPickup()
    {
        audioSource.Play();
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = 
                Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        ReturnObjectToPool();
        //Destroy(gameObject);
    }

    private void ReturnObjectToPool()
    {
        ObjectPoolingManager.Instance.Respawn(gameObject);
    }
}
