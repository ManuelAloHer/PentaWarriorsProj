using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using System.Linq;

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
    List<PathNode> path;

    List<Entity> currentAlies;
    List<Entity> currentFoes; 

    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] LayerMask entityLayerMask;
    [SerializeField] LayerMask obstacleLayers;
    [SerializeField] Pathfinding pathfinding;
    [SerializeField] GridMap grid;
    [SerializeField] ClearUtility clearUtility;

    public void SetCommandType(CommandType commandType)
    {
        readyCommand = commandType;
    }
    public void InitCommand()
    {
        controlChecker.CheckTransitableTerrain(selectedEntity.gridObject);
        Debug.Log("Setting Path");
       
    }

    void Awake()
    {
        commandManager = GetComponent<CommandManager>();
        controlChecker = GetComponent<ControlChecker>();
        aiExecutionState = AIActionState.WaitingForEnemyInput;
    }
    private void Start()
    {
        GetAllEnitities();
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
                    ChoseMoveOrAttack();
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
                    if (readyCommand == CommandType.Attack)
                    {
                        AttackCommandInput();
                        firstExecution = false;
                    }
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

    private void ChoseTarget()
    {
        throw new NotImplementedException();
    }
    private void GetAllEnitities()
    {
        currentAlies = BattleManager.Instance.GetEntitiesByAliance(Aliance.Enemy);
        currentFoes = BattleManager.Instance.GetEntitiesByAliance(Aliance.Player);
        
    }
    public List<Entity> GetAllPlayerEntities() 
    {
        return BattleManager.Instance.GetEntitiesByAliance(Aliance.Player);
    }

    private void ChoseMoveOrAttack()
    {

        targetedEntity = FindBestAttackTarget(selectedEntity);

        if (targetedEntity != null)
        {
            SetCommandType(CommandType.Attack);
            Debug.Log($"{selectedEntity.name} will ATTACK {targetedEntity.name}");
        }
        else
        {
            targetedEntity = FindClosestTarget(selectedEntity); // fallback
            SetCommandType(CommandType.Move);
            Debug.Log($"{selectedEntity.name} will MOVE toward {targetedEntity.name}");
        }

        InitCommand();

    }

    private void MoveCommandInput()
    {
        //Debug.Log("In to Move Command " + selectedEntity.CharacterName);
        //Vector3Int PlaceToMove = targetedEntity.gridObject.positionInGrid;
        //path = controlChecker.possibleNodes;
        //PlaceToMove = controlChecker.CheckForNodeNearestPointInPossibleNodes(PlaceToMove, selectedEntity.gridObject.positionInGrid);
        //path = pathfinding.FindPath(selectedEntity.gridObject.positionInGrid.x, selectedEntity.gridObject.positionInGrid.y, selectedEntity.gridObject.positionInGrid.z,
        //                    PlaceToMove.x, PlaceToMove.y, PlaceToMove.z);
        ////List<PathNode> path = controlChecker.GetPath(PlaceToMove);
        //if (path == null) { Debug.Log("Not functional path"); return; }
        //commandManager.AddMoveCommand(selectedEntity, PlaceToMove,  path);
        //CashAction();

        Vector3Int targetPos = targetedEntity.gridObject.positionInGrid;
        Vector3Int origin = selectedEntity.gridObject.positionInGrid;

        // 1. Generate all possible movement nodes
        controlChecker.CheckTransitableTerrain(selectedEntity.gridObject);
        List<PathNode> possiblePositions = controlChecker.possibleNodes;

        // 2. Find closest tile to target (as before)
        Vector3Int destination = controlChecker.CheckForNodeNearestPointInPossibleNodes(targetPos, origin);

        // 3. If a tile exists within attack range AND has LOS, prefer stopping there
        int attackRange = selectedEntity.AttackRange;

        foreach (PathNode pos in possiblePositions)
        {
            int dist = Mathf.Abs(pos.pos_x - targetPos.x) + Mathf.Abs(pos.pos_y - targetPos.y) + Mathf.Abs(pos.pos_z - targetPos.z);
            if (dist <= attackRange)
            {
                Vector3Int possibleDestiny = new Vector3Int(pos.pos_x, pos.pos_y, pos.pos_z);
                List<Vector3Int> visibleFromThisTile = controlChecker.CalculateSingleTargetAreaAtPosition(possibleDestiny, attackRange, selectedEntity, Aliance.Player);
                if (visibleFromThisTile.Contains(targetPos))
                {
                    Debug.Log($"Stopping early at {pos} — in range & LOS to target.");
                    destination = possibleDestiny;
                    break;
                }
            }
        }

        // 4. Path to chosen destination
        path = pathfinding.FindPath(origin.x, origin.y, origin.z, destination.x, destination.y, destination.z);
        if (path == null)
        {
            Debug.LogWarning($"AI {selectedEntity.name} couldn't path to {destination}");
            EndTurnCommandInput();
            return;
        }

        // 5. Execute move
        commandManager.AddMoveCommand(selectedEntity, destination, path);
        CashAction();
    }
    public void EndTurnCommandInput()
    {

        commandManager.AddFinishTurnCommand(selectedEntity);

    }
    private void AttackCommandInput()
    {
        if (targetedEntity == null || selectedEntity == null)
            return;

        Vector3Int targetPos = targetedEntity.gridObject.positionInGrid;

        // Verify still visible
        controlChecker.CalculateSingleTargetArea(selectedEntity, Aliance.Player);
        var visibleTargets = controlChecker.FilterLineOfSightTargets(selectedEntity, controlChecker.targetPositions, Aliance.Player);

        if (!visibleTargets.Contains(targetPos))
        {
            Debug.Log($"{selectedEntity.name} lost LOS to {targetedEntity.name}.");
            EndTurnCommandInput();
            return;
        }

        // Add the attack command
        commandManager.AddAttackCommand(selectedEntity, targetPos, targetedEntity.gridObject);
        CashAction();

    }
    private Entity FindBestAttackTarget(Entity attacker)
    {
        List<Entity> possibleTargets = currentAlies;
        controlChecker.CalculateSingleTargetArea(attacker, Aliance.Player);

        List<Vector3Int> inRange = controlChecker.targetPositions;
        List<Vector3Int> visibleTargets = controlChecker.FilterLineOfSightTargets(attacker, inRange, Aliance.Player);

        Entity bestTarget = null;
        float bestScore = float.MinValue;

        foreach (Vector3Int pos in visibleTargets)
        {
            ObjectInGrid targetObject = grid.GetPlacedObject(pos);
            if (targetObject == null) continue;

            Entity targetEntity = targetObject.GetEntity();
            if (targetEntity == null) continue;
            if (targetEntity.characterAliance == attacker.characterAliance) { continue; }
            float score = EvaluateTargetValue(attacker, targetEntity);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = targetEntity;
            }
        }

        return bestTarget;
    }

    private float EvaluateTargetValue(Entity attacker, Entity target)
    {
        // Simple scoring: prioritize lowest health or distance
        float healthScore = 1f - ((float)target.healthComponent.Health/ target.healthComponent.Health);
        float distanceScore = 1f / (Vector3Int.Distance(attacker.gridObject.positionInGrid, target.gridObject.positionInGrid) + 1);

        return healthScore + distanceScore * 0.5f;
    }
    private Entity FindClosestTarget(Entity entity)
    {
        return GetAllPlayerEntities().OrderBy(e => Vector3Int.Distance(e.gridObject.positionInGrid, entity.gridObject.positionInGrid))
            .FirstOrDefault();
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