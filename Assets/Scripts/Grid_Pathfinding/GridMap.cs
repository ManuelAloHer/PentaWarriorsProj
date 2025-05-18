using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    [SerializeField] GridNode[,,] grid;
    [SerializeField] int width = 25;
    [SerializeField] int lenght = 25;
    [SerializeField] int height = 1;
    [SerializeField] float cellSize = 1f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask interactiveObstacleLayer;
    [SerializeField] LayerMask entityLayer;
    [SerializeField] LayerMask terrainLayer;

    public int Width { get {return width; } }
    public int Lenght { get { return lenght; } }
    public int Height { get {return height; } }

    // Start is called before the first frame update
    void Awake()
    {
        GenerateGrid();
    }

    private void GenerateGrid() 
    {
        grid = new GridNode[width, lenght, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < lenght; y++)
            {
                for (int z = 0; z < height; z++)
                {
                    grid[x, y, z] = new GridNode();
                }
            }
        }
        //CalculateNodeAltitude();
        CheckWalkableTerrain();
    }

    private void CheckWalkableTerrain()
    {
        Vector3 halfExtents = Vector3.one * cellSize / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < lenght; y++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector3 worldPos = GetWorldPosition(x, y, z);

                    // Overlap for everything relevant at this cell
                    Collider[] colliders = Physics.OverlapBox(worldPos, halfExtents, Quaternion.identity,
                        entityLayer | obstacleLayer | interactiveObstacleLayer | terrainLayer);

                    // Default: cell is empty
                    grid[x, y, z].Reset(); // You define this to clear flags

                    foreach (var col in colliders)
                    {
                        int colLayer = col.gameObject.layer;

                        if (((1 << colLayer) & entityLayer) != 0)
                        {
                            grid[x, y, z].entityOcupied = true;
                            grid[x, y, z].obstructed = true;
                            ObjectInGrid obj = col.transform.root.GetComponent<ObjectInGrid>(); // Checks root of hierarchy
                            if (obj == null)
                            {
                                Debug.Log("ObjectInGridNotFound");
                                continue;   
                            }
                            grid[x, y, z].objectInGrid = obj;
                        }
                        else if (((1 << colLayer) & obstacleLayer) != 0)
                        {
                            grid[x, y, z].obstructed = true;
                        }
                        else if (((1 << colLayer) & interactiveObstacleLayer) != 0)
                        {
                            grid[x, y, z].obstructed = true;

                            // Assign actual object in grid
                            ObjectInGrid obj = col.GetComponent<ObjectInGrid>();
                            if (obj == null)
                            {
                                Debug.Log("ObjectInGridNotFound");
                                continue;
                            }
                            grid[x, y, z].objectInGrid = obj;
                        }
                        else if (((1 << colLayer) & terrainLayer) != 0)
                        {
                            grid[x, y, z].onAir = false;
                        }
                    }
                }
            }
        }
    }

    //private void CalculateNodeAltitude() // Deprecated
    //{
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < lenght; y++)
    //        {
    //            for (int z = 0; z < height; z++)
    //            {
    //                Ray ray = new Ray(GetWorldPosition(x, y, z) + Vector3.up * 1000f, Vector3.down);
    //                RaycastHit hit;
    //                if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayer))
    //                {
    //                    grid[x, y, z].altitude = hit.point.y;
    //                }
    //            }
    //        }
    //    }
    //}
    //private void CheckWalkableTerrain()
    //{
    //    // Pre-clear all grid data (recommended)
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < lenght; y++)
    //        {
    //            for (int z = 0; z < height; z++)
    //            {
    //                grid[x, y, z].Reset(); // You define this method to reset flags
    //            }
    //        }
    //    }

    //    // Process all 4 layers efficiently
    //    ProcessLayer(entityLayer, (cell, col) =>
    //    {
    //        cell.entityOcupied = true;
    //        cell.obstructed = true;
    //    });

    //    ProcessLayer(obstacleLayer, (cell, col) =>
    //    {
    //        cell.obstructed = true;
    //    });

    //    ProcessLayer(interactiveObstacleLayer, (cell, col) =>
    //    {
    //        cell.obstructed = true;
    //        var objInGrid = col.GetComponent<ObjectInGrid>();
    //        if (objInGrid == null)
    //        {
    //            objInGrid = col.gameObject.AddComponent<ObjectInGrid>(); // Optional fallback
    //        }
    //        cell.objectInGrid = objInGrid;
    //    });

    //    ProcessLayer(terrainLayer, (cell, col) =>
    //    {
    //        cell.onAir = false;
    //    });
    //}
    //private void ProcessLayer(LayerMask layer, System.Action<GridNode, Collider> handleCollider)
    //{
    //    // The grid starts at transform.position — that's the world origin of the grid
    //    Vector3 origin = transform.position;

    //    // Half extents represent the size of the whole grid volume
    //    Vector3 fullSize = new Vector3(width * cellSize, height * cellSize, lenght * cellSize);
    //    Vector3 center = origin + fullSize / 2f;
    //    Vector3 halfExtents = fullSize / 2f;

    //    // Use OverlapBox to get all colliders in this layer within the grid volume
    //    Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, layer);

    //    foreach (var col in colliders)
    //    {
    //        Vector3Int gridPos = GetGridPosition(col.transform.position);

    //        if (CheckBounderies(gridPos.x, gridPos.y, gridPos.z))
    //        {
    //            GridNode cell = grid[gridPos.x, gridPos.y, gridPos.z];
    //            handleCollider(cell, col);
    //        }
    //    }

    //}
    //private void CheckWalkableTerrain()
    //{
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < lenght; y++)
    //        {
    //            for (int z = 0; z < height; z++)
    //            {
    //                Vector3 worldPos = GetWorldPosition(x, y, z);
    //                bool entityOnIt = Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, entityLayer);
    //                bool objectInGridOnIt = Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, interactiveObstacleLayer);
    //                bool clear = !Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, obstacleLayer)
    //                            && !entityOnIt;

    //                if (entityOnIt)
    //                {


    //                }
    //                else if ( objectInGridOnIt) 
    //                { 

    //                }
    //                grid[x, y, z].obstructed = !clear;
    //                grid[x, y, z].entityOcupied = entityOnIt;

    //                //grid[x, y, z].ObjectOcupation()

    //                if (Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, terrainLayer))
    //                {
    //                    grid[x, y, z].onAir = false;
    //                }
    //            }
    //        }

    //    }
    //}
    public Vector3Int GetGridPosition (Vector3 worldPosition) 
    {
        //This are for game in witch the grid is no places at 0,0,0 position
        worldPosition -= transform.position;
        worldPosition.x += cellSize / 2;
        worldPosition.y += cellSize / 2;
        worldPosition.z += cellSize / 2;
        Vector3Int gridPos = new Vector3Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize), (int)(worldPosition.y / cellSize));
        //Vector2Int gridPos = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));
        return gridPos;
    }
    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < lenght; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        Vector3 pos = GetWorldPosition(x, y, z);

                        Gizmos.DrawCube(pos, Vector3.one / 4);
                    }
                }
            }
            //return; 
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < lenght; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        Vector3 pos = GetWorldPosition(x, y, z);
                        Gizmos.color = grid[x, y, z].obstructed ? Color.red : Color.green;
                        if (grid[x, y, z].onAir && !grid[x, y, z].obstructed)
                        {
                            Gizmos.color = Color.blue;
                        }
                        if (grid[x, y, z].entityOcupied)
                        {
                            Gizmos.color = Color.magenta;
                        }
                        Gizmos.DrawCube(pos, Vector3.one / 4);
                    }
                }
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y, int z)
    {
        //This are for game in witch the grid is no places at 0,0,0 position
        return new Vector3(transform.position.x + (x * cellSize), transform.position.y + (z * cellSize), transform.position.z + (y * cellSize));

        //return new Vector4(x * cellSize, elevation == true ? grid[x,y,z].altitude : 0, y * cellSize);
    }

    public void PlaceObject(Vector3Int positionInGrid, ObjectInGrid objectInGrid)
    {
        if (CheckBounderies(positionInGrid))
        {
            ChangeAsociatedNodes(positionInGrid, objectInGrid,true); 
        }
        else 
        {
            Debug.Log("You are trying to position the Object out of bounderies");
        
        }
    }

    private void ChangeAsociatedNodes(Vector3Int positionInGrid, ObjectInGrid objectInGrid,bool IsPlacing)
    {
        for (int x = 0; x < objectInGrid.objectDimensions.x; x++)
        {
            for (int y = 0; y < objectInGrid.objectDimensions.y; y++)
            {
                for (int z = 0; z < objectInGrid.objectDimensions.z; z++)
                {
                    if (IsPlacing)
                    {
                        grid[positionInGrid.x + x, positionInGrid.y + y, positionInGrid.z + z].PlaceObjectInNode(objectInGrid);

                        if (objectInGrid.GetEntity() != null)
                        {
                            grid[positionInGrid.x + x, positionInGrid.y + y, positionInGrid.z + z].EntityOcupation();

                        }
                        else
                        {
                            grid[positionInGrid.x + x, positionInGrid.y + y, positionInGrid.z + z].ObjectOcupation();

                        }
                    }
                    else 
                    {
                        grid[positionInGrid.x + x, positionInGrid.y + y, positionInGrid.z + z].ClearNode();

                    }

                    
                }
            }
        }
    }

    public void RemoveObject(Vector3Int positionInGrid, ObjectInGrid objectInGrid)
    {
        
        if (CheckBounderies(positionInGrid))
        {
            ChangeAsociatedNodes(positionInGrid, objectInGrid,false);
        }
        else
        {
            Debug.Log("You are trying to position the Object out of bounderies");

        }
    }
    public bool CheckBounderies(Vector3Int positionOnGrid) 
    {
        if (positionOnGrid.x < 0 || positionOnGrid.x >= width) 
        {
            return false;
        }
        if (positionOnGrid.y < 0 || positionOnGrid.y >= lenght)
        {
            return false;
        }
        if (positionOnGrid.z < 0 || positionOnGrid.z >= height)
        {
            return false;
        }
        return true;
    }

    public bool CheckBounderies(int x, int y, int z)
    {
        return x >= 0 && x < width &&
           y >= 0 && y < lenght &&
           z >= 0 && z < height;
    }

    public ObjectInGrid GetPlacedObject(Vector3Int gridPosition)
    {
        ObjectInGrid gridObject = null;
        if (CheckBounderies(gridPosition))
        {
            gridObject = grid[gridPosition.x, gridPosition.y, gridPosition.z].objectInGrid;
            return gridObject;
        }
        return null;    
    }
    public void PrintNodeState(Vector3Int gridPosition) 
    {
        GridNode gridNode = null;
        ObjectInGrid gridObject = null;
        if (CheckBounderies(gridPosition))
        {
            gridNode = grid[gridPosition.x, gridPosition.y, gridPosition.z];
            gridObject = grid[gridPosition.x, gridPosition.y, gridPosition.z].objectInGrid;
            Debug.LogFormat("Node {0},{1},{2}: HasObjectInGrid {3}, Obstructed: {4}, isThereAnEntity: {5}, isOnAir: {6}, CurrentNodeState: {7} ",
            gridPosition.x, gridPosition.y, gridPosition.z,
            gridNode.objectInGrid != null, gridNode.obstructed, 
            gridNode.entityOcupied, gridNode.onAir, gridNode.currentNodeState);
        }
        
    }
    public GridNode GetNode(Vector3Int Node)
    {
        GridNode gridNode = null;
        if (CheckBounderies(Node))
        { 
            gridNode = grid[Node.x, Node.y, Node.z];
        }
        if (gridNode == null) { return null; }
        return gridNode;    
    }

    public bool CheckTransitable(bool ableOnAir, int pos_x, int pos_y, int pos_z)
    {
        bool transitable = false;
        GridNode gridToTry = grid[pos_x, pos_y, pos_z];
        if (gridToTry == null) { return transitable; }
        transitable = !gridToTry.onAir && !gridToTry.obstructed || gridToTry.onAir && ableOnAir && !gridToTry.obstructed;
        return transitable;

    }
    public bool CheckEntiyPresence(int pos_x, int pos_y, int pos_z)
    {
        bool isEntity = false;
        GridNode gridToTry = grid[pos_x, pos_y, pos_z];
        if (gridToTry == null) { return isEntity; }
        isEntity = gridToTry.entityOcupied;
        return isEntity;

    }

    public List<Vector3> ConvertPathNodesToWorldPositions(List<PathNode> path)
    {
       List<Vector3> worldPositions = new List<Vector3>();
       for (int i = 0; i < path.Count; i++)
       {
            worldPositions.Add(GetWorldPosition(path[i].pos_x, path[i].pos_y, path[i].pos_z));
       }
       return worldPositions;
    }


}
