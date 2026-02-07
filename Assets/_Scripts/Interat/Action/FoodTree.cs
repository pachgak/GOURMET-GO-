using System;
using UnityEngine;

public class FoodTree : SpawnItemDropPoor
{
    public SpriteRenderer spriteRenderer;
    public Sprite imageEmptyTree;
    private bool _isEmpty = false;

    public event Action OnPick;

    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IntractTree()
    {
        spriteRenderer.sprite = imageEmptyTree;

        HealdeDropPoorItems();

        gameObject.layer = 0;

        _isEmpty = true;

        this.enabled = false;

        OnPick?.Invoke();
    }
}
