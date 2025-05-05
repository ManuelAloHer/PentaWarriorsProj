using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class CommandMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    public CommandInput commandInput;

    private void Awake()
    {
        if (commandInput == null) { commandInput = GetComponent<CommandInput>(); }
    }
    public void EnablePanelButtons()
    {
        panel.SetActive(true);
    }

    public void DisablePanelButtons()
    {
        panel.SetActive(false);
    }
    public void ChangeCommandState(CommandType newCommand)
    {
        if (commandInput.readyCommand == newCommand) { commandInput.readyCommand = CommandType.None; }
        else
        {
            commandInput.readyCommand = newCommand;
            commandInput.InitCommand();

        }
    }
    public void MoveButtonControlState()
    {
        ChangeCommandState(CommandType.Move);
        DisablePanelButtons();
    }
    public void AttackButtonControlState()
    {
        ChangeCommandState(CommandType.Attack);
        DisablePanelButtons();
    }
}
