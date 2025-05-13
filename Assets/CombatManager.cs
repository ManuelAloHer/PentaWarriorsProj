using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        BattleStart,
        RoundStart,
        TurnStart,
        BattleEnd
    }

    public BattleState currentState = BattleState.BattleStart;

    public List<Entity> allEntities = new List<Entity>();
    private Queue<Entity> turnQueue = new Queue<Entity>();
    public CommandInput playerInputController;
    public Entity currentEntity;
    public Image charImage;
    private int roundNumber = 1;

    private void Awake()
    {
        foreach (Entity entity in allEntities)
        {
            if (entity.characterAliance.Equals(Aliance.Player)) 
            {
                entity.AssignController(playerInputController);
            }
        
        }
    }
    void Update()
    {
        switch (currentState)
        {
            case BattleState.BattleStart:
                StartBattle();
                break;

            case BattleState.RoundStart:
                StartRound();
                break;

            case BattleState.TurnStart:
                BeginTurn();
                break;

            case BattleState.BattleEnd:
                Debug.Log("Battle ended.");
                break;
        }
    }

    private void StartBattle()
    {
        Debug.Log("Battle begins!");
        currentState = BattleState.RoundStart;
    }

    private void StartRound()
    {
        //Debug.Log($"-- ROUND {roundNumber} --");
        roundNumber++;
        var ordered = TurnOrderSystem.CalculateTurnOrder(allEntities);
        turnQueue = new Queue<Entity>(ordered);
        currentState = BattleState.TurnStart;
    }

    private void BeginTurn()
    {
        
        if (turnQueue.Count == 0)
        {
            currentState = IsBattleOver() ? BattleState.BattleEnd : BattleState.RoundStart;
            return;
        }
        if (currentEntity != null && currentEntity.TurnEnded() == false) { return; }
        currentEntity = turnQueue.Dequeue();
        currentEntity.OnTurnEnded += HandleEntityEndTurn;
        Debug.Log($"{currentEntity.CharacterName}'s turn starts.");
        charImage.color = currentEntity.color;
        //Set UI Turn
        currentEntity.StartTurn();  

    }

    public void HandleEntityEndTurn(Entity entity)
    {
        Debug.Log($"{entity.CharacterName}'s turn ends.");
        entity.OnTurnEnded -= HandleEntityEndTurn;
        currentState = BattleState.TurnStart;
    }

    private bool IsBattleOver()
    {
        bool playersAlive = allEntities.Any(e => e.PlayerCharacter == true && e.IsAlive());
        bool enemiesAlive = allEntities.Any(e => e.characterAliance.Equals(Aliance.Enemy) && e.PlayerCharacter != true && e.IsAlive());
        return !(playersAlive && enemiesAlive);
    }
    //public List<Entity> allEntities = new List<Entity>();
    //private Queue<Entity> turnQueue = new Queue<Entity>();

    //private int roundNumber = 1;

    //private void Start()
    //{
    //    StartCoroutine(BattleLoop());
    //}

    //private IEnumerator BattleLoop()
    //{
    //    while (!IsBattleOver())
    //    {
    //        Debug.Log($"-- ROUND {roundNumber} --");
    //        CalculateInitiative();
    //        yield return StartCoroutine(RunRound());
    //        roundNumber++;
    //    }

    //    Debug.Log("Battle Over!");
    //}

    //private void CalculateInitiative()
    //{
    //    foreach (var entity in allEntities)
    //    {
    //        entity.CalculateInitiative();
    //    }

    //    var sorted = allEntities.OrderByDescending(e => e.initiative);
    //    turnQueue = new Queue<Entity>(sorted);
    //}

    //private IEnumerator RunRound()
    //{
    //    while (turnQueue.Count > 0)
    //    {
    //        Entity current = turnQueue.Dequeue();
    //        Debug.Log($"{current.CharacterName}'s turn!");
    //        yield return StartCoroutine(current.TakeTurn());
    //    }
    //}

    //private bool IsBattleOver()
    //{
    //    // Replace with your own win/loss condition
    //    bool playersAlive = allEntities.Any(e => e.characterAliance == Aliance.Player);
    //    bool enemiesAlive = allEntities.Any(e => !e.characterAliance == Aliance.Enemy);
    //    return !(playersAlive && enemiesAlive);
    //}
}
