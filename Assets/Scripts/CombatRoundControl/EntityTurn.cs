using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTurn : MonoBehaviour
{
    [Header ("ENTITY MAX ACTIONS PER TURN")]
    [SerializeField] int allowedTurnActions = 2;
    [SerializeField] int allowedCardTurnActions = 1;
    [Header("CURRENT ACTIONS")]
    public int currentTurnActions;
    public int currentCardTurnActions;
    public bool turnEnded = false;

    

    public void AllowTurn() 
    {
        turnEnded = false;
        currentTurnActions = allowedTurnActions;
        currentCardTurnActions = allowedCardTurnActions;
    }

    public void ConsumeAction() 
    {
        if (currentTurnActions <= 0) 
        {
            currentTurnActions = 0;
            return; 
        }
        currentTurnActions--;
    }
    public void ConsumeCardAction()
    {
        if (currentCardTurnActions <= 0) 
        {
            currentCardTurnActions = 0;
            return; 
        }
        currentCardTurnActions--;
    }
    public void ConsumeAllActions()
    {
        currentTurnActions = 0;
        currentCardTurnActions = 0;
    }
}
