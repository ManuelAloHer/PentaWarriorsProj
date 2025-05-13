using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIClickHandler : MonoBehaviour
{
    [SerializeField] private InputController inputController;

    private void Update()
    {
        if (inputController.WasConfirmPressed())
        {
            TryClickUIElementUnderCursor();
        }
    }

    private void TryClickUIElementUnderCursor()
    {
        Vector2 cursorPosition = inputController.GetCursorPosition();

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = cursorPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.TryGetComponent(out Button button) && button.interactable)
            {
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                break; // Only trigger the topmost valid button
            }
        }
    }
}
