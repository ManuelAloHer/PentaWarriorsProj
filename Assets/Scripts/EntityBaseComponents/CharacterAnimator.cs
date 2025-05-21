using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialHability {None= 0,Hab1 = 1, Hab2 = 2, Hab3 = 3}
public class CharacterAnimator : MonoBehaviour
{
    public Animator _animator;
    //public Animation attackA


    public void UpdateMovement(float velocity)
    {
        _animator.SetFloat("Speed",velocity);
    }
    public void TriggerAttack()
    {
        _animator.SetTrigger("Attack");
    }
    public void TriggerSpecialHab(SpecialHability specialHab) 
    {
        _animator.SetFloat("SpecialHabSelected", (float)specialHab);
        _animator.SetTrigger("SpecialHabTrigger");
    }
    public void TriggerHurtAndDeath(bool hitLanded, bool isDeadAnimation)
    {
        _animator.SetTrigger("Hurt");

        if (hitLanded && isDeadAnimation) 
        {
            _animator.SetBool("Die", isDeadAnimation);
        }

    }
    public void ReviveAnimation(bool isDeadAnimation)
    {
        _animator.SetBool("Die", isDeadAnimation);
    }
}
