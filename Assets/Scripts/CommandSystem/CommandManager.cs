using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.TextCore.Text;


public enum CommandType
{
    None = -1, Move = 0, Attack = 1,
    AtkOnArea = 2, AddStatus = 3, RemoveStatus = 4,
    AtkWithStatus = 5, AddReaction = 6, Heal = 7,
    Telekinesis = 8, TakeCard = 9, Shuffle = 10,
    AddOrTakeAcción = 11, Interchange = 12, Rise = 13, EndTurn = 14
}
public enum CommandInputType { None = -1, Move = 0, Attack = 1, SpeHab1 = 2, SpeHab2 = 3, SpeHab3 = 4, EndTurn = 5, CardAction = 6 }
public class Command 
{ 
    // Common variables for Commands
    public Entity character;
    public Vector3Int selectedGridPoint;
    public CommandType commandType;

    //Specific variables of one or more Commands
    public List<PathNode> path = null;
    public ObjectInGrid target;

    public Command(Entity character, Vector3Int selectedGridPoint, CommandType commandType)
    {
        this.character = character;
        this.selectedGridPoint = selectedGridPoint;
        this.commandType = commandType;
    }
}
public class CommandManager : MonoBehaviour
{
    Command currentCommand;
    ClearUtility clearUtility;

    private void Awake()
    {
        if (clearUtility == null) { clearUtility = GetComponent<ClearUtility>(); }
    }
    private void Update()
    {
        if (currentCommand != null)
        {
            ExecuteCommand();
        }
    }

    public void AddMoveCommand(Entity character, Vector3Int selectedGridPoint, List<PathNode> path)
    {
        currentCommand = new Command(character, selectedGridPoint, CommandType.Move);
        currentCommand.path = path;
    }
    public void AddAttackCommand(Entity attacker, Vector3Int selectedGridPoint, ObjectInGrid target)// List<PathNode> path) // It could be necesary to calculate a path for the proyectile if we go that far
    {
        currentCommand = new Command(attacker, selectedGridPoint, CommandType.Attack);
        currentCommand.target = target;
        //currentCommand.path = path;
    }
    public void AddAttackOnAreaCommand(Entity attacker, Vector3Int selectedGridPoint, ObjectInGrid target)
    {
        currentCommand = new Command(attacker, selectedGridPoint, CommandType.AtkOnArea);
        currentCommand.target = target;

    }
    public void AddFinishTurnCommand(Entity character)
    {
        Vector3Int charactPos = character.gridObject.positionInGrid;
        currentCommand = new Command(character, charactPos, CommandType.EndTurn);
    }
    public void AddCommand(Entity character, Vector3Int selectedGridPoint, CommandType commandType)
    {
        currentCommand = new Command(character, selectedGridPoint, commandType);
    }
    public CommandType GetCurrentCommandType()
    {
        return currentCommand.commandType;
    }
    public void ExecuteCommand()
    {
        if (currentCommand.commandType == CommandType.Move)
        {
            ExecuteMoveCommand();
        }
        else if (currentCommand.commandType == CommandType.Attack)
        {
            ExecuteAttackCommand();
        }
        else if (currentCommand.commandType == CommandType.AtkOnArea)
        {
            ExecuteAttackOnAreaCommand();
        }
        else if (currentCommand.commandType == CommandType.EndTurn)
        {
            ExecuteEndTurnCommand();
        }
    }

    private void ExecuteEndTurnCommand()
    {
        Entity receiver = currentCommand.character;
        receiver.ConsumeAllActions();
    }

    private void ExecuteMoveCommand()
    {
        Entity receiver = currentCommand.character;
        receiver.gridObject.Move(currentCommand.path);
        receiver.ConsumeActions(false);
        currentCommand = null;
        clearUtility.ClearPathfinding();
        clearUtility.ClearGridHighlighter(0);

    }

    private void ExecuteAttackCommand()
    {
        Entity receiver = currentCommand.character;
        receiver.gridObject.Attack(currentCommand.selectedGridPoint, currentCommand.target);
        receiver.ConsumeActions(false);
        currentCommand = null;
        clearUtility.ClearGridHighlighter(1);

    }
    private void ExecuteAttackOnAreaCommand()
    {
        Entity receiver = currentCommand.character;
        //receiver notifies gridObject the AdE Action
        receiver.ConsumeActions(false);
        currentCommand = null;
        // clearUtility.Clear the correspondingHighlighters

    }
    private void ExecuteAddStatusCommand()
    {
        Entity receiver = currentCommand.character;
        Entity targetEntity = currentCommand.target.GetEntity();
        //AddStatus in Entity     receiver.gridObject.Attack(currentCommand.selectedGridPoint, currentCommand.target);
        receiver.ConsumeActions(false);
        currentCommand = null;
        // clearUtility.Clear the correspondingHighlighters
    }
    private void ExecuteEndStatusCommand()
    {
        Entity receiver = currentCommand.character;
        Entity targetEntity = currentCommand.target.GetEntity();
        //EndAllStatus in Entity     receiver.gridObject.Attack(currentCommand.selectedGridPoint, currentCommand.target);
        receiver.ConsumeActions(false);
        currentCommand = null;
        // clearUtility.Clear the correspondingHighlighters

    }
}
