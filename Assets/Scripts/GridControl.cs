using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridControl : MonoBehaviour
{
    [SerializeField] GridMap targetGrid;
    [SerializeField] LayerMask terrainLayerMask;

    Vector3Int currentHoverPosition = new Vector3Int (-1,-1,-1);

    [SerializeField] ObjectInGrid hoveredOver;
    [SerializeField] SelectableObjectInGrid selectedObject;

    //Update is called once per frame
    void Update()
    {
        HoverOverObjectsInGrid();

        SelectHoveredObject();

        UnselectObject();
    }
    private void HoverOverObjectsInGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            Vector3Int gridPosition = targetGrid.GetGridPosition(hit.point);
            if (currentHoverPosition == gridPosition) { return; }

            ObjectInGrid objectInGrid = targetGrid.GetPlacedObject(gridPosition);
            if (objectInGrid != null)
            {
                hoveredOver = objectInGrid;
            }
        }
    }
    private void SelectHoveredObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hoveredOver == null) { return; }
            selectedObject = hoveredOver.GetComponent<SelectableObjectInGrid>();
        }
    }
    private void UnselectObject()
    {
        if (Input.GetMouseButtonDown(1))
        {
            selectedObject = null;
        }
    }
}
