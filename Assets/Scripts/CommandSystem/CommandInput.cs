using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] ControlChecker controlChecker;

    CharacterSelector characterSelector;

    public CommandType readyCommand;
    public CommandInputType currentMenuCommand;

    public bool cursorNeeded;
    public int activeNodeAliance = 0;
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
        controlChecker = GetComponent<ControlChecker>();
    }

    private void Start()
    {
        highlightActions = new Dictionary<CommandType, Action>
        {
            { CommandType.Move, () => HighlightWalkableTerrain() },
            { CommandType.Attack, () => HighlightAttackArea()},
            { CommandType.AtkOnArea, () => HighlightAtkOnArea()}
        };
    }

    private void HighlightAttackArea()
    {
        controlChecker.CalculateSingleTargetArea(characterSelector.selectedEntity, Aliance.Enemy);
    }

    private void HighlightAtkOnArea()
    {
        controlChecker.CalculateMultipleTargetArea(characterSelector.selectedEntity, Aliance.None);
    }

    private void HighlightWalkableTerrain()
    {
        controlChecker.CheckTransitableTerrain(characterSelector.selectedEntity.gridObject);
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
                List<PathNode> path = controlChecker.GetPath(inputCursor.PosOnGrid);
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
            ChangePositionOnGridMonitor(hit, true);
            if (inputCursor.IsConfirmPressed() && characterSelector.selectedEntity != null)
            {
                if (controlChecker.CheckPosibleAttack(inputCursor.PosOnGrid))
                {
                    Debug.Log("Aqui llego");
                    if (characterSelector.selectedEntity == null) { return; }
                    ObjectInGrid gridTarget = controlChecker.GetTarget(inputCursor.PosOnGrid);
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
                if (controlChecker.CheckPosibleAttack(inputCursor.PosOnGrid))
                {
                    if (characterSelector.selectedEntity == null) { return; }
                    GridNode gridTarget = controlChecker.GetTargetNode(inputCursor.PosOnGrid);
                   
                    if (gridTarget == null || gridTarget.objectInGrid.GetAliance() == characterSelector.selectedEntity.gridObject.GetAliance()) { return; }
                    Debug.Log("Has Entered");

                    //Get all Entities in x Range
                    List<ObjectInGrid> targets = controlChecker.MultipleTargetSelected(characterSelector.selectedEntity, 
                                                 inputCursor.PosOnGrid, 
                                                 characterSelector.selectedEntity.characterAliance);

                    //
                    commandManager.AddAttackOnAreaCommand(characterSelector.selectedEntity, inputCursor.PosOnGrid, targets);
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
        Vector3Int gridPosition = controlChecker.targetGrid.GetGridPosition(hit.point);
        GridNode activeNode = controlChecker.targetGrid.GetNode(gridPosition);
        showSpecialHighlight = showAdE;
        if (gridPosition != inputCursor.PosOnGrid && activeNode != null)
        {
            inputCursor.SetPosOnGrid = gridPosition;
            if (activeNode.entityOcupied == false) 
            {
                activeNodeAliance = 0;
                return true;
            }
            activeNodeAliance = (int)activeNode.objectInGrid.GetAliance();
            return true;
        }
        return false;
    }
    private void CashAction()
    {
        if(readyCommand == CommandType.None || currentMenuCommand == CommandInputType.None) { return; }
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
    public bool IsAI() 
    { 
        return false;
    }

}
