using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class CommandAIInput : MonoBehaviour, IController // This Class functions as a 
{
    CommandManager commandManager;
    InputController inputCursor;
    [SerializeField] PlayerControlChecker playerControl;

    public Entity selectedEntity;
    public CommandType readyCommand;

    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] LayerMask entityLayerMask;
    [SerializeField] LayerMask obstacleLayers;
    private Dictionary<CommandType, Action> highlightActions;

    public void SetCommandType(CommandType commandType)
    {
        readyCommand = commandType;
    }
    public void InitCommand()
    {
        if (highlightActions.TryGetValue(readyCommand, out var highlightAction))
        {
            if (readyCommand == CommandType.None) { return; }
            highlightAction.Invoke();
        }
    }

    void Awake()
    {
        commandManager = GetComponent<CommandManager>();
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
        playerControl.CalculateSingleTargetArea(selectedEntity, false);
    }

    private void HighlightWalkableTerrain()
    {
        playerControl.CheckTransitableTerrain(selectedEntity.gridObject);
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
        else
        {
            if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                ChangePositionOnGridMonitor(hit);
            }
        }

    }

    private void MoveCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            ChangePositionOnGridMonitor(hit);
            if (inputCursor.IsConfirmPressed() && selectedEntity != null)
            {
                List<PathNode> path = playerControl.GetPath(inputCursor.PosOnGrid);
                if (path == null) { return; }
                commandManager.AddMoveCommand(selectedEntity, inputCursor.PosOnGrid, path);
                CashAction();
            }
            else if (inputCursor.IsCancelPressed() && selectedEntity != null && selectedEntity.gridObject.movement.IsMoving)
            {
                //characterSelector.selectedEntity.gridObject.SkipMovementAnimation();
            }

        }
    }

    private void AttackCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            ChangePositionOnGridMonitor(hit);
            if (inputCursor.IsConfirmPressed() && selectedEntity != null)
            {
                if (playerControl.CheckPosibleAttack(inputCursor.PosOnGrid))
                {
                    if (selectedEntity == null) { return; }
                    ObjectInGrid gridTarget = playerControl.GetTarget(inputCursor.PosOnGrid);
                    if (gridTarget == null || gridTarget.GetAliance() == selectedEntity.gridObject.GetAliance()) { return; }
                    commandManager.AddAttackCommand(selectedEntity, inputCursor.PosOnGrid, gridTarget);
                    CashAction();
                }
            }
        }

    }

    private bool ChangePositionOnGridMonitor(RaycastHit hit)
    {
        Vector3Int gridPosition = playerControl.targetGrid.GetGridPosition(hit.point);
        if (gridPosition != inputCursor.PosOnGrid)
        {
            inputCursor.SetPosOnGrid = gridPosition;
            return true;
        }
        return false;
    }
    private void CashAction()
    {
        if (readyCommand == CommandType.None) { return; }
        Debug.Log("ConsumedAction");
        readyCommand = CommandType.None;
    }
    public void BeginTurn(Entity entity)
    {
        selectedEntity = entity;
        Debug.Log($"{entity.CharacterName} is now AI_controlled.");
        // Enable input, show UI, etc.
    }

    public void EndTurn(Entity entity)
    {
        Debug.Log($"{entity.CharacterName}'s player turn ends.");
        selectedEntity = null;
        // Disable input or cleanup
    }

}