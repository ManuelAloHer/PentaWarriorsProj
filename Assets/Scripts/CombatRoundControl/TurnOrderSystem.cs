using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TurnOrderSystem
{
    /// <summary>
    /// Calculates initiative for all entities and returns them sorted in descending order.
    /// </summary>
    public static List<Entity> CalculateTurnOrder(List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            entity.CalculateInitiative();
        }

        return entities
            .OrderByDescending(e => e.initiative)
            .ThenBy(e => e.gameObject.name)
            .ToList();
    }
}
