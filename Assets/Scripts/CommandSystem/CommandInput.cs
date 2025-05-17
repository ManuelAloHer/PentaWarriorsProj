using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Search;
using UnityEngine;

[System.Serializable]
public class InputToCommandMap
{
    public CommandInputType inputType;
    public CommandType commandType;
}
public class CommandInput : MonoBehaviour,IController // This Class functions as a 
{
    CommandManager commandManager;
    InputController inputCursor;
    [SerializeField] PlayerControlChecker playerControl;

    CharacterSelector characterSelector;

    public CommandType readyCommand;
    public CommandInputType currentMenuCommand;

    public bool cursorNeeded;
    public bool showSpecialHighlight = false;

    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] LayerMask entityLayerMask;
    private Dictionary<CommandType, Action> highlightActions;

    public void InitCommand() 
    {
        if (highlightActions.TryGetValue(readyCommand, out var highlightAction))
        {
            if (readyCommand == CommandType.None) { return; }
            highlightAction.Invoke();
        }
    }
    public void InitEndTurnCommand()
    {
        EndTurnCommandInput();
        readyCommand = CommandType.None;
    }
    void Awake()
    {
        commandManager = GetComponent<CommandManager>();
        characterSelector = GetComponent<CharacterSelector>();
        inputCursor = GetComponent<InputController>();
        playerControl = GetComponent<PlayerControlChecker>();
    }

    private void Start()
    {
        highlightActions = new Dictionary<CommandType, Action>
        {
            { CommandType.Move, () => HighlightWalkableTerrain() },
            { CommandType.Attack, () => HighlightAttackArea()}
        };
    }

    private void HighlightAttackArea()
    {
        playerControl.CalculateSingleTargetArea(characterSelector.selectedEntity, false);
    }

    private void HighlightWalkableTerrain()
    {
        playerControl.CheckTransitableTerrain(characterSelector.selectedEntity.gridObject);
    }

    // Update is called once per frame
    void Update()
    {
        
        Ray ray = Camera.main.ScreenPointToRay(inputCursor.GetCursorPosition());
        RaycastHit hit;
        if (InputController.IsPointerOverUI()) { return; }

        if (readyCommand == CommandType.Move)
        {
            MoveCommandInput(ray, out hit);
        }
        else if (readyCommand == CommandType.Attack)
        {
            AttackCommandInput(ray, out hit);
        }
        else if (readyCommand == CommandType.AtkOnArea) 
        {
            AttackOnAreaCommandInput(ray, out hit);
        }
        else
        {
            if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                cursorNeeded = true;
                ChangePositionOnGridMonitor(hit,false);
            }
        }

    }

    private void MoveCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            cursorNeeded = true;
            ChangePositionOnGridMonitor(hit, false);
            if (inputCursor.IsConfirmPressed() && characterSelector.selectedEntity != null) 
            {
                List<PathNode> path = playerControl.GetPath(inputCursor.PosOnGrid);
                if (path == null) { return; }
                commandManager.AddMoveCommand(characterSelector.selectedEntity, inputCursor.PosOnGrid, path);
                CashAction();
            }
            else if (inputCursor.IsCancelPressed() && characterSelector.selectedEntity != null && characterSelector.selectedEntity.gridObject.movement.IsMoving)
            {
                //characterSelector.selectedEntity.gridObject.SkipMovementAnimation();
            }

        }
        else
        {
            cursorNeeded = false;
        }
    }

    private void AttackCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            cursorNeeded = true;
            ChangePositionOnGridMonitor(hit, false);
            if (inputCursor.IsConfirmPressed() && characterSelector.selectedEntity != null)
            {
                if (playerControl.CheckPosibleAttack(inputCursor.PosOnGrid))
                {
                    if (characterSelector.selectedEntity == null) { return; }
                    ObjectInGrid gridTarget = playerControl.GetTarget(inputCursor.PosOnGrid);
                    if (gridTarget == null || gridTarget.GetAliance() == characterSelector.selectedEntity.gridObject.GetAliance()) { return; }
                    commandManager.AddAttackCommand(characterSelector.selectedEntity, inputCursor.PosOnGrid, gridTarget);
                    CashAction();
                }
            }
        }
        else
        {
            cursorNeeded = false;
        }
    }
    private void AttackOnAreaCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            cursorNeeded = true;
            ChangePositionOnGridMonitor(hit,true);
            if (inputCursor.IsConfirmPressed() && characterSelector.selectedEntity != null)
            {
                if (playerControl.CheckPosibleAttack(inputCursor.PosOnGrid))
                {
                    if (characterSelector.selectedEntity == null) { return; }
                    ObjectInGrid gridTarget = playerControl.GetTarget(inputCursor.PosOnGrid);
                    if (gridTarget == null || gridTarget.GetAliance() == characterSelector.selectedEntity.gridObject.GetAliance()) { return; }
                    commandManager.AddAttackOnAreaCommand(characterSelector.selectedEntity, inputCursor.PosOnGrid, gridTarget);
                    CashAction();
                }
            }
        }
        else
        {
            cursorNeeded = false;
        }
    }
    public void EndTurnCommandInput()
    {
        
        commandManager.AddFinishTurnCommand(characterSelector.selectedEntity);
        
    }
    private bool ChangePositionOnGridMonitor(RaycastHit hit, bool showAdE)
    {
        cursorNeeded = true;
        Vector3Int gridPosition = playerControl.targetGrid.GetGridPosition(hit.point);
        showSpecialHighlight = showAdE;
        if (gridPosition != inputCursor.PosOnGrid)
        {
            inputCursor.SetPosOnGrid = gridPosition;
            return true;
        }
        return false;
    }
    private void CashAction()
    {
        if(readyCommand == CommandType.None || currentMenuCommand == CommandInputType.None) { return; }
        Debug.Log("ConsumedAction");
        readyCommand = CommandType.None;
        currentMenuCommand = CommandInputType.None;
        //characterSelector.selectedEntity.ConsumeActions(false);
        //substract form current actions
        // if current actions <=0
        //deselectcharacter
    }
    public void BeginTurn(Entity entity)
    {
        characterSelector.SelectCharacter(entity);
        Debug.Log($"{entity.CharacterName} is now player-controlled.");
        // Enable input, show UI, etc.
    }

    public void EndTurn(Entity entity)
    {
        if (entity == null) 
        {
            Debug.LogWarning("No Entity");
        }
        Debug.Log($"{entity.CharacterName}'s player turn ends.");
        characterSelector.UnselectCharacter();
        // Disable input or cleanup
    }


}
