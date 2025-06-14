using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum NodeState { Normal, InFlames, Cursed, Blessed}
[Serializable]
public class GridNode
{
    public bool obstructed;
    public bool entityOcupied = false;
    public bool isObjectRoot = false;
    public bool onAir = true;
    public NodeState currentNodeState;
    public ObjectInGrid objectInGrid;

    public void ClearNode() 
    {
        entityOcupied = false;
        obstructed = false;
        isObjectRoot = false;
        objectInGrid = null;
    }
    public void EntityOcupation()
    {
        entityOcupied = true;
        obstructed = true;
    }
    public void ObjectOcupation()
    {
        entityOcupied = false;
        obstructed = true;
        
    }
    public void PlaceObjectInNode(ObjectInGrid ocupant)
    {
        objectInGrid = ocupant;
    }

    internal void Reset()
    {
        ClearNode();
        onAir = true;
    }
}
