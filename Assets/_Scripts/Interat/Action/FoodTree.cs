using UnityEngine;

public class FoodTree : ItemDropRage
{
    public Sprite inteactSprite;
    public bool isTakeIt = false;

    public SpriteRenderer _spriteRenderer;

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
        _spriteRenderer.sprite = inteactSprite;

        HealdeDropItems();

        gameObject.layer = 0;
    }
}
