using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommandInputStage
{
    bool NeedsExtraInput { get; }
    void RequestInput(Action onInputReady);
}
