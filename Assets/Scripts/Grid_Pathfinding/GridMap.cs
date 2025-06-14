using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

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
    [SerializeField] bool seeGizmos = false;

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
        CheckWalkableTerrain();
    }

    private void CheckWalkableTerrain()
    {
        Vector3 halfExtents = Vector3.one * cellSize / 4f;

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
                            if (obj.positionInGrid == new Vector3Int(x, y, z)) { grid[x, y, z].isObjectRoot = true; }
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
        if (!seeGizmos) { return; }
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
                        if (grid[x, y, z].isObjectRoot)
                        {
                            Gizmos.color = Color.cyan;
                        }
                        Gizmos.DrawCube(pos, Vector3.one / 4);
                    }
                }
            }
        }
    }

    public Vector3 GetWorldPosition(Vector3Int position)
    {
        //This are for game in witch the grid is no places at 0,0,0 position
        return new Vector3(transform.position.x + (position.x * cellSize), transform.position.y + (position.z * cellSize), transform.position.z + (position.y * cellSize));

        //return new Vector4(x * cellSize, elevation == true ? grid[x,y,z].altitude : 0, y * cellSize);
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
                        if (x == 0 && y == 0 && z == 0) { grid[positionInGrid.x + x, positionInGrid.y + y, positionInGrid.z + z].isObjectRoot = true; }
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
            //Debug.LogFormat("Node {0},{1},{2}: HasObjectInGrid {3}, Obstructed: {4}, isThereAnEntity: {5}, isOnAir: {6}, CurrentNodeState: {7} ",
            //gridPosition.x, gridPosition.y, gridPosition.z,
            //gridNode.objectInGrid != null, gridNode.obstructed, 
            //gridNode.entityOcupied, gridNode.onAir, gridNode.currentNodeState);
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
    public Aliance CheckObjectAliance(ObjectInGrid objectToTest)
    {

        if (objectToTest == null) { return Aliance.None; }
        return objectToTest.GetAliance();
    }

    public Aliance GetAlianceInNode(Vector3Int pos)
    {
        GridNode gridToTry = grid[pos.x, pos.y, pos.z];
        if (gridToTry == null || gridToTry.objectInGrid == null) { return Aliance.None; }
        return gridToTry.objectInGrid.GetAliance();

    }

    public bool CheckEntityRootPresence(int pos_x, int pos_y, int pos_z)
    {
        bool isEntity = false;
        GridNode gridToTry = grid[pos_x, pos_y, pos_z];
        if (gridToTry == null) { return isEntity; }
        isEntity = gridToTry.entityOcupied && gridToTry.isObjectRoot;
        return isEntity;
    }
    public bool CheckDeadEntity(int pos_x, int pos_y, int pos_z)
    {
        bool isDead = false;
        GridNode gridToTry = grid[pos_x, pos_y, pos_z];
        if (gridToTry == null) { return isDead; }
        isDead = !gridToTry.objectInGrid.GetEntity().IsAlive();
        return isDead;
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
