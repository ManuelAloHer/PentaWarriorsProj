using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public enum CoverLevel { Full, Partial, None }
public class LineOfSightResult
{
    public Vector3Int target;
    public CoverLevel cover;
}
public class ControlChecker : MonoBehaviour // Conbines Character Atack and Movement controls
{
    public GridMap targetGrid;
    Pathfinding pathfinding;

    //public Vector3Int positionOnGrid;


    List<PathNode> path = new List<PathNode>();
    public List<PathNode> possibleNodes = new List<PathNode>();
    List<Vector3Int> targetPos;
    Vector3Int origin;

    [SerializeField] GridHighlight highlight;
    [SerializeField] GridHighlight attackHighlight;
    

    private void Awake()
    {
        pathfinding = targetGrid.GetComponent<Pathfinding>();
    }
    public void SetPossibleNodesToNull() 
    {
        possibleNodes = null;
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
            possibleNodes = transitableNodes;
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
        //List<LineOfSightResult> lineOfSight = FilterLineOfSightWithCover(character,targetPos);
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
        // Adjust layer mask as needed (e.g., walls, entities, environment)
        int obstacleMask = LayerMask.GetMask("Obstacle", "VisibleObstacle", "InteractableObstacle","Entity", "EntityBase");


        foreach (Vector3Int target in targets)
        {
            Vector3 targetWorld = targetGrid.GetWorldPosition(target);

            Vector3 direction = (targetWorld - casterOriginWorld).normalized;
            float distance = Vector3.Distance(casterOriginWorld, targetWorld);

            bool blocked = false;

            RaycastHit[] hits = Physics.RaycastAll(casterOriginWorld, direction, distance, obstacleMask);
            foreach (var hit in hits)
            {
                // Check if hit is your own character — ignore it
                ObjectInGrid hitObject = hit.collider.GetComponentInParent<ObjectInGrid>();
                if (hitObject != null && hitObject == controlledCharacter)
                {
                    continue; // ignore self
                }

                // Hit something that blocks LOS
                blocked = true;
                Debug.DrawLine(casterOriginWorld, targetWorld, Color.red, 1f);
                break;
            }

            if (!blocked)
            {
                filtered.Add(target);
                Debug.DrawLine(casterOriginWorld, targetWorld, Color.green, 1f);
            }
        }

        return filtered;
    }
  
    private List<Vector3> GetTargetHitPoints(Vector3Int gridPos)
    {
        Vector3 center = targetGrid.GetWorldPosition(gridPos);

        // Assuming 1x1x3 character — adjust Z offsets accordingly
        return new List<Vector3>
        {
        center + new Vector3(0, 0, 0),     // center
        center + new Vector3(0, 0, 1),     // mid-height
        center + new Vector3(0, 0, 2),     // top of head
        //center + new Vector3(0.4f, 0, 1),  // right shoulder
        //center + new Vector3(-0.4f, 0, 1), // left shoulder
        };
    }
    public List<LineOfSightResult> FilterLineOfSightWithCover(Entity character, List<Vector3Int> targets)
    {
        List<LineOfSightResult> results = new List<LineOfSightResult>();

        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();
        Vector3 casterOriginWorld = targetGrid.GetWorldPosition(controlledCharacter.positionInGrid);

        int losMask = LayerMask.GetMask("Obstacle", "VisibleObstacle", "InteractableObstacle", "Entity", "EntityBase");

        foreach (Vector3Int target in targets)
        {
            Vector3 targetWorld = targetGrid.GetWorldPosition(target);
            ObjectInGrid objectToTest = targetGrid.GetPlacedObject(target);

            List<Vector3> points;
            if (objectToTest != null)
            {
                points = GetTargetHitPoints(target);
            }
            else
            {
                points = new List<Vector3>();
                points.Add(target);
            }
            int blockedCount = 0;

            foreach (var point in points)
            {
                Vector3 dir = (point - casterOriginWorld).normalized;
                float dist = Vector3.Distance(casterOriginWorld, point);

                RaycastHit[] hits = Physics.RaycastAll(casterOriginWorld, dir, dist, losMask);
                bool thisRayBlocked = false;

                foreach (var hit in hits)
                {
                    ObjectInGrid hitObject = hit.collider.GetComponentInParent<ObjectInGrid>();
                    if (hitObject != null && hitObject == controlledCharacter)
                        continue; // ignore self

                    // Any hit = this ray is blocked
                    thisRayBlocked = true;
                    break;
                }

                if (thisRayBlocked)
                {
                    blockedCount++;
                    Debug.DrawRay(casterOriginWorld, dir * dist, Color.red, 1f);
                }
                else
                {
                    Debug.DrawRay(casterOriginWorld, dir * dist, Color.green, 1f);
                }
            }

            CoverLevel cover = CoverLevel.None;
            if (blockedCount == points.Count)
                cover = CoverLevel.Full;
            else if (blockedCount > 0)
                cover = CoverLevel.Partial;

            results.Add(new LineOfSightResult
            {
                target = target,
                cover = cover
            });
            if(targetGrid.GetPlacedObject(target) != null)
            {
                Debug.Log(target + "  " + cover);
            }
        }

        return results;
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

    public Vector3Int CheckForNodeNearestPointInPossibleNodes(Vector3Int placeToMove,Vector3Int originPoint)
    {
        List<Vector3Int> positionsToCheck = new List<Vector3Int>();
        foreach (PathNode possible in possibleNodes)
        {
            positionsToCheck.Add(new Vector3Int(possible.pos_x, possible.pos_y, possible.pos_z));
        }
        if (positionsToCheck.Contains(placeToMove))
        {
            return placeToMove;
        }
        Vector3Int closest = placeToMove;
        float minDistance = float.MaxValue;
        foreach (Vector3Int pos in positionsToCheck)
        {
            float distance = Vector3Int.Distance(placeToMove, pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = pos;
            }
        }
        return closest;

    }

}
