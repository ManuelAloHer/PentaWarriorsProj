using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;


public class PlayerControl : MonoBehaviour // Conbines Character Atack and Movement controls
{
    public GridMap targetGrid;
    Pathfinding pathfinding;

    //public Vector3Int positionOnGrid;


    List<PathNode> path = new List<PathNode>();
    List<Vector3Int> attackPos;

    [SerializeField] GridHighlight highlight;
    [SerializeField] GridHighlight attackHighlight;
    

    private void Awake()
    {
        pathfinding = targetGrid.GetComponent<Pathfinding>();
    }


    public void CheckTransitableTerrain(ObjectInGrid controlledObject)
    {

        List<PathNode> transitableNodes = new List<PathNode>();
        pathfinding.Clear();
        pathfinding.CalculateWalkableNodes(controlledObject.positionInGrid.x,
                                           controlledObject.positionInGrid.y,
                                           controlledObject.positionInGrid.z, controlledObject.movementPoints,
                                           ref transitableNodes);
        highlight.Hide();
        highlight.Highlight(transitableNodes);
        
    }
    public List<PathNode> GetPath(Vector3Int from) 
    {
        path = pathfinding.TraceBackPath(from.x, from.y, from.z);
        if (path == null || path.Count == 0)
        {
            Debug.Log("Path is Null or Empty");
            return null;
        }
        return path;
    }

    public void CalculateAttackArea(Entity character, bool selfAlianceTargetable = false)
    {
        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();
        
        Vector3Int origin = controlledCharacter.positionInGrid;

        if (attackPos == null)
        {
            attackPos = new List<Vector3Int>();
        }
        else 
        {
            attackPos.Clear();  
        }

        if (character.rangedBasedAttack)
        {
            int attackRange = character.attackRange;
            for (int x = -attackRange; x <= attackRange; x++)
            {
                for (int y = -attackRange; y <= attackRange; y++)
                {
                    for (int z = -attackRange; z <= attackRange; z++)
                    {
                        int distance = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                        if (distance > attackRange) { continue; }

                        Vector3Int pos = origin + new Vector3Int(x, y, z);
                        if (selfAlianceTargetable == false)
                        {
                            //Check if sameAlince and leave
                            if (x == 0 && y == 0) { continue; }
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
        }
        else
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {

                        Vector3Int pos = origin + new Vector3Int(x, y, z);
                        if (selfAlianceTargetable == false)
                        {
                            //Check if sameAlince and leave
                            if (x == 0 && y == 0) { continue; }
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
        }
        attackHighlight.Highlight(attackPos);
    }

    #region Deprecated
    //private void FixedUpdate()
    //{
    //    CheckActivation();
    //}

    //private void CheckActivation()
    //{
    //    highlight.gameObject.SetActive(true);
    //    if (controlState != ControlState.Move)
    //    {
    //        if (highlight.gameObject.activeSelf) { highlight.gameObject.SetActive(false); }
    //        return;
    //    }
    //    else if (!highlight.gameObject.activeSelf)
    //    {
    //        highlight.gameObject.SetActive(true);
    //    }
    //}

    //void Update()
    //{
    //    if (controlState == ControlState.Move)
    //    {
    //        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
    //        {
    //            cursorNeeded = true;
    //            ChangePositionOnGridMonitor(hit);
    //            if (inputCursor.IsConfirmPressed())
    //            {
    //                path = pathfinding.TraceBackPath(positionOnGrid.x, positionOnGrid.y, positionOnGrid.z);
    //                if (path == null || path.Count == 0)
    //                {
    //                    Debug.Log("Path is Null or Empty");
    //                    return;
    //                }
    //                controlledObject.Move(path);
    //                hasCalculated = false;
    //            }
    //        }
    //        else
    //        {
    //            cursorNeeded = false;
    //        }
    //    }
    //    else if (controlState == ControlState.Attack)
    //    {
    //        if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
    //        {
    //            cursorNeeded = true;
    //            ChangePositionOnGridMonitor(hit);
    //            if (inputCursor.IsConfirmPressed())
    //            {
    //                if (attackPos.Contains(positionOnGrid))
    //                {
    //                    ObjectInGrid gridTarget = targetGrid.GetPlacedObject(positionOnGrid);
    //                    //if (gridTarget == null || gridTarget.GetAliance() == controlledObject.GetAliance()) { return; }
    //                    //controlledObject.Attack(positionOnGrid);
    //                    //Rest of AttackBehaviour
    //                }
    //            }
    //        }
    //        else
    //        {
    //            cursorNeeded = false;
    //        }
    //    }
    //    if (controlState == ControlState.Move && inputCursor.IsConfirmPressed())
    //    {
    //        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
    //        {
    //            Vector3Int gridPosition = targetGrid.GetGridPosition(hit.point);
    //            //This is for free of turn movement
    //            //path = pathfinding.FindPath(targetObject.positionInGrid.x, targetObject.positionInGrid.y, targetObject.positionInGrid.z,
    //            //                           gridPosition.x, gridPosition.y, gridPosition.z);
    //            path = pathfinding.TraceBackPath(gridPosition.x, gridPosition.y, gridPosition.z);
    //            if (path == null || path.Count == 0)
    //            {
    //                Debug.Log("Path is Null or Empty");
    //                return;
    //            }
    //            targetObject.Move(path);
    //        }
    //    }
    //    if (controlState == ControlState.Attack && inputCursor.IsConfirmPressed())
    //    {
    //        if (Physics.Raycast(ray, out hit, float.MaxValue, entityLayerMask) || Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
    //        {
    //            Vector3Int gridPosition = targetGrid.GetGridPosition(hit.point);
    //            if (attackPos.Contains(gridPosition))
    //            {
    //                ObjectInGrid gridTarget = targetGrid.GetPlacedObject(gridPosition);
    //                if (gridTarget == null || gridTarget.GetAliance() == targetObject.GetAliance()) { return; }
    //                targetObject.Attack(gridPosition);
    //                //Rest of AttackBehaviour
    //            }
    //        }
    //    }
    //}
    #endregion


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

    public bool CheckPosibleAttack(Vector3Int positionOnGrid)
    {
        return attackPos.Contains(positionOnGrid);
    }

    public ObjectInGrid GetTarget(Vector3Int targetPosOnGrid) //returns intended target Object
    {
        return targetGrid.GetPlacedObject(targetPosOnGrid);
    }
}
