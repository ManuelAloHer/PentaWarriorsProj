using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HealthAndConcenUI : MonoBehaviour
{
    [Header("CharacterSelector can be null")]
    [SerializeField] CharacterSelector characterSelector;
    private Entity chechedEntity;
    [SerializeField] HealthConcentComp healthConcentComp;
    [SerializeField] Slider healthSlider,concSlider;
    [SerializeField] TMP_Text healthText, concenText;
    private void Awake()
    {
        if (characterSelector == null && healthConcentComp != null) 
        {
            healthConcentComp.healthGained += ActualizeCurrentBars;
            healthConcentComp.healthLost += ActualizeCurrentBars;
        }
    }
    private void Start()
    {
        if (healthConcentComp != null) 
        {
            ActualizeCurrentBars();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (characterSelector == null)
        {
            return;
        }
        if (characterSelector.selectedEntity == null && chechedEntity != null) 
        {
            UnknownCurrentBarsValues();
            chechedEntity = null;
            return;
        }
        if (characterSelector.selectedEntity != chechedEntity) 
        {
            healthConcentComp = characterSelector.selectedEntity.GetComponent<HealthConcentComp>();
            healthConcentComp.healthGained += ActualizeCurrentBars;
            healthConcentComp.healthLost += ActualizeCurrentBars;
            ActualizeCurrentBars();
            chechedEntity = characterSelector.selectedEntity;

        }
    }
    private void ActualizeCurrentBars() 
    {
        healthSlider.value = healthConcentComp.GetHealthForSlider();
        concSlider.value = healthConcentComp.GetConcentrationForSlider();
        healthText.text = string.Format("{0}/{1}",healthConcentComp.Health,healthConcentComp.MaxHealth);
        concenText.text = string.Format("{0}/{1}", healthConcentComp.Concentration, healthConcentComp.MaxConcentration);
    }
    private void UnknownCurrentBarsValues() 
    {
        healthSlider.value = 1;
        concSlider.value = 1;
        healthText.text = string.Format("????/????");
        concenText.text = string.Format("????/????");
    }
}
