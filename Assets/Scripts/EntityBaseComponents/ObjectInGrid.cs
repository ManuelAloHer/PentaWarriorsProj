using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementInGrid))]
public class ObjectInGrid : MonoBehaviour // Component Necesary to allow an Object to Move in the Grid
{
    public GridMap targetGrid;
    public Vector3Int positionInGrid;
    [SerializeField] LayerMask terrainLayerMask;
    public float movementPoints = 10f;
    public MovementInGrid movement;
    public AttackComponent attackComponent;
    private Entity entity;

    public Vector3Int objectDimensions;

    private void Awake()
    {
        movement = GetComponent<MovementInGrid>();
        entity= GetComponent<Entity>();
        attackComponent = GetComponent<AttackComponent>();
        

    }
    
  
    // Start is called before the first frame update
    void Start()
    {
        Init();
        
    }

    private void Init() 
    {
        positionInGrid = targetGrid.GetGridPosition(transform.position);
        
        targetGrid.PlaceObject(positionInGrid, this);
        Vector3 pos = targetGrid.GetWorldPosition(positionInGrid.x, positionInGrid.y, positionInGrid.z);
        transform.position = pos;
    }
    public void Move(List<PathNode> path)
    {
        movement.pathWorldPositions = targetGrid.ConvertPathNodesToWorldPositions(path);
        
        targetGrid.RemoveObject(positionInGrid, this);
        positionInGrid.x = path[path.Count -1].pos_x;
        positionInGrid.y = path[path.Count - 1].pos_y;
        positionInGrid.z = path[path.Count - 1].pos_z;

        targetGrid.PlaceObject(positionInGrid, this);
        movement.RotateCharacter(transform.position, movement.pathWorldPositions[0]);
    }

    public void SkipMovementAnimation()
    {
        if (movement.pathWorldPositions.Count < 2) { return; }
        transform.position = movement.pathWorldPositions[movement.pathWorldPositions.Count - 1];
        Vector3 originPos = movement.pathWorldPositions[movement.pathWorldPositions.Count - 2];
        Vector3 destinationPos = movement.pathWorldPositions[movement.pathWorldPositions.Count - 1];
        movement.RotateCharacter(originPos, destinationPos);
    }

    public void Attack(Vector3Int attackinGridPosition, ObjectInGrid target) 
    {
        int atkThrowValue = entity.CheckAttackTrow();
        int dmgThrowValue = entity.CheckMainDmgTrow();
        //Debug.LogFormat("AtK done by {0} Atk: {1} Dmg Base: {2}",entity.CharacterName, atkThrowValue,dmgThrowValue);
        attackComponent.AttackGridTarget(target, atkThrowValue,dmgThrowValue, entity.rangedBasedAttack);
    }

    public Entity GetEntity() 
    {
        if (entity == null) { return null; }
        return entity;
    }
    public bool ConfirmEntity()
    {
        if (entity == null) { return false; }
        return true;
    }
    public Aliance GetAliance() // mustChange
    {
        if (entity == null) { return Aliance.None; }
        return entity.characterAliance;
    }
    public bool OccupiesGridCell(Vector3Int cell)
    {
        for (int x = 0; x < objectDimensions.x; x++)
        {
            for (int y = 0; y < objectDimensions.y; y++)
            {
                for (int z = 0; z < objectDimensions.z; z++)
                {
                    Vector3Int occupied = positionInGrid + new Vector3Int(x, y, z);
                    if (occupied == cell)
                        return true;
                }
            }
        }
        return false;
    }
}
