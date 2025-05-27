
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthConcentComp : MonoBehaviour
{
    [SerializeField][Range(0, 100)] private int health;
    [SerializeField][Range(1, 100)] private int maxHealth;
    [SerializeField][Range(0, 50)] private int concentration;
    [SerializeField][Range(1, 50)] private int maxConcentration;
    private bool actorInmortal = false;
    public bool IsDead { get { return health <= 0; } }
    public int Health { get { return health; } }
    public int MaxHealth { get { return maxHealth; } }
    public int Concentration { get { return concentration; } }
    public int MaxConcentration { get { return maxConcentration; } }
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
    public void ConcentrationGain(int concentToGain)
    {
        int newConcentration = concentration + concentToGain;
        concentration = newConcentration >= 50 ? 50 : newConcentration;
        Debug.Log("Current Concentration: " + concentration + " " + maxConcentration);
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
    public void SetToMaxConcentration(int newMax)
    {
        SetMaxConcentration(newMax);
        health = maxHealth;
    }
    public void SetMaxConcentration(int newMax)
    {
        newMax = newMax >= 50 ? 50: newMax;
        maxHealth = newMax;
    }

    public float GetHealthForSlider()
    {
        float percentage = (health * 100) / maxHealth;
        return percentage / 100;
    }
    public float GetConcentrationForSlider()
    {
        float percentage = (concentration * 100) / maxConcentration;
        return percentage / 100;
    }
}

