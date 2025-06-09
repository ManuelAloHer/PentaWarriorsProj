using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour // Añadir un posible circulo para controlar rendierizado tiene que salvar de no renderizar a entities Terrain y ¿VisibleObstacles?
{
    [SerializeField] Transform marker;
    [SerializeField] MeshRenderer markerRenderer;
    [SerializeField] CommandInput commandInput;
    [SerializeField] InputController inputControl;
    [SerializeField] Transform AdEHighLighter;
    [SerializeField] Material[] markerMaterials;
    [SerializeField] Material[] highlightMaterials;

    Vector3Int currentPosition;
    bool activeCursor;
    bool activeAreaHighlight = false;
    [SerializeField] GridMap targetGrid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeCursor != commandInput.cursorNeeded)
        {
            activeCursor = commandInput.cursorNeeded;
            marker.gameObject.SetActive(activeCursor);
            
        }
        if (activeAreaHighlight != commandInput.showSpecialHighlight) 
        {
            activeAreaHighlight = commandInput.showSpecialHighlight;
            AdEHighLighter.gameObject.SetActive(activeAreaHighlight);
        }
        if (activeCursor == false) {return;}
        
        if (currentPosition != inputControl.PosOnGrid) 
        { 
           currentPosition = inputControl.PosOnGrid;
           UpdateMarker();
        }
        
    }
    private void UpdateMarker() 
    {
        Vector3 worldPosition = targetGrid.GetWorldPosition(currentPosition.x, currentPosition.y, currentPosition.z);
        marker.position = worldPosition;
        markerRenderer.material = markerMaterials[commandInput.activeNodeAliance];

        //if (activeAreaHighlight) 
        //{ 
            
        //}


    }
}
