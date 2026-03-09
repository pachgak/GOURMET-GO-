using Inventory.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropRageOld : MonoBehaviour
{
    public GameObject itemDropPrefab;
    public List<ItemDropFormat> Items;
    public Vector3 offSet;
    private EnemyHealth enemyHealth;

    private bool isTriger;
    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if(enemyHealth != null) enemyHealth.OnDie += HealdeDropItems;
    }
    private void OnDisable()
    {
        if (enemyHealth != null) enemyHealth.OnDie -= HealdeDropItems;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HealdeDropItems()
    {
        if (isTriger) return;

        foreach (var itemsSo in Items)
        {
            if (itemsSo == null) continue;

            GameObject itemDrop = ObjectPoolingManager.Instance.Spawn(itemDropPrefab,transform.position + offSet);
            if (itemDrop.TryGetComponent(out Item item))
            {
                item.Setup(itemsSo.item, (!itemsSo.isRandom) ? itemsSo.countMin : UnityEngine.Random.Range(itemsSo.countMin, itemsSo.countMax+1));
            }
        }

        isTriger = true;
    }

    [Serializable]
    public class ItemDropFormat
    {
        public ItemSO item;
        public int countMin;
        public bool isRandom;
        public int countMax;
    }
}
