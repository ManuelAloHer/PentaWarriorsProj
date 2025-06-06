using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealComponent : MonoBehaviour,IActionEffect
{
    ObjectInGrid gridObject;
    CharacterAnimator characterAnimator;
    public ObjectInGrid targetedObject;
    public Entity targetedEntity;
    public int healingRoll;
    Action currentHandler;

    public ActionState State { get; set; } = ActionState.NotInActionYet;
    public ActionState DebugState;

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
                    characterAnimator.TriggerSpecialHab(specialHab);

                }
                break;

            case ActionState.WaitTargetAnimations:
                if (!stateStarted)
                {
                    stateStarted = true;

                    if (targetedEntity == null) { Debug.Log("No One to Heal"); return; }

                    var animator = targetedEntity.GetComponentInChildren<CharacterAnimator>();
                    if (animator != null)
                    {
                        WaitForSignal();

                        Debug.Log("Heal");
                        Action currentHandler = () => OnHealComplete(targetedEntity);
                        animator.OnHealComplete += currentHandler;
                        animator.TriggerHealed();
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

                    if (targetedEntity == null) { Debug.Log("Not Heal"); return; }
                    WaitForSignal();

                    var animator = targetedEntity.GetComponentInChildren<CharacterAnimator>();
                    if (animator != null)
                    {
                        ApplyHealing(targetedEntity, animator);
                    }
                }

                if (pendingSignals == 0)
                {
                    characterAnimator.rootTargetPos = new Vector3Int(-1, -1, -1);
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
    }

    public void RotateCharacter(Vector3 towards)
    {
        Vector3 direction = (towards - transform.position).normalized;
        //Take te altitude between attacker and target here
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void HealTarget(ObjectInGrid target, int healValue, SpecialHability specialHability)
    {
        if (target != gridObject) 
        {
            RotateCharacter(target.transform.position);
        }
        characterAnimator.rootTargetPos = target.positionInGrid;
        State = ActionState.WaitingAnimation;
        DebugState = State;
        targetedObject = target;
        targetedEntity = target.GetEntity();
        this.healingRoll = healValue;
        this.specialHab = specialHability;

    }

    private void ApplyHealing(Entity targetEntity, CharacterAnimator animator) // A revisión
    {
        animator.GeneratePopUp(true, healingRoll.ToString());
        targetedEntity.healthComponent.HealthGain(healingRoll);
        SignalComplete();
    }

    public void OnLocalAnimationComplete()
    {
        characterAnimator.onAnimationComplete -= OnLocalAnimationComplete;
        stateStarted = false;
        Debug.Log("StateCompleted");
        TransitionTo(ActionState.WaitTargetAnimations);
    }
    private void OnHealComplete(Entity entity)
    {
        Debug.Log("HealComplete");

        var animator = entity.GetComponentInChildren<CharacterAnimator>();
        animator.OnHurtComplete -= currentHandler;
        currentHandler = null;
        SignalComplete();
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
    public void Play(Action onComplete)
    {
        this.onComplete = onComplete;
    }
    public void CompleteEffect()
    {
        
        onComplete?.Invoke();
    }

}
