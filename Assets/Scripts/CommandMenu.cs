using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button[] buttons; 
    public CommandInput commandInput;
    public CharacterSelector characterSelector;


    private void Awake()
    {
        if (commandInput == null)
            commandInput = GetComponent<CommandInput>();
    }

    private void Start()
    {
        AssignButtonListeners();
        DisablePanelButtons(); // Or EnablePanelButtons() depending on default state
    }

    private void Update()
    {
        if (characterSelector.selectedEntity != null) 
        {
            EnablePanelButtons();
        }
    }
    private void AssignButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Important: capture loop index
            buttons[i].onClick.AddListener(() => OnButtonPressed(index));
        }
    }

    private void OnButtonPressed(int index)
    {
        if (!buttons[index].interactable) return;
        if (characterSelector.selectedEntity.entityTurn.currentTurnActions <= 0) 
        {
            commandInput.readyCommand = CommandType.None;
            return;
                
        }
        CommandType newCommand = (CommandType)index;

        // Toggle behavior: deselect if already selected
        if (commandInput.readyCommand == newCommand)
        {
            commandInput.readyCommand = CommandType.None;
            EnablePanelButtons(); // Re-enable all buttons
            return;
        }

        // Select new command
        commandInput.readyCommand = newCommand;
        commandInput.InitCommand();
        DisablePanelButtons(); // Lock out others
    }

    public void EnablePanelButtons()
    {
        if (commandInput.readyCommand == CommandType.None)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = true;
            }
        }
        else
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = (i == (int)commandInput.readyCommand);
            }
        }

    }

    public void DisablePanelButtons()
    {
        if (commandInput.readyCommand == CommandType.None)
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].interactable = false;
        }
        else
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = (i==(int)commandInput.readyCommand );
            }
        }
    }

}
