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

    public ActionState State { get; set; } = ActionState.NotInActionYet;
    public ActionState DebugState;
    bool stateStarted = false;

    private int pendingSignals = 0;
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
                    stateStarted = true;
                    Debug.Log("EnteringAnimationState");
                    characterAnimator.onAnimationComplete += OnLocalAnimationComplete;
                    characterAnimator.TriggerAttack();
                }
                break;

            case ActionState.CalculatingEffect:
                if (!stateStarted)
                {
                    Debug.Log("EnteringCalculousState");
                    stateStarted = true;
                    WhichAttacksSucessful();
                    TransitionTo(ActionState.WaitForAnimationCompletion);
                }
                break;

            //case ActionState.WaitForAnimationCompletion:
            //    if (!stateStarted)
            //    {
            //        stateStarted = true;
            //        WaitForSignal();
            //        characterAnimator.ShootBullet();
            //        WaitForSignal(); // Hit animation
            //        targetedEnities[0].characterAnimator.TriggerHurtAndDeath(atkSuccesful, false);
            //        //characterAnimator.OnImpactComplete += SignalComplete;
            //        //TriggerHitVFX(); // will call SignalComplete
            //    }
            case ActionState.WaitForAnimationCompletion:
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
                                    animator.OnHurtComplete += () => OnHurtComplete(entity);
                                    animator.TriggerHurt();
                                }
                                else
                                {
                                    Debug.Log("Not Even a Scratck");
                                    animator.OnHitComplete += () => OnHitComplete(entity);
                                    animator.TriggerHit();
                                }
                            }
                        }
                    }

                    // Optional VFX
                    WaitForSignal();
                    //PlayImpactVFX(() => SignalComplete());
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
                    ApplyFinalDamage();
                    WaitForSignal(); // For damage numbers
                    WaitForSignal(); // For camera shake
                                     // These must call SignalComplete()
                }

                if (pendingSignals == 0)
                {
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

    public void AttackGridTarget(ObjectInGrid target, int attackThrowValue, int damageRoll)
    {
        RotateCharacter(target.transform.position);
        State = ActionState.WaitingAnimation;
        DebugState = State;
        targetedObjectsInGrid.Add(target);
        attackRoll = attackThrowValue;
        this.damageRoll = damageRoll;
    }
    public void WhichAttacksSucessful()
    {
        targetedEntities.Clear();
        foreach (ObjectInGrid target in targetedObjectsInGrid)
        {
            Entity entity = target.GetEntity();
            if (entity != null)
            {
                targetedEntities.Add(entity);
                if (attackRoll >= entity.GetDefenseValue())
                {
                    entityWasHurt.Add(entity, true);
                }
            }


        }
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
        Debug.Log("Miss");
    }
    public void ApplyFinalDamage()
    {
        Entity targetEntity = targetedObjectsInGrid[0].GetEntity();

        if (targetEntity == null)
        {
            targetedObjectsInGrid[0].GetComponent<HealthConcentComp>().HealthLoss(damageRoll);
        }
        damageRoll = targetEntity.GetFinalDmg(damageRoll, mainAttackNature);
        targetEntity.healthComponent.HealthLoss(damageRoll);
    }

    public void Play(Action onComplete)
    {
        ResetAttack();
        this.onComplete = onComplete;
    }

    public void CompleteEffect()
    {
        onComplete?.Invoke();
    }

    public void OnLocalAnimationComplete()
    {
        characterAnimator.onAnimationComplete -= OnLocalAnimationComplete;
        stateStarted = false;
        Debug.Log("StateCompleted");
        TransitionTo(ActionState.CalculatingEffect);
    }
    private void OnHurtComplete(Entity entity)
    {
        Debug.Log("Hurt Complete");
        var animator = entity.GetComponentInChildren<CharacterAnimator>();
        animator.OnHurtComplete -= () => OnHurtComplete(entity);
        SignalComplete();
    }
    private void OnHitComplete(Entity entity)
    {
        Debug.Log("Miss Complete");
        var animator = entity.GetComponentInChildren<CharacterAnimator>();
        animator.OnHitComplete -= () => OnHitComplete(entity);
        SignalComplete();
    }
    public void ResetAttack()
    {
        entityWasHurt.Clear();
        targetedEntities.Clear();
        targetedObjectsInGrid.Clear();
        attackRoll = 0;
        damageRoll = 0;

        onComplete = null;
        pendingSignals = 0;
        stateStarted = false;
        State = ActionState.NotInActionYet;

        // Optional: detach any lingering animator events (for safety)
        foreach (var entity in targetedEntities)
        {
            var anim = entity?.GetComponentInChildren<CharacterAnimator>();
            if (anim != null)
            {
                //anim.OnHitComplete = null;
                //anim.OnHurtComplete = null;
            }
        }
    }
}
