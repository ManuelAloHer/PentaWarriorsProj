using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public enum ControlState { None, Move, Attack}
public class PlayerControl : MonoBehaviour // Conbines Character Atack and Movement controls
{
    [SerializeField] GridMap targetGrid;
    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] LayerMask entityLayerMask;
    Pathfinding pathfinding;

    [SerializeField] InputController inputCursor;
    [SerializeField] ControlState controlState;
    [SerializeField] ObjectInGrid targetObject;

    public Vector3Int positionOnGrid;
    public bool cursorNeeded;

    List<PathNode> path = new List<PathNode>();
    List<Vector3Int> attackPos;

    [SerializeField] GridHighlight highlight;
    [SerializeField] GridHighlight attackHighlight;
    

    private void Awake()
    {
        pathfinding = targetGrid.GetComponent<Pathfinding>();
    }

    private void Start()
    {
        if (inputCursor == null)
        {
            Debug.LogError("InputController not set");
        }
        InvokeRepeating("CheckTransitableTerrain",0,1);
        InvokeRepeating("CalculateAttackAreaRepeat", 0, 1);
    }

    private void FixedUpdate()
    {
        
    }
    private void CheckTransitableTerrain()
    {
        if (controlState != ControlState.Move)
        {
            if (highlight.gameObject.activeSelf) { highlight.gameObject.SetActive(false); }
            return;
        }
        else if(!highlight.gameObject.activeSelf)
        {
            highlight.gameObject.SetActive(true);
        }


        List<PathNode> transitableNodes = new List<PathNode>();
        pathfinding.CalculateWalkableNodes(targetObject.positionInGrid.x, 
                                           targetObject.positionInGrid.y, 
                                           targetObject.positionInGrid.z, targetObject.movementPoints,
                                           ref transitableNodes);
        highlight.Highlight(transitableNodes);

    }
    private void CalculateAttackAreaRepeat() 
    {
        CalculateAttackArea(false);
    }
    private void CalculateAttackArea(bool selfAlianceTargetable = false)
    {
        if (controlState != ControlState.Attack)
        {
            if (attackHighlight.gameObject.activeSelf) { attackHighlight.gameObject.SetActive(false); }
            return;
        }
        else if (!attackHighlight.gameObject.activeSelf)
        {
            attackHighlight.gameObject.SetActive(true);
        }
        Entity character = targetObject.GetComponent<Entity>();
        int attackRange = character.attackRange;
        
        Vector3Int origin = targetObject.positionInGrid;

        attackPos = new List<Vector3Int>();


        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                for (int z = -attackRange; z <= attackRange; z++)
                {
                    int distance = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                    if ( distance > attackRange) { continue; }
                    
                    Vector3Int pos = origin + new Vector3Int(x, y, z);
                    if (selfAlianceTargetable == false) 
                    {
                        //Check if sameAlince and leave
                        if (x == 0 && y == 0){ continue; }
                    }
                    if (targetGrid.CheckBounderies(pos) == true) 
                    {
                        bool isTransitable = !targetGrid.GetNode(pos).onAir;
                        bool hasEntity = targetGrid.CheckEntiyPresence(pos.x, pos.y, pos.z);

                        if (isTransitable || hasEntity)
                        {
                            attackPos.Add(pos);
                        }

                    }

                }
            }
        }
        attackHighlight.Highlight(attackPos);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(inputCursor.GetCursorPosition());
        RaycastHit hit;

        if (controlState == ControlState.Move)
        {
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                cursorNeeded = true;
                ChangePositionOnGridMonitor(hit);
            }
            else
            {
                cursorNeeded = false;
            }

        }
        else 
        {
            if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                cursorNeeded = true;
                ChangePositionOnGridMonitor(hit);
            }
            else 
            {
                cursorNeeded = false;
            }
        }


        //if (controlState == ControlState.Move && inputCursor.IsConfirmPressed())
        //{
        //    if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        //    {
        //        Vector3Int gridPosition = targetGrid.GetGridPosition(hit.point);

        //        //This is for free of turn movement
        //        //path = pathfinding.FindPath(targetObject.positionInGrid.x, targetObject.positionInGrid.y, targetObject.positionInGrid.z,
        //        //                           gridPosition.x, gridPosition.y, gridPosition.z);
        //        path = pathfinding.TraceBackPath(gridPosition.x, gridPosition.y, gridPosition.z);
        //        if (path == null || path.Count == 0) 
        //        {
        //            Debug.Log("Path is Null or Empty");
        //            return;
        //        }
        //        targetObject.Move(path);
        //    }
        //}
        //if (controlState == ControlState.Attack && inputCursor.IsConfirmPressed())
        //{
        //    if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        //    {
        //        Vector3Int gridPosition = targetGrid.GetGridPosition(hit.point);
        //        if (attackPos.Contains(gridPosition)) 
        //        {
        //            ObjectInGrid gridTarget = targetGrid.GetPlacedObject(gridPosition);
        //            if (gridTarget == null || gridTarget.GetAliance() == targetObject.GetAliance()) { return; }
        //            targetObject.Attack(gridPosition);
        //            //Rest of AttackBehaviour

        //        }
        //    }
        //}
    }

    private void ChangePositionOnGridMonitor(RaycastHit hit)
    {
        cursorNeeded = true;
        Vector3Int gridPosition = targetGrid.GetGridPosition(hit.point);
        if (gridPosition != positionOnGrid)
        {
            positionOnGrid = gridPosition;
        }
    }

    private void OnDrawGizmos()
    {
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(targetGrid.GetWorldPosition(path[i].pos_x, path[i].pos_y, path[i].pos_z),
                            targetGrid.GetWorldPosition(path[i + 1].pos_x, path[i + 1].pos_y, path[i].pos_z));
        }

    }
    public void ChangeControlState(ControlState newState) 
    {
        if (controlState == newState) { controlState = ControlState.None; }
        else 
        {
            controlState = newState;

        }
    }
    public void MoveButtonControlState() 
    {
        ChangeControlState(ControlState.Move); 
    }
    public void AttackButtonControlState()
    {
        ChangeControlState(ControlState.Attack);
    }
}
