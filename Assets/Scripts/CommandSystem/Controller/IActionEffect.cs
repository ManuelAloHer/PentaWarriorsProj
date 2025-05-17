using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionEffect
{
    void Play(Action onComplete);
}