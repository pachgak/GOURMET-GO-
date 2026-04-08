// „π‰ø≈Ï ICanTakeDamage.cs
using System;
using UnityEngine;

public interface ITakeDamage
{
    public Action<float , GameObject> OnTakeDamage { get; set; }
    public GameObject gameObjectOwner { get; }
    void TakeDamage(float damage, GameObject customHitVFX = null);
}