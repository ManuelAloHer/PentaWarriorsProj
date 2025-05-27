using System;
using UnityEngine;

public abstract class BaseActionEffect : MonoBehaviour, IActionEffect
{
    public ActionState State { get; set; } = ActionState.NotInActionYet;
    protected Action onComplete;

    public void Play(Action onComplete)
    {
        this.onComplete = onComplete;
        StartEffect();
    }

    protected abstract void StartEffect();

    protected void CompleteEffect()
    {
        onComplete?.Invoke();
    }
}