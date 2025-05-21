using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CommandMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Button[] buttons;
    [SerializeField] private List<CommandInputType> buttonInputTypes;
    public CommandInput commandInput;
    public ClearUtility clearUtility;
    public CharacterSelector characterSelector;
    public List <InputToCommandMap> currentImpToCommand = new List <InputToCommandMap>();


    private void Awake()
    {
        if (commandInput == null) { commandInput = GetComponent<CommandInput>(); }
        if (characterSelector == null) 
        { 
            characterSelector = GetComponent<CharacterSelector>();
            
        }
        if (clearUtility == null) { clearUtility = GetComponent<ClearUtility>(); }
        characterSelector.changeCharacter += SetMappedCommandType;
    }

    private void Start()
    {
        SetMappedDefaultCommandType();
        AssignButtonListeners();
        DisablePanelButtons(); // Or EnablePanelButtons() depending on default state
    }

    private void SetMappedDefaultCommandType()
    {
        currentImpToCommand.Clear();

        for (int i = 0; i < buttonInputTypes.Count; i++)
        {
            CommandInputType inputType = buttonInputTypes[i];

            // Check if inputType has a direct match in CommandType (e.g., Move, Attack, EndTurn)
            if (Enum.IsDefined(typeof(CommandType), (int)inputType))
            {
                InputToCommandMap newInputCommand = new InputToCommandMap
                {
                    inputType = inputType,
                    commandType = (CommandType)inputType

                };
                if (i == 5) { newInputCommand.commandType = CommandType.EndTurn; }
                currentImpToCommand.Add(newInputCommand);
            }
        }
    }

    private void Update()
    {
        if (characterSelector.selectedEntity != null && !characterSelector.selectedEntity.IsBusy)
        {
            EnablePanelButtons();
        }
        else if (characterSelector.selectedEntity != null && characterSelector.selectedEntity.IsBusy)
        {
            DisablePanelButtons();
        }
    }
    public void SetMappedCommandType() // I know exactly what are the values of in 0,1 and 6 imn exact order Move, Attack and EndTurn I didn't want to Add More complexity and Don't use above 6
    {
        if (characterSelector.selectedEntity == null) { return; }
        for (int i = 0; i < currentImpToCommand.Count; i++)
        {
            if (i <= 1 || i >= 6) { continue; }
            else if (i == 5) 
            { 
                currentImpToCommand[i].commandType = CommandType.EndTurn; 
                continue; 
            }
            InputToCommandMap newInputCommand = characterSelector.selectedEntity.GetInputToCommand(i - 2);
            if (newInputCommand != null)
            {
                currentImpToCommand[i] = newInputCommand;
            }

        }
    }
    private CommandType GetMappedCommandType(CommandInputType inputType) 
    {
        int inputToCheck = (int)inputType;
        return currentImpToCommand[inputToCheck].commandType;
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
            commandInput.currentMenuCommand = CommandInputType.None;
            return;
                
        }

        CommandInputType inputType = buttonInputTypes[index];
        CommandType newCommand = GetMappedCommandType(inputType);
        

        // Toggle behavior: deselect if already selected
        if (commandInput.currentMenuCommand == inputType)
        {
            commandInput.readyCommand = CommandType.None;
            commandInput.currentMenuCommand = CommandInputType.None;
            clearUtility.ClearAllHighLighters();
            EnablePanelButtons(); // Re-enable all buttons
            return;
        }
        // Select new command
        commandInput.readyCommand = newCommand;
        commandInput.currentMenuCommand = inputType;
        if (newCommand != CommandType.EndTurn)
        {
            commandInput.InitCommand();
        }
        else 
        {
            commandInput.readyCommand = CommandType.None;
            commandInput.currentMenuCommand = CommandInputType.None;
            commandInput.InitEndTurnCommand();
        }
        
        DisablePanelButtons(); // Lock out others
    }

    public void EnablePanelButtons()
    {
        if (commandInput.currentMenuCommand == CommandInputType.None)
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
                buttons[i].interactable = (i == (int)commandInput.currentMenuCommand);
            }
        }

    }

    public void DisablePanelButtons()
    {
        if (commandInput.currentMenuCommand == CommandInputType.None || characterSelector.selectedEntity.IsBusy)
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].interactable = false;
        }
        else
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = (i==(int)commandInput.currentMenuCommand);
            }
        }
    }

}
