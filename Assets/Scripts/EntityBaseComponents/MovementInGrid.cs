using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class MovementInGrid : MonoBehaviour, IActionEffect// Script that controls the movement of a character or movable Object
{
    public List<Vector3> pathWorldPositions;
    public ActionState State { get; set; }
    public bool IsMoving 
    { 
        get 
        {  
            if (pathWorldPositions == null) { return false; }
            return pathWorldPositions.Count > 0; 
        } 
    }

    [SerializeField] float moveSpeed = 3f;
    CharacterAnimator characterAnimator;
    public bool isCharacter = false;

    Action onComplete;

    private void Awake()
    {
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pathWorldPositions == null) { return; }
        if (pathWorldPositions.Count == 0) 
        {
            
            return; 
        
        }
        if (characterAnimator != null) { characterAnimator.UpdateMovement(3); }
        transform.position = Vector3.MoveTowards(transform.position, pathWorldPositions[0], moveSpeed* Time.deltaTime);

        if (Vector3.Distance(transform.position, pathWorldPositions[0]) < 0.05f) 
        {
            pathWorldPositions.RemoveAt(0);
            if (pathWorldPositions.Count > 0)
            {
                RotateCharacter(transform.position, pathWorldPositions[0]);
            }
            else if (characterAnimator != null)
            {
                characterAnimator.UpdateMovement(0);
            }
            if (pathWorldPositions.Count == 0)
            {
                CompleteEffect();
                return;

            }
        }
    }

    public void RotateCharacter(Vector3 origin, Vector3 destination)
    {
        if (!isCharacter) { return; }
        Vector3 direction = (destination - origin);
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void LateUpdate()
    {
        
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
