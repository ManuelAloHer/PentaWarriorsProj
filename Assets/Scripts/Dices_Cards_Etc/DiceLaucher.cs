using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceLaucher : MonoBehaviour
{
    public int BaseDiceLaunch()
    {
        int result = DiceThrow(2, 8);
        return result;
    }

    public int BaseDiceLaunchModified(int modifier) 
    {
        int result = DiceThrow(2, 8);
        return result + modifier;
    }

    public int d6DiceThrow(int modifier) 
    {
        int result = DiceThrow(1, 6);
        return result += modifier;
    }
    public int d8DiceThrow(int modifier)
    {
        int result = DiceThrow(1, 8);
        return result += modifier;
    }
    public int d10DiceThrow(int modifier)
    {
        int result = DiceThrow(1,10);
        return result += modifier;
    }
    public int DiceThrow(int numberDice, int DiceType) 
    {
        int maxValue = numberDice * DiceType;
        int result = Random.Range(numberDice, DiceType);
        return result;
    }
}
