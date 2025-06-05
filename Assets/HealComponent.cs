using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealComponent : MonoBehaviour,IActionEffect
{
    ObjectInGrid gridObject;
    CharacterAnimator characterAnimator;
    [SerializeField] AttackNature mainAttackNature = AttackNature.Neutral;
    public ObjectInGrid targetedObject;
    public Entity targetedEntity;
    public int healingRoll;

    public ActionState State { get; set; } = ActionState.NotInActionYet;
    public ActionState DebugState;

    bool stateStarted = false;
    SpecialHability specialHab;
    [SerializeField] private int pendingSignals = 0;
    protected Action onComplete;

    public void Play(Action onComplete)
    {
        this.onComplete = onComplete;
    }
    public void CompleteEffect()
    {
        
        onComplete?.Invoke();
    }

}
