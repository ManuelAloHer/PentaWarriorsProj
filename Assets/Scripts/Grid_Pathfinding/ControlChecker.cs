using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;


public class ControlChecker : MonoBehaviour // Conbines Character Atack and Movement controls
{
    public GridMap targetGrid;
    Pathfinding pathfinding;

    //public Vector3Int positionOnGrid;


    List<PathNode> path = new List<PathNode>();
    List<Vector3Int> targetPos;
    Vector3Int origin;

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
        if (!controlledObject.GetAliance().Equals(Aliance.Player)) 
        { 
            // AI possible routs
            return;
        }
        highlight.Hide();
        highlight.Highlight(transitableNodes); 
        attackHighlight.Hide();
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

    //public void CalculateSingleTargetArea(Entity character, Aliance targetAliance)
    //{
    //    ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

    //    if (targetPos == null)
    //    {
    //        targetPos = new List<Vector3Int>();
    //    }
    //    else
    //    {
    //        targetPos.Clear();
    //    }

    //    Queue<(Vector3Int pos, int dist)> frontier = new Queue<(Vector3Int, int)>();
    //    HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

    //    Vector3Int origin = controlledCharacter.positionInGrid;
    //    frontier.Enqueue((origin, 0));
    //    visited.Add(origin);

    //    Vector3Int[] directions = new Vector3Int[]
    //{
    //    new Vector3Int(1, 0, 0),
    //    new Vector3Int(-1, 0, 0),
    //    new Vector3Int(0, 1, 0),
    //    new Vector3Int(0, -1, 0),
    //    new Vector3Int(0, 0, 1),
    //    new Vector3Int(0, 0, -1),
    //};
    //    while (frontier.Count > 0)
    //    {
    //        var (current, distance) = frontier.Dequeue();
    //        if (distance > character.AttackRange)
    //            continue;

    //        if (!targetGrid.CheckBounderies(current))
    //            continue;

    //        var node = targetGrid.GetNode(current);
    //        bool isObstacle = node.onAir || node.obstructed;
    //        bool hasAnyEntity = targetGrid.CheckEntityRootPresence(current.x, current.y, current.z); // not just root
    //        bool isSelfOccupied = controlledCharacter.OccupiesGridCell(current);

    //        Determine if this tile should stop the spread
    //        bool blocksSpread = (isObstacle || hasAnyEntity) && !isSelfOccupied;

    //        Handle targeting
    //        if (hasAnyEntity)
    //        {
    //            Aliance entityAliance = targetGrid.GetAlianceInNode(current);

    //            if (targetAliance != Aliance.None && entityAliance == targetAliance)
    //            {
    //                targetPos.Add(current); // Valid target
    //            }
    //        }
    //        else if (!isObstacle)
    //        {
    //            targetPos.Add(current); // Empty walkable space
    //        }

    //        if (blocksSpread)
    //        {
    //            Stop the flood here
    //            continue;
    //        }

    //        Spread to neighbors
    //        foreach (var dir in directions)
    //        {
    //            Vector3Int neighbor = current + dir;
    //            if (!visited.Contains(neighbor))
    //            {
    //                visited.Add(neighbor);
    //                frontier.Enqueue((neighbor, distance + 1));
    //            }
    //        }
    //    }
    //    if (!character.characterAliance.Equals(Aliance.Player))
    //    {
    //        AI possible attackPositions
    //        return;
    //    }
    //    attackHighlight.Hide();
    //    attackHighlight.Highlight(targetPos);
    //    highlight.Hide();
    //}
    public void CalculateSingleTargetArea(Entity character, Aliance targetAliance)
    {
        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

        
        Vector3Int origin = controlledCharacter.positionInGrid;
        if (targetPos == null)
        {
            targetPos = new List<Vector3Int>();
        }
        else
        {
            targetPos.Clear();
        }

        if (character.rangedBasedAttack)
        {
            int attackRange = character.AttackRange;
            for (int x = -attackRange; x <= attackRange; x++)
            {
                for (int y = -attackRange; y <= attackRange; y++)
                {
                    for (int z = -attackRange; z <= attackRange; z++)
                    {
                        int distance = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                        if (distance > attackRange) { continue; }

                        Vector3Int pos = origin + new Vector3Int(x, y, z);

                        if (targetGrid.CheckBounderies(pos) == true)
                        {
                            bool isTransitable = !targetGrid.GetNode(pos).onAir && !targetGrid.GetNode(pos).obstructed;
                            bool hasEntity = targetGrid.CheckEntityRootPresence(pos.x, pos.y, pos.z);
                            if (!targetAliance.Equals(Aliance.None) && targetGrid.GetAlianceInNode(pos) != Aliance.None)
                            {
                                if (!targetAliance.Equals(targetGrid.GetAlianceInNode(pos)))
                                {
                                    hasEntity = false;
                                    
                                }
                            }

                            if (isTransitable || hasEntity)
                            {
                                targetPos.Add(pos);

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

                        if (targetGrid.CheckBounderies(pos) == true)
                        {
                            bool isTransitable = !targetGrid.GetNode(pos).onAir && !targetGrid.GetNode(pos).obstructed;
                            bool hasEntity = targetGrid.CheckEntityRootPresence(pos.x, pos.y, pos.z);

                            if (!targetAliance.Equals(Aliance.None) && targetGrid.GetAlianceInNode(pos) != Aliance.None)
                            {
                                if (!targetAliance.Equals(targetGrid.GetAlianceInNode(pos)))
                                {
                                    hasEntity = false;
                                }
                            }

                            if (isTransitable || hasEntity)
                            {
                                targetPos.Add(pos);
                            }
                        }
                    }
                }
            }
        }
        targetPos = FilterLineOfSightTargets(character, targetPos);
        if (!character.characterAliance.Equals(Aliance.Player))
        {
            //AI possible attackPositions
            return;
        }
        attackHighlight.Hide();
        attackHighlight.Highlight(targetPos);
        highlight.Hide();
    }
    public List<Vector3Int> FilterLineOfSightTargets(Entity character, List<Vector3Int> targets)
    {
        List<Vector3Int> filtered = new List<Vector3Int>();
        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

        Vector3 casterOriginWorld = targetGrid.GetWorldPosition(controlledCharacter.positionInGrid); // Start point

        foreach (Vector3Int target in targets)
        {
            Vector3 targetWorld = targetGrid.GetWorldPosition(target);

            Vector3 direction = (targetWorld - casterOriginWorld).normalized;
            float distance = Vector3.Distance(casterOriginWorld, targetWorld);

            // Adjust layer mask as needed (e.g., walls, entities, environment)
            int obstacleMask = LayerMask.GetMask("Obstacle", "VisibleObstacle", "InteractableObstacle");

            // Perform the raycast
            if (!Physics.Raycast(casterOriginWorld, direction, distance, obstacleMask))
            {
                // No hit = clear line of sight
                filtered.Add(target);
            }
            else
            {
                Debug.DrawLine(casterOriginWorld, targetWorld, Color.red, 1f); // Optional debug line
            }
        }

        return filtered;
    }
    public void CalculateMultipleTargetArea(Entity character, Aliance targetAliance)
    {
        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

        Vector3Int origin = controlledCharacter.positionInGrid;

        if (targetPos == null)
        {
            targetPos = new List<Vector3Int>();
        }
        else
        {
            targetPos.Clear();
        }
            int attackRange = character.AttackRange;
            for (int x = -attackRange; x <= attackRange; x++)
            {
                for (int y = -attackRange; y <= attackRange; y++)
                {
                    for (int z = -attackRange; z <= attackRange; z++)
                    {
                        int distance = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                        if (distance > attackRange) { continue; }

                        Vector3Int pos = origin + new Vector3Int(x, y, z);

                        if (targetGrid.CheckBounderies(pos) == true)
                        {
                            bool isTransitable = !targetGrid.GetNode(pos).onAir && !targetGrid.GetNode(pos).obstructed;
                            bool hasEntity = targetGrid.CheckEntityRootPresence(pos.x, pos.y, pos.z);
                        if (!targetAliance.Equals(Aliance.None) && targetGrid.GetAlianceInNode(pos) != Aliance.None)
                        {
                            if (!targetAliance.Equals(targetGrid.GetAlianceInNode(pos)))
                            {
                                hasEntity = false;
                            }
                        }
                        if (isTransitable || hasEntity)
                            {
                                targetPos.Add(pos);
                            }
                        }
                    }
                }
            }
        if (!character.characterAliance.Equals(Aliance.Player))
        {
            // AI possible attackPositions
            return;
        }
        attackHighlight.Hide();
        attackHighlight.Highlight(targetPos);
        highlight.Hide();
    }
    
    public List<ObjectInGrid> MultipleTargetSelected(Entity caster,Vector3Int originOfEffect, Aliance targetAliance)
    {
        List<ObjectInGrid> targets = new List<ObjectInGrid>();
            int attackRange = caster.AttackRange;
            for (int x = -attackRange; x <= attackRange; x++)
            {
                for (int y = -attackRange; y <= attackRange; y++)
                {
                    for (int z = -attackRange; z <= attackRange; z++)
                    {
                        int distance = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                        if (distance > attackRange) { continue; }

                        Vector3Int pos = origin + new Vector3Int(x, y, z);

                        if (targetGrid.CheckBounderies(pos) == true)
                        {
                            bool isTransitable = !targetGrid.GetNode(pos).onAir && !targetGrid.GetNode(pos).obstructed;
                            bool hasEntity = targetGrid.CheckEntityRootPresence(pos.x, pos.y, pos.z);
                        if (!targetAliance.Equals(Aliance.None) && targetGrid.GetAlianceInNode(pos) != Aliance.None)
                        {
                            if (!targetAliance.Equals(targetGrid.GetAlianceInNode(pos)))
                            {
                                hasEntity = false;
                            }
                        }

                        if (hasEntity)
                            {
                                ObjectInGrid target = targetGrid.GetNode(pos).objectInGrid;
                                targets.Add(target);
                            }
                        }
                    }
                }
            }
            return targets;
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
        if (targetGrid == null) return;
        
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
        return targetPos.Contains(positionOnGrid);
    }

    public ObjectInGrid GetTarget(Vector3Int targetPosOnGrid) //returns intended target Object
    {
        if (targetPosOnGrid == null) Debug.Log("No Target");
        targetGrid.PrintNodeState(targetPosOnGrid);
        return targetGrid.GetPlacedObject(targetPosOnGrid);
    }
    public GridNode GetTargetNode(Vector3Int targetPosOnGrid) //returns intended target Object
    {
        if (targetPosOnGrid == null) Debug.Log("No Target");
        targetGrid.PrintNodeState(targetPosOnGrid);
        return targetGrid.GetNode(targetPosOnGrid);
    }
}
