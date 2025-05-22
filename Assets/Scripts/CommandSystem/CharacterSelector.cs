using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    InputController inputController;
    CommandInput commandInput;
    CommandMenu comMenu;
    
    public Entity selectedEntity;
    ObjectInGrid hoveredObject;
    public Entity hoveredEntity;
    Vector3Int positionOnGrid = new Vector3Int(-1, -1, -1);
    [SerializeField] GridMap targetGrid;
    [SerializeField] ClearUtility clearUtility;
    public event Action changeCharacter;

    [Header("UI FIELDS")]
    [SerializeField] TMP_Text selectedCharText;
    [SerializeField] Image selectedCharPortrait;
    [SerializeField] Sprite portraitDefaultImage;

    void Awake()
    {
        inputController = GetComponent<InputController>();
        commandInput = GetComponent<CommandInput>();
        comMenu = GetComponent<CommandMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        HoverOver();
    }

    private void HoverOver()
    {
        if (InputController.IsPointerOverUI()) { return;}
        if (positionOnGrid != inputController.PosOnGrid)
        {
            // If Displayed hovered objectUI Undisplay it
            positionOnGrid = inputController.PosOnGrid;
            hoveredObject = targetGrid.GetPlacedObject(positionOnGrid);
            if (hoveredObject != null)
            {
                hoveredEntity = hoveredObject.GetEntity();
                //Display hovered objectUI
            }
            else
            {
                hoveredEntity = null;
            }
        }
    }

    public void SelectCharacter(Entity characterToSelect)
    {
        if (characterToSelect.characterAliance != Aliance.Player) { return; }
        selectedEntity = characterToSelect;
        ChangePortraitAndNameState();
        changeCharacter();
        //if (inputController.IsConfirmPressed())
        //{
        //    if (hoveredEntity != null && selectedEntity == null && hoveredEntity.characterAliance == Aliance.Player) 
        //    {
        //        selectedEntity = characterToSelect;
        //    }

        //    UpdatePanel();
        //}
    }

    private void UpdatePanel()
    {
        if (selectedEntity != null)
        {
            comMenu.EnablePanelButtons();
        }
        else 
        {
            comMenu.DisablePanelButtons();
        }
        
    }

    public void UnselectCharacter()
    {
        Deselect();
        UpdatePanel();
        clearUtility.ClearAllHighLighters();
        //if (inputController.IsCancelPressed())
        //{
        //    Deselect();
        //    UpdatePanel();
        //    clearUtility.ClearAllHighLighters();
        //}
    }

    private void LateUpdate()
    {
        //ChangePortraitAndNameState();
    }

    private void ChangePortraitAndNameState()
    {
        if (selectedCharText == null || selectedEntity == null)
        {
            selectedCharText.text = "Not Selected";
            if (selectedCharPortrait != null)
            {
                selectedCharPortrait.sprite = portraitDefaultImage;
            }
        }
        else
        {
            selectedCharText.text = selectedEntity.CharacterName;
            if (selectedCharPortrait != null)
            {
                selectedCharPortrait.sprite = selectedEntity.sprite;
            }
        }
    }

    private void Deselect()
    {
        selectedEntity = null;
        changeCharacter();
        commandInput.readyCommand = CommandType.None;
        ChangePortraitAndNameState();
    }
}
