using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public enum AttackNature { Physical, Spiritual, Neutral} //helpsDeffine Which resistance to Trigger
public class AttackComponent : MonoBehaviour, IActionEffect
{
    ObjectInGrid gridObject;
    CharacterAnimator characterAnimator;
    [SerializeField] AttackNature mainAttackNature = AttackNature.Neutral;
    public List<ObjectInGrid> targetedObjectsInGrid;
    public List<Entity> targetedEntities;
    public int attackRoll, damageRoll;
    private Dictionary<Entity, bool> entityWasHurt = new();
    private Dictionary<Entity, Action> hurtHandlers = new();
    private Dictionary<Entity, Action> hitHandlers = new();

    public ActionState State { get; set; } = ActionState.NotInActionYet;
    public ActionState DebugState;
    public bool isRangedAttack = false;
    public bool isMultipleAtk = false;
    bool stateStarted = false;
    SpecialHability specialHab;

    [SerializeField] private int pendingSignals = 0;
    protected Action onComplete;

    private void Awake()
    {
        gridObject = GetComponent<ObjectInGrid>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
        DebugState = State;
    }
    private void Update()
    {
        switch (State)
        {
            case ActionState.NotInActionYet:
                break;

            case ActionState.WaitingAnimation:
                if (!stateStarted)
                {
                    gridObject.GetEntity().SetIsBusy(true);
                    stateStarted = true;
                    
                    Debug.Log("EnteringAnimationState");
                    characterAnimator.onAnimationComplete += OnLocalAnimationComplete;

                    if (isMultipleAtk == false)
                    {
                        characterAnimator.targetedObjectInGrid = targetedObjectsInGrid[0];
                        characterAnimator.rootTargetPos = targetedObjectsInGrid[0].positionInGrid;
                        WasSingleAttackSucessful();
                        characterAnimator.TriggerAttack();
                    }
                    else 
                    {
                        Debug.Log("My targetPos"+characterAnimator.rootTargetPos);
                        characterAnimator.TriggerSpecialHab(specialHab);
                    }
                }
                break;

            case ActionState.CalculatingEffect:
                if (!stateStarted)
                {
                    Debug.Log("EnteringCalculousState");
                    stateStarted = true;
                    if (isMultipleAtk == true)
                    {
                        WhichAttacksSucessful();
                    }
                    
                }
                break;

            case ActionState.WaitTargetAnimations:
                if (!stateStarted)
                {
                    stateStarted = true;
                    foreach (var entity in targetedEntities)
                    {
                        if (entity == null) continue;

                        var animator = entity.GetComponentInChildren<CharacterAnimator>();
                        if (animator != null)
                        {
                            WaitForSignal();

                            if (entityWasHurt.TryGetValue(entity, out bool wasHurt))
                            {
                                if (wasHurt)
                                {
                                    Debug.Log("My man was Hurt");
                                    Action handler = () => OnHurtComplete(entity);
                                    hurtHandlers[entity] = handler;

                                    animator.OnHurtComplete += handler;
                                    animator.TriggerHurt();
                                    
                                }
                                else
                                {
                                    Debug.Log("Not Even a Scratck");
                                    Action handler = () => OnHitComplete(entity);
                                    hitHandlers[entity] = handler;

                                    animator.OnHitComplete += handler;
                                    animator.TriggerHit();
                                }
                            }
                        }
                    }
                    
                }
                
                if (pendingSignals == 0)
                {
                    TransitionTo(ActionState.ApplyEffect);
                }
                break;

            case ActionState.ApplyEffect:
                if (!stateStarted)
                {
                    stateStarted = true;
                    
                    foreach (var entity in targetedEntities)
                    {
                        if (entity == null) continue;
                        WaitForSignal();

                        var animator = entity.GetComponentInChildren<CharacterAnimator>();
                        if (animator != null)
                        {
                            if (entityWasHurt.TryGetValue(entity, out bool wasHurt))
                            {
                                if (wasHurt)
                                {
                                    ApplyFinalDamage(entity,animator);
                                }
                                else
                                {
                                    animator.GeneratePopUp(false, "Miss");
                                    SignalComplete();
                                    Debug.Log("Not Even a Scratck");
                                }
                            }
                        }
                    }
                    
                    //WaitForSignal(); // For damage numbers
                    //WaitForSignal(); // For camera shake
                    // These must call SignalComplete()
                }

                if (pendingSignals == 0)
                {
                    characterAnimator.rootTargetPos = new Vector3Int(-1,-1,-1);
                    TransitionTo(ActionState.Complete);
                }
                break;

            case ActionState.Complete:
                CompleteEffect();
                TransitionTo(ActionState.NotInActionYet);
                break;

            default:
                Debug.LogError("Unhandled action state");
                break;
        }
        //switch (State)
        //{
        //    case ActionState.NotInActionYet:

        //        break;
        //    case ActionState.WaitingAnimation:
        //        //Set Shooting Animation
        //        break;
        //    case ActionState.CalculatingEffect:
        //        //Calculate if attack is successful and going for next fase
        //        break;
        //    case ActionState.WaitForAnimationCompletion:
        //        // Apply Impact animation or missed animation
        //        break;
        //    case ActionState.ApplyEffect:
        //        // Aply damage if needed and notify damage/ missed attack
        //        break;
        //    case ActionState.Complete:
        //        CompleteEffect();
        //        State = ActionState.NotInActionYet;
        //        break;
        //    default:
        //        Debug.LogError("Action State Unset in: " + GetType().Name);
        //        break;

        //}
    }
    public void WaitForSignal()
    {
        pendingSignals++;
    }
    public void SignalComplete()
    {
        pendingSignals = Mathf.Max(0, pendingSignals - 1);
    }
    private void TransitionTo(ActionState newState)
    {
        State = newState;
        DebugState = State;
        stateStarted = false;
        pendingSignals = 0;
    }
    public void RotateCharacter(Vector3 towards)
    {
        Vector3 direction = (towards - transform.position).normalized;
        //Take te altitude between attacker and target here
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void AttackGridTarget(ObjectInGrid target, int attackThrowValue, int damageRoll, bool rangedAttack)
    {
        RotateCharacter(target.transform.position);
        characterAnimator.rootTargetPos = target.positionInGrid;
        State = ActionState.WaitingAnimation;
        DebugState = State;
        targetedObjectsInGrid.Add(target);
        isMultipleAtk = false;
        isRangedAttack = rangedAttack;
        attackRoll = attackThrowValue;
        this.damageRoll = damageRoll;
    }
    public void AttackOnArea(Vector3Int attackinGridPosition, List<ObjectInGrid> targets, int attackThrowValue, int dmgThrowValue, SpecialHability SpecialHab)
    {
        Vector3 onWorldAttack = new Vector3(attackinGridPosition.x, attackinGridPosition.z, attackinGridPosition.y);
        RotateCharacter(onWorldAttack);
        characterAnimator.rootTargetPos = attackinGridPosition;
        targetedObjectsInGrid = targets;
        isRangedAttack = true;
        attackRoll = attackThrowValue;
        isMultipleAtk = true;
        this.specialHab = SpecialHab;
        this.damageRoll = dmgThrowValue;
        State = ActionState.WaitingAnimation;
        DebugState = State;
    }
    public void WhichAttacksSucessful()
    {
        targetedEntities.Clear();
        foreach (ObjectInGrid target in targetedObjectsInGrid)
        {
            if (target == null)
            {
                Debug.LogWarning("Null Objective");
            }
            Entity entity = target.GetEntity();
            if (entity != null)
            {
                targetedEntities.Add(entity);
                Debug.LogWarning("Added Entity");
                if (attackRoll >= entity.GetDefenseValue())
                {
                    entityWasHurt.Add(entity, true);
                }
                else 
                {
                    entityWasHurt.Add(entity, false);
                }
            }
            if (entity == null) 
            {
                Debug.LogWarning("Null Objective");
            }
        }
        TransitionTo(ActionState.WaitTargetAnimations);
    }
    public void WasSingleAttackSucessful()
    {
        targetedEntities.Clear();
        targetedEntities.Add(targetedObjectsInGrid[0].GetEntity());
        if (targetedEntities[0] == null)
        {
            return;
        }
        if (attackRoll >= targetedEntities[0].GetDefenseValue())
        {
            entityWasHurt.Add(targetedEntities[0], true);
        }
        else 
        {
            entityWasHurt.Add(targetedEntities[0], false);
            Debug.Log("Miss");
        }
        
    }
    public void ApplyFinalDamage(Entity targetEntity,CharacterAnimator animator) // A revisión
    {
        damageRoll = targetEntity.GetFinalDmg(damageRoll, mainAttackNature);
        animator.GeneratePopUp(false, damageRoll.ToString());
        targetEntity.healthComponent.HealthLoss(damageRoll);
        if (targetEntity.healthComponent.Health <= 0)
        {
            animator.SetDeath(true);
        }
        SignalComplete();
    }

    public void Play(Action onComplete)
    {
        this.onComplete = onComplete;
    }

    public void CompleteEffect()
    {
        ResetAttack();
        onComplete?.Invoke();
    }

    public void OnLocalAnimationComplete()
    {
        characterAnimator.onAnimationComplete -= OnLocalAnimationComplete;
        stateStarted = false;
        Debug.Log("StateCompleted");
        if (isMultipleAtk == false) { TransitionTo(ActionState.WaitTargetAnimations); }
        else { TransitionTo(ActionState.CalculatingEffect); }
    }
    private void OnHurtComplete(Entity entity)
    {
        Debug.Log("Hurt Complete");

        var animator = entity.GetComponentInChildren<CharacterAnimator>();
        if (hurtHandlers.TryGetValue(entity, out var handler))
        {
            animator.OnHurtComplete -= handler;
            hurtHandlers.Remove(entity);
        }
        SignalComplete();
    }
    private void OnHitComplete(Entity entity)
    {
        Debug.Log("Miss Complete");

        var animator = entity.GetComponentInChildren<CharacterAnimator>();
        if (hitHandlers.TryGetValue(entity, out var handler))
        {
            animator.OnHitComplete -= handler;
            hitHandlers.Remove(entity);
        }

        SignalComplete();
    }
    public void ResetAttack()
    {
        entityWasHurt.Clear();
        targetedEntities.Clear();
        targetedObjectsInGrid.Clear();
        hurtHandlers.Clear();
        hitHandlers.Clear();
        attackRoll = 0;
        damageRoll = 0;
        specialHab = SpecialHability.None;

        pendingSignals = 0;
        stateStarted = false;
        isMultipleAtk = false;
        State = ActionState.NotInActionYet;
    }


}
