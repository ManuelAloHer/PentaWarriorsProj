using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.TextCore.Text;


public enum CommandType { None, Move, Attack }

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
    public void AddAttackCommand(Entity attacker, Vector3Int selectedGridPoint, ObjectInGrid target)// List<PathNode> path) // It will be necesary to calculate a path for the proyectile if we go that far
    {
        currentCommand = new Command(attacker, selectedGridPoint, CommandType.Attack);
        currentCommand.target = target;
        //currentCommand.path = path;
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
    }

    private void ExecuteMoveCommand()
    {
        Entity receiver = currentCommand.character;
        receiver.gridObject.Move(currentCommand.path);
        currentCommand = null;
        clearUtility.ClearPathfinding();
        clearUtility.ClearGridHighlighter(0);
       
    }

    private void ExecuteAttackCommand()
    {
        Entity receiver = currentCommand.character;
        receiver.gridObject.Attack(currentCommand.selectedGridPoint,currentCommand.target);
        currentCommand = null;
        clearUtility.ClearGridHighlighter(1);
        
    }

}
