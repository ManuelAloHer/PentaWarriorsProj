using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class CommandInput : MonoBehaviour
{
    CommandManager commandManager;
    InputController inputCursor;
    [SerializeField] PlayerControl playerControl;

    CharacterSelector characterSelector;
    //[SerializeField] Entity selectedEntity;
    public CommandType readyCommand;

    public bool cursorNeeded;

    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] LayerMask entityLayerMask;
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
        characterSelector = GetComponent<CharacterSelector>();
        inputCursor = GetComponent<InputController>();
        playerControl = GetComponent<PlayerControl>();
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
        playerControl.CalculateAttackArea(characterSelector.selectedEntity, false);
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
                cursorNeeded = true;
                ChangePositionOnGridMonitor(hit);
            }
        }

    }

    private void MoveCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            cursorNeeded = true;
            ChangePositionOnGridMonitor(hit);
            if (inputCursor.IsConfirmPressed())
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
            ChangePositionOnGridMonitor(hit);
            if (inputCursor.IsConfirmPressed())
            {
                if (playerControl.CheckPosibleAttack(inputCursor.PosOnGrid))
                {
                    ObjectInGrid gridTarget = playerControl.GetTarget(inputCursor.PosOnGrid);
                    if (gridTarget == null || gridTarget.GetAliance() == characterSelector.selectedEntity.gridObject.GetAliance()) { return; }
                    commandManager.AddAttackCommand(characterSelector.selectedEntity, inputCursor.PosOnGrid, gridTarget);
                    // Rest of AttackBehaviour
                    CashAction();
                }
            }
        }
        else
        {
            cursorNeeded = false;
        }
    }

    private bool ChangePositionOnGridMonitor(RaycastHit hit)
    {
        cursorNeeded = true;
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
        characterSelector.Deselect();    
        //substract form current actions
        // if current actions <=0
        //deselectcharacter
    }

}
