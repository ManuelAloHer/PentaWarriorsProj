using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public enum Aliance {None,Player,Enemy}
public enum TypeOfHitter{ Light, Medium, Heavy }

[RequireComponent(typeof(ObjectInGrid), typeof(HealthConcentComp), typeof(EntityTurn))]
public class Entity : MonoBehaviour
{
    public InputToCommandMap[] InputToCommand;
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

    public DiceLaucher diceLaucher;
   
    public float movementPoints = 30f;
    public int maxAtkRange = 4;
    private int attackRange = 1;
    public int AttackRange { get { return attackRange; } }


    public int baseDefValue = 8;
    public int initiative;

    public ObjectInGrid gridObject;
    public EntityTurn entityTurn;
    [SerializeField] GameObject turnMarker;

    public bool rangedBasedAttack = false;
    public bool isMagicAttack = false;
    public TypeOfHitter hitter = TypeOfHitter.Light;


    public int maxHp = 100;
    public HealthConcentComp healthComponent;

    public Aliance characterAliance = Aliance.None;
    
    public Sprite sprite;
    public CharacterAnimator characterAnimator;
    public bool showsInfoOnHovereable = true;


    [Header("Is Busy")]
    [SerializeField] private bool isBusy = false;
    public bool IsBusy { get { return isBusy; } }

    public IController Controller;

    [Header("Command Input for PlayerCharacters, Comand Input Enemies for Enemies")]
    [SerializeField] bool originallyPlayerControlled = true;

    public bool PlayerCharacter { get { return originallyPlayerControlled; } }
    
    public event Action<Entity> OnTurnEnded;

    public void SetIsBusy(bool getBusy) 
    { 
        isBusy = getBusy;
    }
    public InputToCommandMap GetInputToCommand(int index) 
    {
        return InputToCommand[index];
    }

    void Awake()
    {
        if (gridObject == null) { gridObject = GetComponent<ObjectInGrid>(); }
        if (entityTurn == null) { entityTurn = GetComponent<EntityTurn>(); }
        if (healthComponent == null)
        {
            healthComponent = GetComponent<HealthConcentComp>();
        }
        healthComponent.healthLost += Damaged;
        healthComponent.healthGained += Healed;
        healthComponent.hasDied += Dying;
        healthComponent.SetMaxHealth(maxHp);
        healthComponent.SetMaxConcentration(50);
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
    private void Update()
    {
        characterAnimator.ChangeRangedAttack(rangedBasedAttack);
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
        attackRange = rangedBasedAttack == true ? maxAtkRange : 1;
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

    public virtual void CalculateInitiative()
    {
        initiative = Random.Range(2, 16) + dexterity + modDexterity;
    }
    public void CheckAndMaybeEndTurn()
    {
        if (entityTurn.turnEnded) { return; }
        if (TurnEnded())
        {
            EndTurn(); // Delegates to controller logic, then notifies the system
        }
    }
    public void ConsumeNormalAction()
    {
        entityTurn.ConsumeAction();
        SetIsBusy(false);
        Debug.Log("Action Consumed");
        CheckAndMaybeEndTurn();
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
    public void ConsumeAllActions()
    {
        entityTurn.ConsumeAllActions();
      
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
        if (healthComponent.IsDead) 
        {
            Controller.EndTurn(this);
            return;
        }
        entityTurn.AllowTurn();
        if (healthComponent.IsDead) { }
        if (Controller == null) 
        { 
            Debug.Log(characterName + "  has not a controller now " + Controller);
            return;
        }
        turnMarker.SetActive(true);
        Controller.BeginTurn(this);
    }

    public void EndTurn()
    {
        if (entityTurn.turnEnded)
        {
            Debug.LogWarning($"{name} tried to end turn again. Skipping...");
            return;
        }

        entityTurn.turnEnded = true;
        Debug.LogFormat("{0} EndTurn() called. TurnEnded: {1} | Current Actions: {2}, Card Actions: {3}",CharacterName, TurnEnded(), entityTurn.currentTurnActions, entityTurn.currentCardTurnActions);
        Debug.LogFormat("{0} is ending their turn. Event has {1}.", CharacterName, (OnTurnEnded != null ? "subscribers" : "no subscribers"));
        
        turnMarker.SetActive(false);
        Controller.EndTurn(this);
        OnTurnEnded.Invoke(this);
    }
    public int CheckAttackTrow() // Uses 0.5 to divide by 2 since I need more perfonce than precision and multiplication is cheaper
    { 
        float baseModifier = !rangedBasedAttack ? strenght + modStrenght : isMagicAttack ? instict + modInstict :  dexterity + modDexterity;
        int atkModifiers = (int)Mathf.Floor((baseModifier) * 0.5f);
        int result = diceLaucher.BaseDiceLaunch();
        if (result == 16) { healthComponent.ConcentrationGain(1); }
        result += atkModifiers;
        return result;
    }

    public int GetDefenseValue()
    {
        float baseModifier = agility + modAgility;
        int defModifiers = (int)Mathf.Floor((baseModifier) * 0.5f);
        return baseDefValue + defModifiers;
    }

    public int CheckMainDmgTrow()
    {
        int dmg = 0;
        int modifiers = !rangedBasedAttack ? strenght + modStrenght : isMagicAttack ? instict + modInstict : dexterity + modDexterity;
        if (hitter == TypeOfHitter.Light) 
        {
            dmg = diceLaucher.d6DiceThrow(modifiers);
        }
        else if(hitter == TypeOfHitter.Medium)
        {
            dmg = diceLaucher.d8DiceThrow(modifiers);
        }
        else 
        {
            dmg = diceLaucher.d10DiceThrow(modifiers);
        }
        return dmg; 
    }

    public int GetResistance(AttackNature attacknature) // Uses 0.5 to divide by 2 since I need more perfonce than precision and multiplication is cheaper
    {
        float resistanceBase = 0f;

        if (attacknature == AttackNature.Neutral) 
        {
            resistanceBase = conviction + modStrenght + conviction + modConviction;
            resistanceBase *= 0.5f;
        }
        if (attacknature == AttackNature.Physical) 
        {
            resistanceBase = strenght + modStrenght;
        }
        else 
        {
            resistanceBase = conviction + modConviction;
        }

        int resistance = (int)Mathf.Floor((resistanceBase) *0.5f);
        return resistance;
    }

    public int GetFinalDmg(int damageRoll, AttackNature attackNature)
    {
        int finalDamage = damageRoll-GetResistance(attackNature);
        return finalDamage = finalDamage <= 0 ? 1 : finalDamage;
    }
}