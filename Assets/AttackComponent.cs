using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.EventSystems.EventTrigger;

public class AttackComponent : MonoBehaviour
{
    ObjectInGrid gridObject;
    CharacterAnimator characterAnimator;

    private void Awake()
    {
        gridObject = GetComponent<ObjectInGrid>();
        characterAnimator = GetComponent<CharacterAnimator>();
    }
    public void AttackPosition(Vector3 targetDirection) 
    {
        RotateCharacter(targetDirection);
        characterAnimator.TriggerAttack();
    }
    public void RotateCharacter(Vector3 towards)
    {
        Vector3 direction = (towards - transform.position).normalized;
        //Take te altitude between attacker and target here
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

}
