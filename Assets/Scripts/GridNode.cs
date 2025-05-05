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
    public bool onAir = true;
    public NodeState currentNodeState;
    public ObjectInGrid objectInGrid;
    public float altitude;

    public void ClearNode() 
    {
        entityOcupied = false;
        obstructed = false;
        objectInGrid = null;
    }
}
