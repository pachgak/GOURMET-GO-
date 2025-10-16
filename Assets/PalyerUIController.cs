using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PalyerUIController : MonoBehaviour
{
    private PlayerHealth playerHealth;

    public Slider hpBar;
    public TMP_Text hpText;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        playerHealth.OnCurrentChang += HandleCurrentChang;
    }

    private void OnDisable()
    {
        playerHealth.OnCurrentChang -= HandleCurrentChang;
    }

    private void HandleCurrentChang(float obj)
    {
        hpBar.value = obj;
        hpText.text = $"{obj}/{playerHealth.maxHealth}";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpBar.maxValue = playerHealth.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
