using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    InputController inputController;
    CommandMenu comMenu;
    
    public Entity selectedEntity;
    ObjectInGrid hoveredObject;
    public Entity hoveredEntity;
    Vector3Int positionOnGrid = new Vector3Int(-1, -1, -1);
    [SerializeField] GridMap targetGrid;

    [SerializeField] TMP_Text selectedCharText;

    void Awake()
    {
        inputController = GetComponent<InputController>();
        comMenu = GetComponent<CommandMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        HoverOver();
        SelectCharacter(hoveredEntity);
        UnselectCharacter();
    }

    private void HoverOver()
    {
        if (positionOnGrid != inputController.PosOnGrid)
        {
            // If Displayed hovered objectUI Undisplay it
            positionOnGrid = inputController.PosOnGrid;
            hoveredObject = targetGrid.GetPlacedObject(positionOnGrid);
            if (hoveredObject != null)
            {
                hoveredEntity = hoveredObject.GetEntity();
            }
            else
            {
                hoveredEntity = null;
            }
            if (hoveredEntity != null)
            {
                //Display hovered objectUI
            }
        }
    }

    private void SelectCharacter(Entity characterToSelect)
    {
        if (inputController.IsConfirmPressed())
        {
            if (hoveredEntity != null && selectedEntity == null && hoveredEntity.characterAliance == Aliance.Player) 
            {
                selectedEntity = characterToSelect;
            }
            
            UpdatePanel();
        }
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

    private void UnselectCharacter()
    {
        if (inputController.IsCancelPressed())
        {
            Deselect();
        }
    }

    private void LateUpdate()
    {
        if (selectedCharText == null || selectedEntity == null) 
        {
            selectedCharText.text = "Not Selected";
        }
        else
        {
            selectedCharText.text = selectedEntity.CharacterName;
        }
    }

    public void Deselect()
    {
        selectedEntity = null;
    }
}
