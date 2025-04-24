using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInGrid : MonoBehaviour // Component Necesary to allow an Object to Move in the Grid
{
    public GridMap targetGrid;
    public Vector3Int positionInGrid;
    [SerializeField] LayerMask terrainLayerMask;
    public float movementPoints = 10f;
    public MovementInGrid movement;
    public AttackComponent attackComponent;
    public Entity entity;

    private void Awake()
    {
        movement = GetComponent<MovementInGrid>();
        entity= GetComponent<Entity>();
        attackComponent = GetComponent<AttackComponent>();
        

    }
    
  
    // Start is called before the first frame update
    void Start()
    {
        if (entity != null) { movementPoints = entity.movementPoints; }
        Init();
        
    }

    private void FixedUpdate()
    {
        if (entity != null) { movementPoints = entity.movementPoints; }
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
        positionInGrid.x = path[path.Count -1].pos_x;
        positionInGrid.y = path[path.Count - 1].pos_y;
        positionInGrid.z = path[path.Count - 1].pos_z;
        movement.RotateCharacter();
    }
    public void Attack(Vector3Int attackinGridPosition) 
    {
        Vector3 attackPosition = targetGrid.GetWorldPosition(attackinGridPosition.x, attackinGridPosition.y, attackinGridPosition.z);
        attackComponent.AttackPosition(attackPosition);
    }

    public Aliance GetAliance()
    {
        if (entity == null) { return Aliance.None; }
        return entity.characterAliance;
    }
}
