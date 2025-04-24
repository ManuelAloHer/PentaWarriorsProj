using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileState { Normal, InFlames, Cursed, Blessed}
[Serializable]
public class TileNode
{
    public bool obstructed;
    public bool onAir = true;
    public NodeState currentNodeState;
    public ObjectInGrid objectInGrid;
    public float altitude;
}
