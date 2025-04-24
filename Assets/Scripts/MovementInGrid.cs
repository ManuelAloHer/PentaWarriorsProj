using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementInGrid : MonoBehaviour // Script that controls the movementof a character 
{
    public List<Vector3> pathWorldPositions;
    [SerializeField] float moveSpeed = 3f;
    CharacterAnimator characterAnimator;
    public bool isCharacter = false;

    private void Awake()
    {
        characterAnimator = GetComponent<CharacterAnimator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pathWorldPositions == null) { return; }
        if (pathWorldPositions.Count == 0) { return; }
        if (characterAnimator != null) { characterAnimator.UpdateMovement(3); }
        transform.position = Vector3.MoveTowards(transform.position, pathWorldPositions[0], moveSpeed* Time.deltaTime);

        if (Vector3.Distance(transform.position, pathWorldPositions[0]) < 0.05f) 
        {
            pathWorldPositions.RemoveAt(0);
            if (pathWorldPositions.Count > 0)
            {
                RotateCharacter();
            }
            else if (characterAnimator != null)
            {
                characterAnimator.UpdateMovement(0);
            }
        }
    }

    public void RotateCharacter()
    {
        if (!isCharacter) { return; }
        Vector3 direction = (pathWorldPositions[0] - transform.position);
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void LateUpdate()
    {
        
    }
}
