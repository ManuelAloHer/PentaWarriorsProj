using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;


public enum Aliance {None,Player,Enemy}

[RequireComponent(typeof(ObjectInGrid), typeof(HealthComponent))]
public class Entity : MonoBehaviour
{
    public string characterName = "Adam";

    [Header("CHARACTERISTICS")]
    [Range(1, 10)] public int strenght = 5;
    [Range(1, 10)] public int agility = 5;
    [Range(1, 10)] public int dexterity = 5;
    [Range(1, 10)] public int conviction = 5;
    [Range(1, 10)] public int instict = 5;
    [Header("CHARACTERISTICS MODDIFIERS")]
    [Range(-5, 5)] public int modStrenght = 0;
    [Range(-5, 5)] public int modAgility = 0;
    [Range(-5, 5)] public int modDexterity = 0;
    [Range(-5, 5)] public int modConviction = 0;
    [Range(-5, 5)] public int modInstict = 0;


    public const float BaseMovementPoints = 30f;
    public float movementPoints = 30f;
    public int attackRange = 1;

    public bool rangedBasedAttack = false;

    public int maxHp = 100;
    public HealthComponent healthComponent;

    public int damage = 100;

    public Aliance characterAliance = Aliance.None;

    // Start is called before the first frame update
    void Awake()
    {
        
        healthComponent = GetComponent<HealthComponent>();
        healthComponent.healthLost += Damaged;
        healthComponent.healthGained += Healed;
        healthComponent.hasDied += Dying;
        healthComponent.SetMaxHealth(maxHp);
    }
    private void Start()
    {
        UpdateMovementPoints();

    }

    private void UpdateMovementPoints()
    {
        movementPoints = BaseMovementPoints + 5 * (agility + modAgility);
    }

    void Update()
    {
        UpdateMovementPoints();
        UpdateAttackRange();
    }

    private void UpdateAttackRange()
    {
        attackRange = rangedBasedAttack == true ? 1 : 4;
    }

    void Damaged() 
    { 
        
    }
    void Healed()
    {

    }
    void Dying() 
    {
    
    }
    virtual public void SpecialHability1() 
    { 
    
    }
    virtual public void SpecialHability2()
    {

    }
}
