using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridHighlight : MonoBehaviour
{
    GridMap grid;
    [SerializeField] bool isAdEHighlight = false;
    [SerializeField] GameObject highlightPoint;
    List<GameObject> highlightPointGOs;
    [SerializeField] GameObject highlightPointContainer;
    [SerializeField] Material highlightMaterial;
    [SerializeField] Material highlightMaterialSecund;
    private List<Vector3Int> previousHighlightPositions = new List<Vector3Int>();

    float pointOffset = 0.2f;
    private Vector3 highlightOffset;

    private void Awake()
    {
        if (isAdEHighlight == true)
        {
            grid = GetComponentInParent<GridMap>();
        }
        else 
        {
            grid = FindObjectOfType<GridMap>();
        }
        
        highlightPointGOs = new List<GameObject>();

    }
    private void Start()
    {
        highlightOffset = Vector3.up * pointOffset;
    }

    private GameObject CreatePointHighlight()
    {
        GameObject go = Instantiate(highlightPoint);
        go.transform.SetParent(highlightPointContainer.transform, true);
        go.SetActive(true);
        highlightPointGOs.Add(go);
        return go;
    }

    public bool Highlight(List<LineOfSightResult> listed)
    {
        for (int i = 0; i < listed.Count; i++)
        {
            Highlight(listed[i].target.x, listed[i].target.y, listed[i].target.z, GetPointGo(i, listed[i].cover));
        }
        DeactivateUnusedHighlights(listed.Count);
        return true;
    }
    public bool Highlight(List<Vector3Int> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Highlight(positions[i].x, positions[i].y, positions[i].z, GetPointGo(i));
        }
        DeactivateUnusedHighlights(positions.Count);
        return true;
    }
    public bool Highlight(List<PathNode> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Highlight(positions[i].pos_x, positions[i].pos_y, positions[i].pos_z, GetPointGo(i));
        }
        DeactivateUnusedHighlights(positions.Count);
        return true;
    }
    private GameObject GetPointGo(int i, CoverLevel cover)
    {
        if (i < highlightPointGOs.Count)
        {
            // Reuse existing
            GameObject newHighlightObject = highlightPointGOs[i];
            newHighlightObject.SetActive(true); // Make sure it's visible again
            return newHighlightObject;
        }

        // Create and add until we have enough
        while (highlightPointGOs.Count <= i)
        {
            CreatePointHighlight(); // Adds to highlightPointGOs inside
        }
        if (cover == CoverLevel.None)
        {
            highlightPointGOs[i].GetComponentInChildren<MeshRenderer>().material = highlightMaterial;
        }
        else if (cover == CoverLevel.Partial)
        {
            highlightPointGOs[i].GetComponentInChildren<MeshRenderer>().material = highlightMaterial;
        }
        return highlightPointGOs[i];

        //if (highlightPointGOs.Count < i) 
        //{
        //    highlightPointGOs[i].SetActive(true);
        //    return highlightPointGOs[i];
        //}
        //GameObject newHighlightObject = CreatePointHighlight();
        //newHighlightObject.SetActive(true);
        //return newHighlightObject;
    }
    private GameObject GetPointGo(int i) 
    {
        if (i < highlightPointGOs.Count)
        {
            // Reuse existing
            GameObject newHighlightObject = highlightPointGOs[i];
            newHighlightObject.SetActive(true); // Make sure it's visible again
            return newHighlightObject;
        }

        // Create and add until we have enough
        while (highlightPointGOs.Count <= i)
        {
            CreatePointHighlight(); // Adds to highlightPointGOs inside
        }
        return highlightPointGOs[i];

        //if (highlightPointGOs.Count < i) 
        //{
        //    highlightPointGOs[i].SetActive(true);
        //    return highlightPointGOs[i];
        //}
        //GameObject newHighlightObject = CreatePointHighlight();
        //newHighlightObject.SetActive(true);
        //return newHighlightObject;
    }

    private void Highlight(int x,int y,int z, GameObject highlightObject)
    {
       highlightObject.SetActive(true);
       Vector3 position = grid.GetWorldPosition(x,y,z);
       position += highlightOffset;
       highlightObject.transform.position = position;
    }
    public void DeactivateUnusedHighlights(int usedCount)
    {
        for (int i = usedCount; i < highlightPointGOs.Count; i++)
        {
            highlightPointGOs[i].SetActive(false);
        }
    }

    public void Hide()
    {
        for (int i = 0; i < highlightPointGOs.Count; i++) 
        { 
            highlightPointGOs [i].SetActive(false);
        }
    }
}
