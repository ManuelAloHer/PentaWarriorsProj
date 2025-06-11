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
    private Entity checkedEntity;
    [SerializeField] HealthConcentComp healthConcentComp;
    [SerializeField] Slider healthSlider,concSlider;
    [SerializeField] TMP_Text healthText, concenText;
    [SerializeField] bool hovereable = false;
    private void Awake()
    {
        if (characterSelector == null && healthConcentComp != null) 
        {
            healthConcentComp.healthGained += ActualizeCurrentBars;
            healthConcentComp.healthLost += ActualizeCurrentBars;
            healthConcentComp.hasConcetrationChanged += ActualizeCurrentBars;
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
        if (checkedEntity != null && ( !hovereable && characterSelector.selectedEntity == null) || (hovereable && characterSelector.hoveredEntity)  ) 
        {
            UnknownCurrentBarsValues();
            checkedEntity = null;
            return;
        }
        if (!hovereable && characterSelector.selectedEntity != checkedEntity) 
        {
            healthConcentComp = characterSelector.selectedEntity.GetComponent<HealthConcentComp>();
            healthConcentComp.healthGained += ActualizeCurrentBars;
            healthConcentComp.healthLost += ActualizeCurrentBars;
            healthConcentComp.hasConcetrationChanged += ActualizeCurrentBars;
            ActualizeCurrentBars();
            checkedEntity = characterSelector.selectedEntity;

        }
        if (hovereable && characterSelector.hoveredEntity != checkedEntity)
        {
            healthConcentComp = characterSelector.hoveredEntity.GetComponent<HealthConcentComp>();
            healthConcentComp.healthGained += ActualizeCurrentBars;
            healthConcentComp.healthLost += ActualizeCurrentBars;
            healthConcentComp.hasConcetrationChanged += ActualizeCurrentBars;
            ActualizeCurrentBars();
            checkedEntity = characterSelector.hoveredEntity;
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
