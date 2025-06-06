using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private Transform bulletHitVfxPrefab;
    [SerializeField] float moveSpeed = 200f;

    private Vector3 targetPosition;
    public void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }


    private void Update()
    {
        Vector3 moveDir = (targetPosition - transform.position).normalized;

        float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float distanceAfterMoving = Vector3.Distance(transform.position, targetPosition);

        if (distanceBeforeMoving < distanceAfterMoving)
        {
            transform.position = targetPosition;

            if (trailRenderer != null)
            {
                trailRenderer.transform.parent = null;

                Instantiate(bulletHitVfxPrefab, targetPosition, Quaternion.identity);
                Destroy(this.gameObject);
            }
            else if (particleSystem != null)
            {
                Instantiate(bulletHitVfxPrefab, targetPosition, Quaternion.identity);
                Destroy(this.gameObject);
            }


        }
    }
}
