using Inventory.Model;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SpawnItemDropPoor : MonoBehaviour
{
    public GameObject itemDropPrefab;
    public List<ItemDropPoor> Items;
    public Vector3 offSet;
    private EnemyHealth enemyHealth;

    private bool isTriger;
    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null) enemyHealth.OnDie += HealdeDropPoorItems;
    }
    private void OnDisable()
    {
        if (enemyHealth != null) enemyHealth.OnDie -= HealdeDropPoorItems;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HealdeDropPoorItems()
    {
        if (isTriger) return;

        foreach (var itemsSo in Items)
        {
            if (itemsSo == null) continue;

            int itemCount = (itemsSo.countMax <= 0) ? itemsSo.countMin : UnityEngine.Random.Range(itemsSo.countMin, itemsSo.countMax + 1);

            for (int i = 0; i < itemCount; i++)
            {
                GameObject itemDrop = ObjectPoolingManager.Instance.Spawn(itemDropPrefab, transform.position + offSet);
                if (itemDrop.TryGetComponent(out Item item))
                {
                    item.Setup(itemsSo.item, 1);
                }
            }

            
        }

        isTriger = true;
    }

    [Serializable]
    public class ItemDropPoor
    {
        public ItemSO item;
        public int countMin;
        public int countMax;

    }
}
