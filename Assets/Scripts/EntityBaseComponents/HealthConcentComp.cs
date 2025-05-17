
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthConcentComp : MonoBehaviour
{
    [SerializeField][Range(0, 100)] private int health;
    [SerializeField][Range(1, 100)] private int maxHealth;
    private bool actorInmortal = false;
    public bool IsDead { get { return health <= 0; } }
    public int Health { get { return health; } }
    public int MaxHealth { get { return maxHealth; } }
    public bool ActorInmortal { get { return actorInmortal; } set { actorInmortal = value; } }

    public event Action healthLost;
    public event Action healthGained;
    public event Action hasDied;

    public void HealthLoss(int healthToLoss)
    {
        if (actorInmortal) { return; }
        int newHealth = health - healthToLoss;
        health = newHealth <= 0 ? 0 : newHealth;
        healthLost();
        Debug.Log("Current Health: " + health + " " + maxHealth);
        if (health == 0) { DyingBehaviour(); }
    }
    public void HealthGain(int healthToGain)
    {
        int newHealth = health + healthToGain;
        health = newHealth >= 100 ? 100 : newHealth;
        Debug.Log("Current Health: " + health + " " + maxHealth);
        healthGained();
    }
    private void DyingBehaviour()
    {
        hasDied();
    }

    public void SetToMaxHealth(int newMax) 
    {
        SetMaxHealth(newMax);
        health = maxHealth;
    }
    public void SetMaxHealth(int newMax)
    {
        newMax = newMax >= 100 ? 100 : newMax;
        maxHealth = newMax;
    }
}

