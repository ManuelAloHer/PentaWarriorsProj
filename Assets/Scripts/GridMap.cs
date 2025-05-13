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

    private void CalculateNodeAltitude()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < lenght; y++)
            {
                for (int z = 0; z < height; z++)
                {
                    Ray ray = new Ray(GetWorldPosition(x, y, z) + Vector3.up * 1000f, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayer))
                    {
                        grid[x, y, z].altitude = hit.point.y;
                    }
                }
            }
        }
    }

    private void CheckWalkableTerrain()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < lenght; y++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector3 worldPos = GetWorldPosition(x, y, z);
                    bool entityOnIt = Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, entityLayer);
                    bool objectInGridOnIt = Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, interactiveObstacleLayer);
                    bool clear = !Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, obstacleLayer)
                                && !entityOnIt;

                    if (entityOnIt || objectInGridOnIt) 
                    {
                        
                        //grid[x, y, z].objectInGrid = Physics.OverlapBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, entityLayer).g;
                    }
                    grid[x, y, z].obstructed = !clear;
                    grid[x, y, z].entityOcupied = entityOnIt;

                    //grid[x, y, z].ObjectOcupation()

                    if (Physics.CheckBox(worldPos, Vector3.one / 2 * cellSize, Quaternion.identity, terrainLayer))
                    {
                        grid[x, y, z].onAir = false;
                    }
                }
            }
            
        }
    }
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
            ChangeAsociatedNodes(positionInGrid, objectInGrid); 
        }
        else 
        {
            Debug.Log("You are trying to position the Object out of bounderies");
        
        }
    }

    private void ChangeAsociatedNodes(Vector3Int positionInGrid, ObjectInGrid objectInGrid)
    {
        for (int x = 0; x < objectInGrid.objectDimensions.x; x++)
        {
            for (int y = 0; y < objectInGrid.objectDimensions.y; y++)
            {
                for (int z = 0; z < objectInGrid.objectDimensions.z; z++)
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
            }
        }
    }

    public void RemoveObject(Vector3Int positionInGrid, ObjectInGrid objectInGrid)
    {
        if (CheckBounderies(positionInGrid))
        {
            //if(grid[positionInGrid.x, positionInGrid.y, positionInGrid.z].objectInGrid == objectInGrid){ return; }
            grid[positionInGrid.x, positionInGrid.y, positionInGrid.z].ClearNode();
            //EmptyNodeState
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
