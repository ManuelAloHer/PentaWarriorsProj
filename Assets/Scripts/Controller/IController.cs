using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IController
{
    void BeginTurn(Entity entity);
    void EndTurn(Entity entity); // Optional
}
