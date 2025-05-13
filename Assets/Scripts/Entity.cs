using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public enum Aliance {None,Player,Enemy}

[RequireComponent(typeof(ObjectInGrid), typeof(HealthComponent), typeof(EntityTurn))]
public class Entity : MonoBehaviour
{
    [SerializeField] string characterName = "Adam";
    public string CharacterName { get {return characterName; } }

    [Header("CHARACTERISTICS")]
    [Range(1, 10)] public int strenght = 5;
    [Range(1, 10)] public int agility = 5;
    [Range(1, 10)] public int dexterity = 5;
    [Range(1, 10)] public int conviction = 5;
    [Range(1, 10)] public int instict = 5;
    [Header("CHARACTERISTICS MODDIFIERS")]
    [Range(-5, 5)] public int modStrenght = 0;
    [Range(-5, 5)] public int modAgility = 0;
    [Range(-5, 5)] public int modDexterity = 0;
    [Range(-5, 5)] public int modConviction = 0;
    [Range(-5, 5)] public int modInstict = 0;


    public float movementPoints = 30f;
    public int attackRange = 1;
    public int initiative;

    public ObjectInGrid gridObject;
    public EntityTurn entityTurn;

    public bool rangedBasedAttack = false;

    public int maxHp = 100;
    public HealthComponent healthComponent;

    public int damage = 100;

    public Aliance characterAliance = Aliance.None;
    public Color color;

    public IController Controller;

    [Header("Command Input for PlayerCharacters, Comand Input Enemies for Enemies")]
    [SerializeField] bool originallyPlayerControlled = true;

    public bool PlayerCharacter { get { return originallyPlayerControlled; } }
    
    public event Action<Entity> OnTurnEnded;



    void Awake()
    {
        if (gridObject == null) { gridObject = GetComponent<ObjectInGrid>(); }
        if (entityTurn == null) { entityTurn = GetComponent<EntityTurn>(); }
        if (healthComponent == null)
        {
            healthComponent = GetComponent<HealthComponent>();
        }
        healthComponent.healthLost += Damaged;
        healthComponent.healthGained += Healed;
        healthComponent.hasDied += Dying;
        healthComponent.SetMaxHealth(maxHp);
    }
    private void Start()
    {
        UpdateMovementPoints();
    }

    private void UpdateMovementPoints()
    {
        movementPoints = 5 * (agility + modAgility);
        gridObject.movementPoints = movementPoints;
    }

    void FixedUpdate()
    {
        UpdateMovementPoints();
        UpdateAttackRange();
    }
    public void AssignController(IController newController)
    {
        Controller = newController;
    }

    private void UpdateAttackRange()
    {
        attackRange = rangedBasedAttack == true ? 4 : 1;
    }

    void Damaged() 
    { 
        
    }
    void Healed()
    {

    }
    void Dying() 
    {
    
    }
    virtual public void SpecialHability1() 
    { 
    
    }
    virtual public void SpecialHability2()
    {

    }

    public virtual void CalculateInitiative()
    {
        initiative = Random.Range(2, 16) + dexterity + modDexterity;
    }
    public void CheckAndMaybeEndTurn()
    {
        if (TurnEnded())
        {
            EndTurn(); // Delegates to controller logic, then notifies the system
        }
    }
    public void ConsumeActions(bool isCardAction) 
    {
        if (!isCardAction)
        {
            entityTurn.ConsumeAction();
        }
        else 
        {
            entityTurn.ConsumeCardAction();
        }
        CheckAndMaybeEndTurn();
    }
    public bool TurnEnded() 
    {
        bool ended = entityTurn.currentTurnActions <= 0 && entityTurn.currentCardTurnActions <= 0;
        //Debug.LogFormat("[TurnEnded Check] {0} TurnEnded: {1} (Actions: {2}, CardActions: {3})", CharacterName, ended, entityTurn.currentTurnActions, entityTurn.currentCardTurnActions);
        return ended;
    }

    public bool IsAlive()
    {
        return !healthComponent.IsDead;
    }
    public void StartTurn()
    {
        entityTurn.AllowTurn();
        Controller.BeginTurn(this);
    }

    public void EndTurn()
    {
        Debug.LogFormat("{0} EndTurn() called. TurnEnded: {1} | Current Actions: {2}, Card Actions: {3}",CharacterName, TurnEnded(), entityTurn.currentTurnActions, entityTurn.currentCardTurnActions);
        Debug.LogFormat("{0} is ending their turn. Event has {1}.", CharacterName, (OnTurnEnded != null ? "subscribers" : "no subscribers"));
        Controller.EndTurn(this);
        OnTurnEnded.Invoke(this);

    }

}