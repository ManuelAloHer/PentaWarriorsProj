using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.PlayerSettings;
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
    public List<PathNode> possibleAtackNodes = new List<PathNode>();
    public List<Vector3Int> targetPositions;
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
        possibleNodes = transitableNodes;
        if (!controlledObject.GetAliance().Equals(Aliance.Player)) 
        {
            
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
        if (targetPositions == null)
        {
            targetPositions = new List<Vector3Int>();
        }
        else
        {
            targetPositions.Clear();
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

                            if (isTransitable || hasEntity)
                            {
                                targetPositions.Add(pos);

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

                            if (isTransitable || hasEntity)
                            {
                                targetPositions.Add(pos);
                            }
                        }
                    }
                }
            }
        }
        targetPositions = FilterLineOfSightTargets(character, targetPositions, targetAliance);
        //List<LineOfSightResult> lineOfSight = FilterLineOfSightWithCover(character,targetPos);
        if (!character.characterAliance.Equals(Aliance.Player))
        {
            //targetPos = FilterOnlyPlayerTargets(targetPos);
            return;
        }
        attackHighlight.Hide();
        attackHighlight.Highlight(targetPositions);
        highlight.Hide();
    }

    public List<Vector3Int> FilterLineOfSightTargets(Entity character, List<Vector3Int> targets, Aliance targetAliance)
    {
        List<Vector3Int> visibleTargets = new List<Vector3Int>();
        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

        if (controlledCharacter == null || targetGrid == null)
        {
            Debug.LogError("Character or targetGrid is null.");
            return visibleTargets;
        }

        Vector3Int origin = controlledCharacter.positionInGrid;
        Vector3 originWorld = targetGrid.GetWorldPosition(origin);

        int maxRange = character.AttackRange;
        int obstacleMask = LayerMask.GetMask("Obstacle", "VisibleObstacle", "InteractableObstacle", "Entity", "EntityBase");

        Queue<(Vector3Int pos, int dist)> frontier = new Queue<(Vector3Int, int)>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        frontier.Enqueue((origin, 0));
        visited.Add(origin);

        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),
        };

        while (frontier.Count > 0)
        {
            var (current, distance) = frontier.Dequeue();
            if (distance > maxRange)
                continue;

            if (!targetGrid.CheckBounderies(current))
                continue;
            bool somethingDied = false;

            if (current != origin)
            {
                Vector3 currentWorld = targetGrid.GetWorldPosition(current);
                Vector3 dir = (currentWorld - originWorld).normalized;
                float dist = Vector3.Distance(originWorld, currentWorld);

                // Perform raycast
                if (Physics.Raycast(originWorld, dir, out RaycastHit hit, dist, obstacleMask))
                {
                    ObjectInGrid hitObject = hit.collider.GetComponentInParent<ObjectInGrid>();

                    // Skip self
                    if (hitObject != null && hitObject == controlledCharacter)
                        continue;
                    //SkipDead
                    if (hitObject != null && hitObject.CheckIfSomethingDead())
                    {
                        somethingDied = true;
                        continue;
                    }
                    Vector3Int hitGrid = targetGrid.GetGridPosition(hit.point);

                    // If we hit an entity directly on the target cell, check if it's a valid target
                    if (hitGrid == current)
                    {
                        if (targetGrid.CheckEntityRootPresence(current.x, current.y, current.z))
                        {
                            Aliance hitAliance = targetGrid.GetAlianceInNode(current);
                            if (targetAliance == Aliance.None || hitAliance == targetAliance && !somethingDied)
                            {
                                visibleTargets.Add(current); // valid target
                            }
                        }
                    }

                    // Spread is blocked regardless of what was hit
                    Debug.DrawLine(originWorld, hit.point, Color.red, 1f);
                    continue;
                }
            }

            if (current != origin && !visibleTargets.Contains(current))
            {
                var node = targetGrid.GetNode(current);
                if (node.onAir && !node.entityOcupied) { continue; }

                visibleTargets.Add(current);
            }

            // Empty/unblocked tile — add to list
            //if (!visibleTargets.Contains(current))
            //{
            //    if (targetGrid.GetNode(current).onAir && !targetGrid.GetNode(current).entityOcupied) { continue; }
            //    visibleTargets.Add(current);
            //}

            // Continue spread from here
            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    frontier.Enqueue((neighbor, distance + 1));
                }
            }
        }

        return visibleTargets;

        #region Deprecated versions
        //List<Vector3Int> filtered = new List<Vector3Int>();
        //ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

        //Vector3 casterOriginWorld = targetGrid.GetWorldPosition(controlledCharacter.positionInGrid); // Start point
        //// Adjust layer mask as needed (e.g., walls, entities, environment)
        //int obstacleMask = LayerMask.GetMask("Obstacle", "VisibleObstacle", "InteractableObstacle", "EntityBase"); //, "Entity"); );


        //foreach (Vector3Int target in targets)
        //{
        //    Vector3 targetWorld = targetGrid.GetWorldPosition(target);
        //    Vector3 direction = (targetWorld - casterOriginWorld).normalized;
        //    float distance = Vector3.Distance(casterOriginWorld, targetWorld);

        //    bool allowTarget = false;

        //    // Determine if the target tile contains a valid entity
        //    bool targetHasEntity = targetGrid.CheckEntityRootPresence(target.x, target.y, target.z);
        //    bool isTargetValid = false;

        //    if (targetHasEntity)
        //    {
        //        Aliance targetAlianceInGrid = targetGrid.GetAlianceInNode(target);
        //        if (targetAliance == Aliance.None || targetAlianceInGrid == targetAliance)
        //        {
        //            isTargetValid = true;
        //        }
        //    }

        //    // RaycastAll to detect any blockers
        //    RaycastHit[] hits = Physics.RaycastAll(casterOriginWorld, direction, distance, obstacleMask);
        //    RaycastHit? firstHit = hits.OrderBy(h => h.distance).FirstOrDefault();

        //    if (firstHit.HasValue)
        //    {
        //        var hit = firstHit.Value;

        //        ObjectInGrid hitObject = hit.collider ? hit.collider.GetComponentInParent<ObjectInGrid>() : null;
        //        Vector3Int hitGridPos = targetGrid.GetGridPosition(hit.point);

        //        if (hitObject != null && hitObject == controlledCharacter)
        //        {
        //            continue; // ignore self
        //        }

        //        // If the first hit is on the target cell (entity or tile), accept it
        //        if (targetGrid.GetGridPosition(hit.point) == target)
        //        {
        //            allowTarget = true;
        //            Debug.DrawLine(casterOriginWorld, targetWorld, Color.green, 1f);
        //        }
        //        else
        //        {
        //            Debug.DrawLine(casterOriginWorld, hit.point, Color.red, 1f);
        //        }
        //    }
        //    else
        //    {
        //        // No hit at all, target is visible
        //        allowTarget = true;
        //        Debug.DrawLine(casterOriginWorld, targetWorld, Color.green, 1f);
        //    }

        //    if (allowTarget || isTargetValid)
        //    {
        //        filtered.Add(target);
        //    }
        //}

        //return filtered;

        //foreach (Vector3Int target in targets)
        //{
        //    Vector3 targetWorld = targetGrid.GetWorldPosition(target);

        //    Vector3 direction = (targetWorld - casterOriginWorld).normalized;
        //    float distance = Vector3.Distance(casterOriginWorld, targetWorld);

        //    bool blocked = false;
        //    bool hasEntity = false;

        //    RaycastHit[] hits = Physics.RaycastAll(casterOriginWorld, direction, distance, obstacleMask);
        //    foreach (var hit in hits)
        //    {
        //        // Check if hit is your own character — ignore it
        //        ObjectInGrid hitObject = hit.collider.GetComponentInParent<ObjectInGrid>();
        //        if (hitObject != null)
        //        {
        //            hasEntity = hitObject.ConfirmEntity();
        //            if (hitObject == controlledCharacter) 
        //            {
        //                continue; // ignore self
        //            }
        //            if (!hitObject.GetAliance().Equals(Aliance.None) && targetGrid.GetAlianceInNode(target) != Aliance.None)
        //            {
        //                if (!targetAliance.Equals(targetGrid.GetAlianceInNode(target)))
        //                {
        //                    hasEntity = false;
        //                }
        //            }

        //        }

        //        // Hit something that blocks LOS
        //        blocked = true;

        //        Debug.DrawLine(casterOriginWorld, targetWorld, Color.red, 1f);
        //        break;
        //    }

        //    if (!blocked || hasEntity)
        //    {
        //        filtered.Add(target);
        //        Debug.DrawLine(casterOriginWorld, targetWorld, Color.green, 1f);
        //    }
        //}

        //return filtered;
        #endregion
    }
    public List<Vector3Int> CalculateSingleTargetAreaAtPosition(Vector3Int origin, int attackRange, Entity attacker, Aliance targetAliance)
    {
        List<Vector3Int> rawTargets = new List<Vector3Int>();

        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                for (int z = -attackRange; z <= attackRange; z++)
                {
                    int distance = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                    if (distance > attackRange) continue;

                    Vector3Int pos = origin + new Vector3Int(x, y, z);
                    if (!targetGrid.CheckBounderies(pos)) continue;

                    bool isTransitable = !targetGrid.GetNode(pos).onAir && !targetGrid.GetNode(pos).obstructed;
                    bool hasEntity = targetGrid.CheckEntityRootPresence(pos.x, pos.y, pos.z);

                    if (isTransitable || hasEntity)
                    {
                        rawTargets.Add(pos);
                    }
                }
            }
        }

        return FilterLineOfSightTargetsAt(origin, attacker, rawTargets, targetAliance);
    }
    public List<Vector3Int> FilterLineOfSightTargetsAt(Vector3Int origin, Entity character,List<Vector3Int> targets, Aliance targetAliance)
    {
        List<Vector3Int> visibleTargets = new List<Vector3Int>();
        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

        if (controlledCharacter == null || targetGrid == null)
        {
            Debug.LogError("Character or targetGrid is null.");
            return visibleTargets;
        }

        Vector3 originWorld = targetGrid.GetWorldPosition(origin);

        
        int maxRange = character.AttackRange;
        int obstacleMask = LayerMask.GetMask("Obstacle", "VisibleObstacle", "InteractableObstacle", "Entity", "EntityBase");

        Queue<(Vector3Int pos, int dist)> frontier = new Queue<(Vector3Int, int)>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        frontier.Enqueue((origin, 0));
        visited.Add(origin);

        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),
        };

        while (frontier.Count > 0)
        {
            var (current, distance) = frontier.Dequeue();
            if (distance > maxRange)
                continue;

            if (!targetGrid.CheckBounderies(current))
                continue;

            bool somethingDied = false;
            if (current != origin)
            {
                Vector3 currentWorld = targetGrid.GetWorldPosition(current);
                Vector3 dir = (currentWorld - originWorld).normalized;
                float dist = Vector3.Distance(originWorld, currentWorld);


                //Perform raycast
                if (Physics.Raycast(originWorld, dir, out RaycastHit hit, dist, obstacleMask))
                {
                    ObjectInGrid hitObject = hit.collider.GetComponentInParent<ObjectInGrid>();

                    // Skip self
                    if (hitObject != null && hitObject == controlledCharacter)
                        continue;
                    //SkipDead
                    if (hitObject != null && hitObject.CheckIfSomethingDead()) 
                    {
                        somethingDied = true;
                        continue;
                    }
                        
                    Vector3Int hitGrid = targetGrid.GetGridPosition(hit.point);

                    // If we hit an entity directly on the target cell, check if it's a valid target
                    if (hitGrid == current)
                    {
                        if (targetGrid.CheckEntityRootPresence(current.x, current.y, current.z))
                        {
                            Aliance hitAliance = targetGrid.GetAlianceInNode(current);
                            if (targetAliance == Aliance.None || hitAliance == targetAliance && !somethingDied)
                            {
                                visibleTargets.Add(current); // valid target
                            }
                        }
                    }

                    // Spread is blocked regardless of what was hit
                    Debug.DrawLine(originWorld, hit.point, Color.red, 1f);
                    continue;
                }
            }

            if (current != origin && !visibleTargets.Contains(current))
            {
                var node = targetGrid.GetNode(current);
                if (node.onAir && !node.entityOcupied) { continue; }

                visibleTargets.Add(current);
            }

            // Empty/unblocked tile — add to list
            //if (!visibleTargets.Contains(current))
            //{
            //    if (targetGrid.GetNode(current).onAir && !targetGrid.GetNode(current).entityOcupied) { continue; }
            //    visibleTargets.Add(current);
            //}


            // Continue spread from here
            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    frontier.Enqueue((neighbor, distance + 1));
                }
            }
        }

        return visibleTargets;
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

    public void CalculateMultipleTargetArea(Entity character, Aliance targetAliance)
    {
        ObjectInGrid controlledCharacter = character.GetComponent<ObjectInGrid>();

        Vector3Int origin = controlledCharacter.positionInGrid;

        if (targetPositions == null)
        {
            targetPositions = new List<Vector3Int>();
        }
        else
        {
            targetPositions.Clear();
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
                                targetPositions.Add(pos);
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
        attackHighlight.Highlight(targetPositions);
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
        return targetPositions.Contains(positionOnGrid);
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
        GridNode placeToMoveNode = targetGrid.GetNode(placeToMove);
        foreach (PathNode possible in possibleNodes)
        {
            positionsToCheck.Add(new Vector3Int(possible.pos_x, possible.pos_y, possible.pos_z));
        }
        if (positionsToCheck.Contains(placeToMove) && placeToMoveNode.obstructed)
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
                placeToMoveNode = targetGrid.GetNode(pos);
                if (placeToMoveNode != null && placeToMoveNode.obstructed != true) 
                {
                    minDistance = distance;
                    closest = pos;
                }
            }
        }
        return closest;

    }

}
