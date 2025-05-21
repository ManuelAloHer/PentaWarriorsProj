using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


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
    public IActionEffect effect;
    //public ICommandInputStage inputStage;

    //Specific variables of one or more Commands
    public List<PathNode> path = null;
    public List<ObjectInGrid> target = new List<ObjectInGrid>();

    public Command(Entity character, Vector3Int selectedGridPoint, CommandType commandType, IActionEffect effect)
                    // IActionEffect effect, ICommandInputStage inputStage = null)
    {
        this.character = character;
        this.selectedGridPoint = selectedGridPoint;
        this.commandType = commandType;
        this.effect = effect;
        //this.inputStage = inputStage;
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
        currentCommand = new Command(character, selectedGridPoint, CommandType.Move, character.gridObject.movement);
        currentCommand.path = path;
    }
    public void AddAttackCommand(Entity attacker, Vector3Int selectedGridPoint, ObjectInGrid target)// List<PathNode> path) // It could be necesary to calculate a path for the proyectile if we go that far
    {
        currentCommand = new Command(attacker, selectedGridPoint, CommandType.Attack,null);
        currentCommand.target.Add(target);
        //currentCommand.path = path;
    }
    public void AddAttackOnAreaCommand(Entity attacker, Vector3Int selectedGridPoint, List<ObjectInGrid> targets)
    {
        currentCommand = new Command(attacker, selectedGridPoint, CommandType.AtkOnArea, null);
        currentCommand.target = targets;

    }
    public void AddFinishTurnCommand(Entity character)
    {
        Vector3Int charactPos = character.gridObject.positionInGrid;
        currentCommand = new Command(character, charactPos, CommandType.EndTurn, null);
    }
    public void AddCommand(Entity character, Vector3Int selectedGridPoint, CommandType commandType)
    {
        currentCommand = new Command(character, selectedGridPoint, commandType, null);
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
        Entity caster = currentCommand.character;
        caster.SetIsBusy(true);
        caster.gridObject.Move(currentCommand.path);
        currentCommand.effect.Play(caster.ConsumeNormalAction);
        currentCommand = null;
        clearUtility.ClearPathfinding();
        clearUtility.ClearGridHighlighter(0);

    }

    private void ExecuteAttackCommand()
    {
        Entity caster = currentCommand.character;
        caster.gridObject.Attack(currentCommand.selectedGridPoint, currentCommand.target[0]);
        caster.ConsumeActions(false);
        currentCommand = null;
        clearUtility.ClearGridHighlighter(1);

    }
    private void ExecuteAttackOnAreaCommand()
    {
        Entity receiver = currentCommand.character;
        foreach (ObjectInGrid target in currentCommand.target) 
        {
            Entity victim = target.GetEntity();
            if (victim != null) { Debug.Log(victim.CharacterName + " is afected"); }

        }
        //receiver notifies gridObject the AdE Action
        receiver.ConsumeActions(false);
        currentCommand = null;
        // clearUtility.Clear the correspondingHighlighters

    }
    private void ExecuteAddStatusCommand()
    {
        Entity receiver = currentCommand.character;
        Entity targetEntity = currentCommand.target[0].GetEntity();
        //AddStatus in Entity     receiver.gridObject.Attack(currentCommand.selectedGridPoint, currentCommand.target);
        receiver.ConsumeActions(false);
        currentCommand = null;
        // clearUtility.Clear the correspondingHighlighters
    }
    private void ExecuteEndStatusCommand()
    {
        Entity receiver = currentCommand.character;
        Entity targetEntity = currentCommand.target[0].GetEntity();
        //EndAllStatus in Entity     receiver.gridObject.Attack(currentCommand.selectedGridPoint, currentCommand.target);
        receiver.ConsumeActions(false);
        currentCommand = null;
        // clearUtility.Clear the correspondingHighlighters

    }
}
