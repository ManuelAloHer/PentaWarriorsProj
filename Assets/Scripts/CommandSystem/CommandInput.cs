using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputToCommandMap
{
    public CommandInputType inputType;
    public CommandType commandType;
    public int concentrationCost;
    public Sprite standard, pressed, deactivated;
    public string description;
}
public class CommandInput : MonoBehaviour,IController // This Class functions as a 
{
    CommandManager commandManager;
    BattleManager battleManager;
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
        battleManager = GetComponent<BattleManager>();
    }

    private void Start()
    {
        highlightActions = new Dictionary<CommandType, Action>
        {
            { CommandType.Move, () => HighlightWalkableTerrain() },
            { CommandType.Attack, () => HighlightAttackArea()},
            { CommandType.AtkOnArea, () => HighlightAtkOnArea()},
            { CommandType.Heal, () => HighlightHeal()},
        };
    }

    private void HighlightAttackArea()
    {
        controlChecker.CalculateSingleTargetArea(characterSelector.selectedEntity, Aliance.Enemy);
    }

    private void HighlightHeal()
    {
        controlChecker.CalculateSingleHeal(characterSelector.selectedEntity, Aliance.Player);
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
        if (!battleManager.timeForBatlle) { characterSelector.selectedEntity = null;  }
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
        else if (readyCommand == CommandType.Heal)
        {
            HealCommandInput(ray, out hit);
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
            ChangePositionOnGridMonitor(hit, false);
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
    private void HealCommandInput(Ray ray, out RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            cursorNeeded = true;
            ChangePositionOnGridMonitor(hit, false);
            if (inputCursor.IsConfirmPressed() && characterSelector.selectedEntity != null)
            {
                if (controlChecker.CheckPosibleAttack(inputCursor.PosOnGrid))
                {
                    
                    if (characterSelector.selectedEntity == null) { return; }
                    ObjectInGrid gridTarget = controlChecker.GetTarget(inputCursor.PosOnGrid);
                    if (gridTarget != null && gridTarget.GetAliance() == characterSelector.selectedEntity.gridObject.GetAliance()) 
                    {
                        
                        SpecialHability specialHability = TranslateMenuCommandToSpecialHab();
                        characterSelector.selectedEntity.SubstractConcentrationFromUse((int)specialHability);
                        commandManager.AddHealCommand(characterSelector.selectedEntity, inputCursor.PosOnGrid, gridTarget, specialHability);
                        CashAction();
                        return;
                    }

                    Debug.Log("Algo falla"); 
                    return;
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

                    //Get all Entities in x Range
                    List<ObjectInGrid> targets = controlChecker.MultipleTargetSelected(characterSelector.selectedEntity,
                                                 inputCursor.PosOnGrid,
                                                 characterSelector.selectedEntity.characterAliance);
                    if (targets != null && targets.Count > 0)
                    {
                        SpecialHability specialHability = TranslateMenuCommandToSpecialHab();
                        characterSelector.selectedEntity.SubstractConcentrationFromUse((int)specialHability);
                        commandManager.AddAttackOnAreaCommand(characterSelector.selectedEntity, inputCursor.PosOnGrid, targets, specialHability);
                        showSpecialHighlight = false;
                        CashAction();
                        return;
                    }
                    else
                    {
                        showSpecialHighlight = false;
                        Debug.Log("No Valid Targets");
                    }
                }
            }
        }
        else
        {
            showSpecialHighlight = false;
            cursorNeeded = false;
            
        }
    }

    private SpecialHability TranslateMenuCommandToSpecialHab()
    {
        //public enum SpecialHability { None = 0, Hab1 = 1, Hab2 = 2, Hab3 = 3 }
        //public enum CommandInputType { None = -1, Move = 0, Attack = 1, SpeHab1 = 2, SpeHab2 = 3, SpeHab3 = 4, EndTurn = 5, CardAction = 6 }
        
        int index = (int)currentMenuCommand;
        SpecialHability specialHab = SpecialHability.None;
        if (index >= 2 && index <= 4)
        {
            specialHab = index == 2 ? SpecialHability.Hab1 : index == 3 ? SpecialHability.Hab2 : SpecialHability.Hab3;    
        }
        return specialHab;
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
