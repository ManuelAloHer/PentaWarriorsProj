using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

public class PathNode 
{
    public int pos_x;
    public int pos_y;
    public int pos_z;

    public float gValue;
    public float hValue;

    public PathNode parentNode;

    public float fValue
    {
        get { return gValue + hValue; }
    }

    public PathNode( int xPos, int yPos, int zPos) 
    {
        pos_x = xPos;
        pos_y = yPos;
        pos_z = zPos;   
    
    }

    public void Clear()
    {
        gValue = 0f;
        hValue = 0f;
        parentNode = null;
    }
}

[RequireComponent(typeof(GridMap))]
public class Pathfinding : MonoBehaviour
{
    private GridMap gridMap;
    PathNode[,,] pathNodes;

    int straightCost = 10;
    int diagonalCost2D = 14;
    int diagonalCost3D = 17;
    
    List<PathNode> pathOpenList = new List<PathNode>();
    List<PathNode> pathCloseList = new List<PathNode>();
    List<PathNode> neighbourNodes = new List<PathNode>();

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    private void Init()
    {
        if (gridMap == null) { gridMap = GetComponent<GridMap>(); }

        pathNodes = new PathNode[gridMap.Width, gridMap.Lenght, gridMap.Height];

        for (int x = 0; x < gridMap.Width; x++)
        {
            for (int y = 0; y < gridMap.Lenght; y++)
            {
                for (int z = 0; z < gridMap.Height; z++)
                {
                    pathNodes[x, y, z] = new PathNode(x,y,z);
                }
            }
        }

    }
    public void CalculateWalkableNodes(int startX, int startY, int startZ, float range ,ref List<PathNode> toHighLight) 
    {
        PathNode startNode = pathNodes[startX, startY, startZ];

        List<PathNode> OpenList = new List<PathNode>();
        HashSet<PathNode> CloseList = new HashSet<PathNode>();

        OpenList.Add(startNode);

        while (OpenList.Count > 0)
        {
            PathNode currentNode = OpenList[0];
            OpenList.RemoveAt(0);
            CloseList.Add(currentNode);

            List<PathNode> neighbourSpreadNodes = new List<PathNode>();

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        if (x == 0 && y == 0 && z == 0) { continue; }
                        if (gridMap.CheckBounderies(currentNode.pos_x + x, currentNode.pos_y + y, currentNode.pos_z + z) == false) { continue; }

                        neighbourSpreadNodes.Add(pathNodes[currentNode.pos_x + x, currentNode.pos_y + y, currentNode.pos_z + z]);
                    }
                }
            }
            for (int i = 0; i < neighbourSpreadNodes.Count; i++)
            {
                if (CloseList.Contains(neighbourSpreadNodes[i])) { continue; }
                if (gridMap.CheckTransitable(false, neighbourSpreadNodes[i].pos_x, neighbourSpreadNodes[i].pos_y, neighbourSpreadNodes[i].pos_z) == false) { continue; }

                float movementCost = currentNode.gValue + CalculateDistance3D(currentNode, neighbourSpreadNodes[i]);
                //Debug.Log(movementCost);
                if (movementCost > range) {continue; }
                

                if (!OpenList.Contains(neighbourSpreadNodes[i])
                    || movementCost < neighbourSpreadNodes[i].gValue)
                {
                    neighbourSpreadNodes[i].gValue = movementCost;
                    neighbourSpreadNodes[i].parentNode = currentNode;

                    if (!OpenList.Contains(neighbourSpreadNodes[i]))
                    {
                        OpenList.Add(neighbourSpreadNodes[i]);
                    }
                }
            }
            
        }
        if (toHighLight != null) { toHighLight.AddRange(CloseList); }
        
    }


    public List<PathNode> FindPath(int startX, int startY, int startZ, int endX, int endY, int endZ) 
    {
        if (!gridMap.CheckBounderies(startX, startY, startZ) || !gridMap.CheckBounderies(endX, endY, endZ))
        {
            //Debug.LogError("Start or End position is out of bounds!");
            return null;
        }
        PathNode startNode = pathNodes[startX,startY,startZ];
        //if(pathNodes.)
        PathNode endNode = pathNodes[endX,endY,endZ];
        
        pathOpenList.Clear();
        pathCloseList.Clear();

        pathOpenList.Add(startNode);

        while (pathOpenList.Count > 0) 
        {
            PathNode currentNode = pathOpenList[0];

            for (int i = 0; i < pathOpenList.Count; i++)
            {
                if (currentNode.fValue > pathOpenList[i].fValue) 
                { 
                    currentNode = pathOpenList[i];
                }
                if (currentNode.fValue == pathOpenList[i].fValue && currentNode.hValue > pathOpenList[i].hValue)
                {
                    currentNode = pathOpenList[i];
                }
            }
            pathOpenList.Remove(currentNode);
            pathCloseList.Add(currentNode);

            if (currentNode == endNode) 
            {
                return RetracePath(startNode, endNode);
            }

            neighbourNodes.Clear();

            for (int x = -1; x < 2; x++) 
            {
                for (int y = -1; y < 2; y++) 
                {
                    for (int z = -1; z < 2; z++) 
                    {
                        if (x == 0 && y == 0 && z == 0) { continue;}
                        if (gridMap.CheckBounderies(currentNode.pos_x + x, currentNode.pos_y + y, currentNode.pos_z + z) == false) { continue;}
                        
                        neighbourNodes.Add(pathNodes[currentNode.pos_x + x, currentNode.pos_y + y,currentNode.pos_z + z]);
                    }
                }
            }
            for (int i = 0; i < neighbourNodes.Count; i++)
            {
                if (pathCloseList.Contains(neighbourNodes[i])) { continue; }
                if (gridMap.CheckTransitable(false, neighbourNodes[i].pos_x, neighbourNodes[i].pos_y, neighbourNodes[i].pos_z) == false) { continue; }

                float movementCost = currentNode.gValue + CalculateDistance3D(currentNode, neighbourNodes[i]);
                
                HashSet<PathNode> pathOpenSet = new HashSet<PathNode>();

                if (!pathOpenSet.Contains(neighbourNodes[i])
                    || movementCost < neighbourNodes[i].gValue)
                {
                    neighbourNodes[i].gValue = movementCost;
                    neighbourNodes[i].hValue = CalculateDistance3D(neighbourNodes[i], endNode);
                    neighbourNodes[i].parentNode = currentNode;

                    if (!pathOpenSet.Contains(neighbourNodes[i])) 
                    {
                        pathOpenSet.Add(neighbourNodes[i]);
                        pathOpenList.Add(neighbourNodes[i]);
                    }
                }
            }

        }

        return null;
    }

    //private int CalculateDistance(PathNode currentNode, PathNode pathNode)
    //{
    //    int distX = Mathf.Abs(currentNode.pos_x - pathNode.pos_x);
    //    int distY = Mathf.Abs(currentNode.pos_y - pathNode.pos_y);

    //    if (distX > distY) { return diagonalCost2D + distY + straightCost + (distX - distY); }
    
    //    return diagonalCost2D + distX + straightCost + (distY - distX);
    //}

    private int CalculateDistance3D(PathNode currentNode, PathNode pathNode)
    {
        int distX = Mathf.Abs(currentNode.pos_x - pathNode.pos_x);
        int distY = Mathf.Abs(currentNode.pos_y - pathNode.pos_y);
        int distZ = Mathf.Abs(currentNode.pos_z - pathNode.pos_z);

        // Find the three smallest distances to determine movement cost
        int minDist = Mathf.Min(distX, Mathf.Min(distY, distZ));
        int midDist = distX + distY + distZ - Mathf.Max(distX, Mathf.Max(distY, distZ)) - minDist;
        int maxDist = Mathf.Max(distX, Mathf.Max(distY, distZ));

        // 3D diagonal movement first (cost 17 per step)
        // 2D diagonal movement second (cost 14 per step)
        // Straight movement last (cost 10 per step)

        return (minDist * diagonalCost3D) + ((midDist - minDist) * diagonalCost2D) + ((maxDist - midDist) * straightCost);
    }

    private List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path= new List<PathNode>();

        PathNode currentNode = endNode;
        while (currentNode != startNode) 
        { 
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();
        return path;
    }

    public List<PathNode> TraceBackPath( int x, int y, int z) 
    {
        Vector3Int positionToTest = new Vector3Int(x, y, z);
        if (gridMap.CheckBounderies(positionToTest) == false) { return null; }
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = pathNodes[x, y, z];
        while (currentNode.parentNode != null)
        {
            if (path.Contains(currentNode)) { continue; }
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();
        return path;
    }

    public void Clear()
    {
        for (int x = 0; x < gridMap.Width; x++)
        {
            for (int y = 0; y < gridMap.Lenght; y++)
            {
                for (int z = 0; z < gridMap.Height; z++)
                {
                    pathNodes[x, y, z].Clear();
                }
            }
        }
    }
}
