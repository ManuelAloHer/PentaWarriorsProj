using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public enum SpecialHability {None= 0,Hab1 = 1, Hab2 = 2, Hab3 = 3}

public class CharacterAnimator : MonoBehaviour
{
    public Animator _animator;
    public List<GameObject> projectiles;
    public List<GameObject> effects;
    public GameObject shootingPoint;
    public GameObject auraSpawner;
    public Vector3Int rootTargetPos;
    public ObjectInGrid targetedObjectInGrid;

    //public Animation attackA

    public Action onAnimationComplete;
    public event Action OnHitComplete, OnHurtComplete, OnHealComplete, 
                        OnBuffedComplete, OnDebuffedComplete;

    public bool isDead = false;

    private void Awake()
    {
        onAnimationComplete += AnimationCompleteGeneric;
        OnHitComplete += AnimationCompleteGeneric;
        OnHurtComplete += AnimationCompleteGeneric;
        OnHealComplete += AnimationCompleteGeneric;
        OnBuffedComplete += AnimationCompleteGeneric;
        OnDebuffedComplete += AnimationCompleteGeneric;
    }


    public void UpdateMovement(float velocity)
    {
        _animator.SetFloat("Speed",velocity);
    }
    public void TriggerAttack()
    {

        _animator.SetTrigger("Attack");
    }
    public void ChangeRangedAttack(bool newRange)
    {
        _animator.SetBool("RangedAttack", newRange);
    }
    public void SetDeath(bool isDead)
    {
        Debug.Log("Is so Dead");
        _animator.SetBool("Die",isDead);
        _animator.SetTrigger("TriggerDeath");
    }



    public void OtherEffectsSpawn(int EffectToSpawn) 
    {
        Transform bulletProjectileTransform = Instantiate(effects[EffectToSpawn], auraSpawner.transform.position, Quaternion.identity).transform;

    }


    public void TriggerShootEffect()//, ShootAction.OnShootEventArgs e)
    {
        ShootProjectile(0);
    }
    public void FireBalls()
    {
        ShootProjectile(1);
    }
    private void ShootProjectile(int projIndex)
    {
        Transform bulletProjectileTransform = Instantiate(projectiles[projIndex], shootingPoint.transform.position, Quaternion.identity).transform;

        Projectile bulletProjectile = bulletProjectileTransform.GetComponent<Projectile>();

        Vector3Int targetPos = rootTargetPos;
        if (targetedObjectInGrid != null && targetedObjectInGrid.objectDimensions.z > 1)
        {
            targetPos += new Vector3Int(0, 0, 1);
        }

        Vector3 targetShootAtPosition = new Vector3(targetPos.x, targetPos.z, targetPos.y);

        bulletProjectile.Setup(targetShootAtPosition);
    }

    public void TriggerSpecialHab(SpecialHability specialHab) 
    {
        _animator.SetFloat("SpecialHabSelected", (float)specialHab);
        _animator.SetTrigger("SpecialHabTrigger");
    }
    //public void TriggerHurtAndDeath(bool hitLanded, bool isDeadAnimation)
    //{
    //    if (hitLanded) 
    //    {
    //        _animator.SetTrigger("Hurt");
    //        if (hitLanded && isDeadAnimation)
    //        {
    //            _animator.SetBool("Die", isDeadAnimation);
    //        }
    //        return;
    //    }
    //}
    public void ReviveAnimation(bool isDeadAnimation)
    {
        _animator.SetBool("Die", isDeadAnimation);
    }
    public void OnAnimationComplete()
    {
        //Debug.Log("Animation Finished!");
        onAnimationComplete();
        // Notify other components or perform logic here
    }
    //public void OnHitAnimationComplete()
    //{
    //    Debug.Log("Hit Animation Finished!");
    //    onHitAnimation();
    //    // Notify other components or perform logic here
    //}
    public void OnDeadAnimationComplete()
    {
        Debug.Log("Dead Animation Finished!");
        onAnimationComplete();
    }
    public void HitComplete()
    {
        Debug.Log("Hit Finished");
        OnHitComplete();
    }
    public void HurtComplete()
    {
        Debug.Log("Hurt Finished");
        OnHurtComplete();
    }

    public void GeneratePopUp(bool positive, string text)
    {
        Color colorToUse = new Color(1f, 0f, 0f, 1f); ;
        if (positive) { colorToUse = new Color(0f, 1f, 0f, 1f); }
        PopUpGenerator.currentIntance.CreatePopUp(transform.position, text, colorToUse);
    }

    public void AnimationCompleteGeneric()
    { 
    
    }
    public void TriggerHurt()
    {
        _animator.SetTrigger("Hurt");
    }
    public void TriggerHit()
    {
        _animator.SetTrigger("Hit");
    }
    public void TriggerHealed()
    {
        _animator.SetTrigger("Healed");
    }
    public void TriggerBuffed()
    {
        _animator.SetTrigger("Buffed");
    }
    public void TriggerDebuffed()
    {
        _animator.SetTrigger("Debuffed");
    }
    public void NotifyHitComplete() => OnHitComplete?.Invoke();
    public void NotifyHurtComplete() => OnHurtComplete?.Invoke();
    public void NotifyHealComplete() => OnHealComplete?.Invoke();
    public void NotifyBuffedComplete() => OnBuffedComplete?.Invoke();
    public void NotifyDebuffedComplete() => OnDebuffedComplete?.Invoke();

}
