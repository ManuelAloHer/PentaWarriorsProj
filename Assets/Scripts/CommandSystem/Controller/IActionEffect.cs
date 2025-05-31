using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionState { NotInActionYet, WaitingAnimation, CalculatingEffect, WaitTargetAnimations, ApplyEffect, Complete}
public interface IActionEffect
{
    ActionState State { get; set; }
    void Play(Action onComplete);
}