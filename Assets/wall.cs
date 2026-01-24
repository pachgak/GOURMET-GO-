using UnityEngine;

public class wall : MonoBehaviour , IKnockbackable
{
    public bool canKnockback;
    bool IKnockbackable._canKnockback { get => canKnockback; set => canKnockback = value; }
    
    [SerializeField] private float knockbackMultiplier;
    float IKnockbackable._knockbackMultiplier { get => knockbackMultiplier; set => knockbackMultiplier = value; }

    public void GetKnockedBack(Vector3 direction, float force, float time)
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
}
