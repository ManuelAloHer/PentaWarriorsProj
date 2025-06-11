using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HealthAndConcenUI1: MonoBehaviour
{
    [Header("CharacterSelector can be null")]
    [SerializeField] CharacterSelector characterSelector;
    private Entity checkedEntity;

    [Header("UI FIELDS")]
    [SerializeField] TMP_Text selectedCharText;
    [SerializeField] Image selectedCharPortrait;
    [SerializeField] Sprite portraitDefaultImage;
    [SerializeField] HealthConcentComp healthConcentComp;
    [SerializeField] Slider healthSlider,concSlider;
    [SerializeField] TMP_Text healthText, concenText;
    [SerializeField] bool hovereable = false;
    private void Awake()
    {
    }
    private void Start()
    {
        if (healthConcentComp != null)
        {
            ActualizeCurrentBars();
        }
        else 
        {
            UnknownCurrentBarsValues();
        }
        selectedCharPortrait.sprite = portraitDefaultImage;
        selectedCharText.text = "Not Selected";

    }
    // Update is called once per frame
    void Update()
    {
        if (characterSelector == null)
        {
            UnknownCurrentBarsValues();
            selectedCharPortrait.sprite = portraitDefaultImage;
            selectedCharText.text = "Not Selected";
            return;
        }
        if (checkedEntity != null && (characterSelector.hoveredEntity == null)) 
        {
            UnknownCurrentBarsValues();
            selectedCharPortrait.sprite = portraitDefaultImage;
            selectedCharText.text = "Not Selected";
            checkedEntity = null;
            return;
        }
        if (checkedEntity != null && !checkedEntity.showsInfoOnHovereable) 
        {
            UnknownCurrentBarsValues();
            selectedCharPortrait.sprite = checkedEntity.sprite;
            selectedCharText.text = checkedEntity.CharacterName;    
            return;
        }
        if (hovereable && characterSelector.hoveredEntity != checkedEntity)
        {
            healthConcentComp = characterSelector.hoveredEntity.GetComponent<HealthConcentComp>();
            healthConcentComp.healthGained += ActualizeCurrentBars;
            healthConcentComp.healthLost += ActualizeCurrentBars;
            healthConcentComp.hasConcetrationChanged += ActualizeCurrentBars;
            ActualizeCurrentBars();
            checkedEntity = characterSelector.hoveredEntity;
            selectedCharPortrait.sprite = checkedEntity.sprite;
            selectedCharText.text = checkedEntity.CharacterName;
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
