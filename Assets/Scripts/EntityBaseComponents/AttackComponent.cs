using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.EventSystems.EventTrigger;

public enum AttackNature { Physical, Spiritual, Neutral} //helpsDeffine Which resistance to Trigger
public class AttackComponent : MonoBehaviour
{
    ObjectInGrid gridObject;
    CharacterAnimator characterAnimator;
    [SerializeField]AttackNature mainAttackNature = AttackNature.Neutral;

    private void Awake()
    {
        gridObject = GetComponent<ObjectInGrid>();
        characterAnimator = GetComponent<CharacterAnimator>();
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
        Entity targetEntity = target.GetEntity();
        if (targetEntity == null) 
        {
            target.GetComponent<HealthConcentComp>().HealthLoss(damageRoll);
        }
        if (attackThrowValue >= targetEntity.GetDefenseValue())
        {
            Debug.Log("SuccesfulAttack");
            damageRoll = targetEntity.GetFinalDmg(damageRoll, mainAttackNature);
            targetEntity.healthComponent.HealthLoss(damageRoll);
        }
        characterAnimator.TriggerAttack();
    }
}
