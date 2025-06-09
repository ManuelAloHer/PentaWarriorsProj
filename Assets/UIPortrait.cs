using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPortrait : MonoBehaviour
{
    [SerializeField] Image charPortrait;
    [SerializeField] TMP_Text selectedCharText;
    [SerializeField] Entity displayEntity;
    // Start is called before the first frame update
    void Start()
    {
        if (displayEntity.sprite != null) { charPortrait.sprite = displayEntity.sprite; }
        selectedCharText.text = displayEntity.CharacterName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
