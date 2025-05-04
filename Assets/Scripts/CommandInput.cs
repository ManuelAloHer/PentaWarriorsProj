using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ReadyCommand { None, Move, Attack }
public class CommandInput : MonoBehaviour
{
    CommandManager commandManager;
    InputController inputCursor;
    [SerializeField] PlayerControl playerControl;

    [SerializeField] Entity selectedEntity;
    [SerializeField] ReadyCommand readyCommand;

    public bool cursorNeeded;

    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] LayerMask entityLayerMask;
    private Vector3Int positionOnGrid;
    public Vector3Int PosOnGrid { get { return positionOnGrid; } }

    void Awake()
    {
        commandManager = GetComponent<CommandManager>();
        inputCursor = GetComponent<InputController>();
        playerControl = GetComponent<PlayerControl>();
    }

    private void Start()
    {
        //HighlightWalkableTerrain();
        playerControl.CalculateAttackArea(selectedEntity, false);
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

        if (readyCommand == ReadyCommand.Move)
        {
            //MoveCommandInput(ray, out hit);
        }
        else if (readyCommand == ReadyCommand.Attack)
        {
            AttackCommandInput(ray, out hit);
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
                List<PathNode> path = playerControl.GetPath(positionOnGrid);
                if (path == null) { return; }
                commandManager.AddMoveCommand(selectedEntity, positionOnGrid, path);
            }
            else if (inputCursor.IsCancelPressed() && selectedEntity != null && selectedEntity.gridObject.movement.IsMoving)
            {
                selectedEntity.gridObject.SkipMovementAnimation();
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
                if (playerControl.CheckPosibleAttack(positionOnGrid))
                {
                    ObjectInGrid gridTarget = playerControl.GetTarget(positionOnGrid);
                    if (gridTarget == null || gridTarget.GetAliance() == selectedEntity.gridObject.GetAliance()) { return; }
                    commandManager.AddAttackCommand(selectedEntity,positionOnGrid, gridTarget);
                    // Rest of AttackBehaviour

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
        if (gridPosition != positionOnGrid)
        {
            positionOnGrid = gridPosition;
            return true;
        }
        return false;
    }
    public void ChangeControlState(ReadyCommand newCommand)
    {
        if (readyCommand == newCommand) { readyCommand = ReadyCommand.None; }
        else
        {
            readyCommand = newCommand;

        }
    }
    public void MoveButtonControlState()
    {
        ChangeControlState(ReadyCommand.Move);
    }
    public void AttackButtonControlState()
    {
        ChangeControlState(ReadyCommand.Attack);
    }
}
