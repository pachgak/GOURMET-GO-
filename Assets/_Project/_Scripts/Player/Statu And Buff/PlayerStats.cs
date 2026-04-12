using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    [HideInInspector] public float baseValue;
    private List<float> modifiers = new List<float>();
    [SerializeField] private float _cachedMultiplier = 1f;

    public void AddModifier(float mod) { modifiers.Add(mod); UpdateCachedMultiplier(); }
    public void RemoveModifier(float mod) { modifiers.Remove(mod); UpdateCachedMultiplier(); }

    private void UpdateCachedMultiplier()
    {
        float percentMultiplier = 0f;
        foreach (float mod in modifiers) percentMultiplier += mod;
        _cachedMultiplier = 1f + percentMultiplier;
    }

    public float GetValue() => baseValue * _cachedMultiplier;
    public float GetMultiplier() => _cachedMultiplier;
}

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public Stat moveSpeed = new Stat { baseValue = 1f };
    public Stat attackPower = new Stat { baseValue = 1f };
    public Stat maxHealth;
    public Stat dashRang;

    private PlayerHealth _health;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        maxHealth.baseValue = _health.baseMaxHealth;
    }

    public void UpdateMaxHealth()
    {
        float oldMax = _health.currentMaxHealth;
        _health.currentMaxHealth = _health.baseMaxHealth * maxHealth.GetMultiplier();

        float healthRatio = _health.currentHealth / oldMax;
        _health.setHp(_health.currentMaxHealth * healthRatio);
    }
}