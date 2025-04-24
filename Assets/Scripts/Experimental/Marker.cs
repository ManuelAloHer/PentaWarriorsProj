using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField] Transform marker;

    [SerializeField] PlayerControl playerControl;

    Vector3Int currentPosition;
    bool activeCursor;
    [SerializeField] GridMap targetGrid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeCursor != playerControl.cursorNeeded)
        {
            activeCursor = playerControl.cursorNeeded;
            marker.gameObject.SetActive(activeCursor);
        }
        if(activeCursor == false) {return;}
        if (currentPosition != playerControl.positionOnGrid) 
        { 
            currentPosition = playerControl.positionOnGrid;
           UpdateMarker();
        }
        
    }
    private void UpdateMarker() 
    {
        Vector3 worldPosition = targetGrid.GetWorldPosition(currentPosition.x, currentPosition.y, currentPosition.z);
        marker.position = worldPosition;
    
    }
}
