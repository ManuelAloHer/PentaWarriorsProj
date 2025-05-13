
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField][Range(1, 100)] private int health;
    [SerializeField][Range(1, 100)] private int maxHealth;
    private bool actorInmortal = false;
    public bool IsDead { get { return health <= 0; } }
    public int Health { get { return health; } }
    public int MaxHealth { get { return maxHealth; } }
    public bool ActorInmortal { get { return actorInmortal; } set { actorInmortal = value; } }

    public event Action healthLost;
    public event Action healthGained;
    public event Action hasDied;


    //private void OnEnable()
    //{
    //    ResetState();
    //}

    //private void ResetState()
    //{
    //    health = maxHealth;
    //}

    //// Start is called before the first frame update
    //void Start()
    //{
    //    health = maxHealth;
    //}
    public void HealthLoss(int healthToLoss)
    {
        if (actorInmortal) { return; }
        int newHealth = health - healthToLoss;
        health = newHealth <= 0 ? 0 : newHealth;
        healthLost();
        if (health == 0) { DyingBehaviour(); }
    }
    public void HealthGain(int healthToGain)
    {
        int newHealth = health + healthToGain;
        health = newHealth >= maxHealth ? maxHealth : newHealth;
        healthGained();
    }
    private void DyingBehaviour()
    {
        hasDied();
    }
    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
    }
}

