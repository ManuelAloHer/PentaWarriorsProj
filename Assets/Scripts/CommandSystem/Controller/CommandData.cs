using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandData // ClassResponsibleToHoldCommandExecutionData
{
    public CommandType commandType;
    public IActionEffect effect;
    public ICommandInputStage inputStage;

    public CommandData(CommandType type, IActionEffect effect, ICommandInputStage inputStage = null)
    {
        this.commandType = type;
        this.effect = effect;
        this.inputStage = inputStage;
    }
}