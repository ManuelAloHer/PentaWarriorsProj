using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClearUtility : MonoBehaviour
{
    [SerializeField] Pathfinding targetPF;
    [Header("1 for Movement\n2 for Attack")]

    [SerializeField] GridHighlight[] gridHighlighters; // 1 for MovementHighlight,
                                                       // 2 for AttackHighlight;

    public void ClearPathfinding() 
    {
        targetPF.Clear();
    }
    public void ClearGridHighlighter(int highlighterIndex) 
    {
        if (gridHighlighters[highlighterIndex] != null)
        {
            gridHighlighters[highlighterIndex].Hide();
        }
    }
}
