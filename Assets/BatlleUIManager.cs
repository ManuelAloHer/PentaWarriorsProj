using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BatlleUIManager : MonoBehaviour
{
    [SerializeField] TMP_Text batlleText;
    [SerializeField] GameObject[] CharacterRelevantUIComponents;
    public Sprite defaultCharSprite;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBatlleStartText()
    {
        batlleText.gameObject.SetActive(true);
        batlleText.text = "Batlle Start";
    }
    public void SetBatlleEndText(bool isBatlleWon)
    {
        batlleText.gameObject.SetActive(true);
        if (isBatlleWon) 
        {
            batlleText.text = "Victory";
            return;
        }
        batlleText.text = "You've Died";
    }
    public void HideBatlleText()
    {
        batlleText.gameObject.SetActive(false);
    }
    public void ShowBatlleRelevantUI() 
    { 
        foreach (var component in CharacterRelevantUIComponents) { component.gameObject.SetActive(true); }
    }
    public void HideBatlleRelevantUI()
    {
        foreach (var component in CharacterRelevantUIComponents) { component.gameObject.SetActive(false); }
    }
}
