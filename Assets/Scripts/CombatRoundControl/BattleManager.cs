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
        BattleIntro,      // New
        BattleStart,
        RoundStart,
        TurnStart,
        BattleEnd,
        BattleConclusion  // New
    }

    public static BattleManager Instance;
    public bool timeForBatlle = false;
    [SerializeField] BatlleUIManager uIManager;
    [SerializeField] Entity[] upcoming;

    public BattleState currentState = BattleState.BattleIntro;

    public List<Entity> allEntities = new List<Entity>();
    public List<Entity> playerEntities = new List<Entity>();
    public List<Entity> AIEntities = new List<Entity>();

    public Queue<Entity> turnQueue = new Queue<Entity>();
    public CommandInput playerInputController;
    public CommandAIInput enemyInputController;
    public Entity currentEntity;
    public Image[] charImages;
    public TMP_Text roundText;
    private int roundNumber = 1;

    private bool roundInProgress = false;



    private void Awake()
    {
        Instance = this;
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
    private void Start()
    {
        uIManager.HideBatlleRelevantUI();
        currentState = BattleState.BattleIntro;
    }
    void Update()
    {
        switch (currentState)
        {
            case BattleState.BattleIntro:
                StartCoroutine(HandleBattleIntro());
                break;
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
                uIManager.HideBatlleRelevantUI();
                StartCoroutine(HandleBattleConclusion());
                currentState = BattleState.BattleConclusion;
                break;

            case BattleState.BattleConclusion:
                // Wait for coroutine to handle it
                break;
        }
    }
    private IEnumerator HandleBattleIntro()
    {
        uIManager.SetBatlleStartText();
        // Possible delay or cinematic step
        yield return new WaitForSeconds(0.5f);
        uIManager.ShowBatlleRelevantUI();
        currentState = BattleState.BattleStart;
    }
    private IEnumerator HandleBattleConclusion()
    {
        bool playerWon = playerEntities.Any(e => e.IsAlive());
        bool enemiesWon = AIEntities.Any(e => e.IsAlive());
        timeForBatlle = false;
        if (playerWon)
        {
            uIManager.SetBatlleEndText(playerWon);
            // Show victory screen
        }
        else if (enemiesWon)
        {
            uIManager.SetBatlleEndText(!enemiesWon);
            // Show defeat screen
        }

        // Example wait or screen fade
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(0); // Or a win/lose scene
    }
    private void StartBattle()
    {
        roundNumber = 0;
        currentState = BattleState.RoundStart;
        
    }

    private void StartRound()
    {
        Debug.Log(" BeginTurn Queue Peek:");
        if (turnQueue.Count > 0)
            Debug.Log($" - Next: {turnQueue.Peek().CharacterName} (Init: {turnQueue.Peek().initiative})");
        //Debug.Log($"-- ROUND {roundNumber} --");
        if (roundInProgress) return; // Prevent multiple calls in same frame
        roundInProgress = true;
        StartCoroutine(DelayFirstTurn());
    }

    private IEnumerator DelayFirstTurn()
    {
        
        // Only include alive entities
        var ordered = TurnOrderSystem.CalculateTurnOrder(
        allEntities.Where(e => e.IsAlive()).ToList());
        turnQueue = new Queue<Entity>(ordered);
        yield return new WaitForSeconds(1.5f);
        roundNumber++;
        roundText.text = "Round " + roundNumber;
        timeForBatlle = true;
        roundInProgress = false;
        uIManager.HideBatlleText();
        upcoming = turnQueue.ToArray();
        currentState = BattleState.TurnStart;
    }
    private void BeginTurn()
    {
        bool itsOver = IsBattleOver();
        if (turnQueue.Count == 0 || itsOver)
        {
            Debug.Log("Turn ended");
            roundInProgress = false;
            currentState = IsBattleOver() ? BattleState.BattleEnd : BattleState.RoundStart;
            return;
        }
        if (currentEntity != null && currentEntity.TurnEnded() == false) 
        {
            return; 
        
        }
        Debug.Log($"BEGIN TURN: Queue Count = {turnQueue.Count}");

        if (turnQueue.Count > 0)
        {
            Debug.Log($" - Next: {turnQueue.Peek().CharacterName} (Init: {turnQueue.Peek().initiative})");
            Debug.Log($"Next Entity: {turnQueue.Peek().CharacterName} (Init: {turnQueue.Peek().initiative})");
        }
        upcoming = turnQueue.ToArray();
        for (int i = 0; i < charImages.Length; i++)
        {
            if (i < upcoming.Length)
            {
                charImages[i].sprite = upcoming[i].sprite;
            }
            else
            {
                charImages[i].sprite = uIManager.defaultCharSprite; // Hide empty slots
            }
        }

        currentEntity = turnQueue.Dequeue();
        currentEntity.OnTurnEnded += HandleEntityEndTurn;
        //Debug.Log($"{currentEntity.CharacterName}'s turn starts. Has a controller? {currentEntity.Controller != null}");
        

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
