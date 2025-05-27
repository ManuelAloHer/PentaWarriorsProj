using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceLaucher : MonoBehaviour
{
    public int BaseDiceLaunch()
    {
        int result = DiceTrow(2, 8);
        return result;
    }

    public int BaseDiceLaunchModified(int modifier) 
    {
        int result = DiceTrow(2, 8);
        return result + modifier;
    }

    public int d6DiceTrow(int modifier) 
    {
        int result = DiceTrow(1, 6);
        return result += modifier;
    }
    public int d8DiceTrow(int modifier)
    {
        int result = DiceTrow(1, 8);
        return result += modifier;
    }
    public int d10DiceTrow(int modifier)
    {
        int result = DiceTrow(1,10);
        return result += modifier;
    }
    public int DiceTrow(int numberDice, int DiceType) 
    {
        int maxValue = numberDice * DiceType;
        int result = Random.Range(numberDice, DiceType);
        return result;
    }
}
