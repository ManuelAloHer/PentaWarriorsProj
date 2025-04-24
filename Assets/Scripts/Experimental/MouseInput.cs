using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MouseInput : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [SerializeField] GridMap targetGrid;
    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] InputController inputCursor;
    
    public Vector3Int positionOnGrid;

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(inputCursor.GetCursorPosition());
        RaycastHit hit;
        if ( Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            Vector3Int gridPosition = targetGrid.GetGridPosition(hit.point);

            //This is for free of turn movement
            //path = pathfinding.FindPath(targetObject.positionInGrid.x, targetObject.positionInGrid.y, targetObject.positionInGrid.z,
            //                           gridPosition.x, gridPosition.y, gridPosition.z);
            if (gridPosition != positionOnGrid)
            {
                positionOnGrid = gridPosition;
            }
        }
    }
}
