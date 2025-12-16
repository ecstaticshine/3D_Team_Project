using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_UIManager : MonoBehaviour
{
    public static S_UIManager instance = null;
    public Slider abilitySlider;
    public TMP_Text ammoText;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void UpdateAbilitySlider(float currentGauge)
    {
        if (abilitySlider != null) abilitySlider.value = currentGauge;
    }

    public void UpdateAmmoText(int currentAmmo, int totalAmmo)
    {
        if (ammoText != null) ammoText.text = $"{currentAmmo} / {totalAmmo}";
    }
}
