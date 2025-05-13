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


    public void AllowTurn() 
    { 
        currentTurnActions = allowedTurnActions;
        currentCardTurnActions = allowedCardTurnActions;
    }

    public void ConsumeAction() 
    {
        currentTurnActions--;
    }
    public void ConsumeCardAction()
    {
        currentTurnActions--;
    }

    // Start is called before the first frame update
    void Start()
    {
        AllowTurn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
