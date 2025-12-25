using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Áß¾Ó HUD")]
    [SerializeField] private Image hpGauge;
    [SerializeField] private Image apGauge;
    [SerializeField] private Image dashGauge;

    [Header("¿ùµå UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void UpdateHP(float currentHp, float maxHp)
    {
        float ratio = currentHp / maxHp;

        hpGauge.fillAmount = ratio * 0.21f;
    }

    public void UpdateAbilitySlider(float currentGauge)
    {
        float ratio = currentGauge / 100f;

        apGauge.fillAmount = ratio * 0.21f;
    }

    public void UpdateDashSlider(float amount)
    {
        dashGauge.fillAmount = amount * 0.21f;

        dashGauge.color = (amount >= 1f) ? Color.skyBlue : new Color(1, 1, 1, 0.5f);
    }

    public void UpdateAmmoText(int currentAmmo, int totalAmmo)
    {
        ammoText.text = currentAmmo.ToString();

        ammoText.text = $"{currentAmmo}";

        ammoText.color = currentAmmo == 0 ? Color.red : Color.white;
    }
}
