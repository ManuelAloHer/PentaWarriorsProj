using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        BattleStart,
        RoundStart,
        TurnStart,
        BattleEnd
    }

    public static BattleManager Instance;

    public BattleState currentState = BattleState.BattleStart;

    public List<Entity> allEntities = new List<Entity>();
    public List<Entity> playerEntities = new List<Entity>();
    public List<Entity> AIEntities = new List<Entity>();

    private Queue<Entity> turnQueue = new Queue<Entity>();
    public CommandInput playerInputController;
    public CommandAIInput enemyInputController;
    public Entity currentEntity;
    public Image charImage;
    public TMP_Text roundText;
    private int roundNumber = 1;


    private void Awake()
    {
        Instance = this;    
    }
    private void Start()
    {
        foreach (Entity entity in allEntities)
        {
            //Debug.Log(entity.CharacterName + "  " + entity.characterAliance);
            if (entity.characterAliance.Equals(Aliance.Player)) 
            {
                entity.AssignController(playerInputController);
                playerEntities.Add(entity);
            }
            else if (entity.characterAliance.Equals(Aliance.Enemy))
            {
                entity.AssignController(enemyInputController);
                AIEntities.Add(entity);
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
                SceneManager.LoadScene(0);
                break;
        }
    }

    private void StartBattle()
    {
        roundNumber = 0;
        currentState = BattleState.RoundStart;
        
    }

    private void StartRound()
    {
        //Debug.Log($"-- ROUND {roundNumber} --");
        roundNumber++;
        roundText.text = "Round " + roundNumber;
        // Only include alive entities
        var ordered = TurnOrderSystem.CalculateTurnOrder(
            allEntities.Where(e => e.IsAlive()).ToList()
        );

        turnQueue = new Queue<Entity>(ordered);
        currentState = BattleState.TurnStart;
    }

    private void BeginTurn()
    {
        bool itsOver = IsBattleOver();

        if (turnQueue.Count == 0 || itsOver)
        {
            Debug.Log("Turn ended");
            currentState = IsBattleOver() ? BattleState.BattleEnd : BattleState.RoundStart;
            return;
        }
        if (currentEntity != null && currentEntity.TurnEnded() == false) { return; }
        currentEntity = turnQueue.Dequeue();
        currentEntity.OnTurnEnded += HandleEntityEndTurn;
        //Debug.Log($"{currentEntity.CharacterName}'s turn starts. Has a controller? {currentEntity.Controller != null}");
        charImage.sprite = currentEntity.sprite;

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
        Debug.Log("Combat in place is: "+(playersAlive && enemiesAlive));
        return !playersAlive || !enemiesAlive;

    }

    public List<Entity> GetEntitiesByAliance(Aliance targetedAliance)
    {
        if (targetedAliance == Aliance.None) 
        {
            Debug.LogWarning("No Entity Aliance Choosen");
        }
        if (targetedAliance == Aliance.Player) { return playerEntities; }
        return AIEntities;
    }
}
