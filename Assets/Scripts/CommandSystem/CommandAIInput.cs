using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandAIInput : MonoBehaviour, IController // This Class functions as a 
{
    private enum AIActionState
    {
        WaitingForEnemyInput,
        TakingTurn,
        FinalTurnTunning,
    }
    bool firstExecution = true;
    private float executionTimer = 1f;

    [SerializeField] AIActionState aiExecutionState;
    CommandManager commandManager;
    [SerializeField] ControlChecker controlChecker;

    public Entity selectedEntity;
    public Entity targetedEntity;
    public CommandType readyCommand;

    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] LayerMask entityLayerMask;
    [SerializeField] LayerMask obstacleLayers;
    [SerializeField] ClearUtility clearUtility;

    public void SetCommandType(CommandType commandType)
    {
        readyCommand = commandType;
    }
    public void InitCommand()
    {
        controlChecker.CheckTransitableTerrain(selectedEntity.gridObject);
    }

    void Awake()
    {
        commandManager = GetComponent<CommandManager>();
        controlChecker = GetComponent<ControlChecker>();
        aiExecutionState = AIActionState.WaitingForEnemyInput;
    }

    private void HighlightAttackArea()
    {
        controlChecker.CalculateSingleTargetArea(selectedEntity, Aliance.Player);
    }

    private void HighlightWalkableTerrain()
    {
        controlChecker.CheckTransitableTerrain(selectedEntity.gridObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedEntity == null) { return; }
        switch (aiExecutionState)
        {
            case AIActionState.WaitingForEnemyInput:
                if (firstExecution) 
                {
                    controlChecker.SetPossibleNodesToNull();
                    SetCommandType(CommandType.Move);
                    InitCommand();
                    firstExecution = false;
                }
                if (controlChecker.possibleNodes!= null)
                {
                    firstExecution = true;
                    aiExecutionState = AIActionState.TakingTurn;
                }
                break;
            case AIActionState.TakingTurn:
                if (firstExecution)
                {
                    if (readyCommand == CommandType.Move)
                    {
                        MoveCommandInput();
                        firstExecution= false;
                    }
                }
                aiExecutionState=AIActionState.FinalTurnTunning;
                //executionTimer -= Time.deltaTime;
                //if (executionTimer <= 0f)
                //{
  
                //}
                break;
            case AIActionState.FinalTurnTunning:
                if (!selectedEntity.IsBusy) 
                {
                    firstExecution = true;
                    aiExecutionState = AIActionState.WaitingForEnemyInput;

                }
                break;
        }
        
        //if (readyCommand == CommandType.None) 
        //{
        //    EndTurnCommandInput();
        //    //ObjectiveSelection
        //    //SetCommandType(CommandType.Move);
        //    //InitCommand();
        //}

    }

    private void MoveCommandInput()
    {
        Vector3Int PlaceToMove = targetedEntity.gridObject.positionInGrid;
        PlaceToMove = controlChecker.CheckForNodeNearestPointInPossibleNodes(PlaceToMove, selectedEntity.gridObject.positionInGrid);
        List<PathNode> path = controlChecker.GetPath(PlaceToMove);
        if (path == null) { return; }
        commandManager.AddMoveCommand(selectedEntity, PlaceToMove,  path);
        CashAction();
    }
    public void EndTurnCommandInput()
    {

        commandManager.AddFinishTurnCommand(selectedEntity);

    }
    private void AttackCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
                    if (selectedEntity == null) { return; }
                    //ObjectInGrid gridTarget = controlChecker.GetTarget(inputCursor.PosOnGrid);
                    //if (gridTarget == null || gridTarget.GetAliance() == selectedEntity.gridObject.GetAliance()) { return; }
                    //commandManager.AddAttackCommand(selectedEntity, inputCursor.PosOnGrid, gridTarget);
                    CashAction();
        }

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
        Debug.Log($"{entity.CharacterName}'s turn ends.");
        selectedEntity = null;
        //clearUtility.ClearAllHighLighters();
        // Disable input or cleanup
    }

    public bool IsAI()
    {
        return true;
    }
}