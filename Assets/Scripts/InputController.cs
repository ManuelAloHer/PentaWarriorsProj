using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public interface ICursorInputProvider
{
    Vector2 GetCursorPosition();       // Where the cursor is (screen space)
    Vector2 GetCursorDelta();          // Movement delta this frame
    bool IsConfirmPressed();           // e.g. left click or gamepad A
    bool WasConfirmPressed();          // Just pressed this frame
    bool WasConfirmReleased();         // Just released
    bool IsCancelPressed();           // e.g. left click or gamepad A
    bool WasCancelPressed();          // Just pressed this frame
    bool WasCancelReleased();         // Just released
}
public class InputController : MonoBehaviour, ICursorInputProvider
{
    [SerializeField] Camera mainCamera;

    public static InputController Instance { get; private set; }
    
    private Vector2 simulatedCursorPosition;
    private bool usingGamepad;
    public Image simCursor;

    private PlayerInput inputActions;

    private InputAction cursorPosition;
    private InputAction cursorDelta;
    private InputAction confirmAction;
    private InputAction cancelAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        inputActions = new PlayerInput();
        inputActions.Enable();

        simulatedCursorPosition = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    private void OnEnable()
    {
        cursorPosition = inputActions.MainCombatMap.CursorMovement;
        cursorDelta = inputActions.MainCombatMap.CursorDelta;
        confirmAction = inputActions.MainCombatMap.ConfirmAction;
        cancelAction = inputActions.MainCombatMap.CancelAction;
    }

    private void OnDisable()
    {
        
    }
    private void Update()
    {
        
        Vector2 delta = cursorDelta.ReadValue<Vector2>();

        usingGamepad = Gamepad.current != null && delta != Vector2.zero;

        if (usingGamepad)
        {
            simulatedCursorPosition += delta * 1000f * Time.deltaTime;
            simulatedCursorPosition = ClampToScreen(simulatedCursorPosition);
            simCursor.transform.position = simulatedCursorPosition;
        }
        else if (Mouse.current != null)
        {
            simulatedCursorPosition = cursorPosition.ReadValue<Vector2>();
        }
    }

    private Vector2 ClampToScreen(Vector2 pos)
    {
        pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0, Screen.height);
        return pos;
    }

    // --- Public API ---

    public Vector2 GetCursorPosition() => simulatedCursorPosition;
    public Vector2 GetCursorDelta() => cursorDelta.ReadValue<Vector2>();
    public bool IsConfirmPressed() => confirmAction.IsPressed();
    public bool WasConfirmPressed() => confirmAction.WasPressedThisFrame();
    public bool WasConfirmReleased() => confirmAction.WasReleasedThisFrame();
    public bool IsCancelPressed() => cancelAction.IsPressed();
    public bool WasCancelPressed() => cancelAction.WasPressedThisFrame();
    public bool WasCancelReleased() => cancelAction.WasReleasedThisFrame();
    public bool IsUsingGamepad() => usingGamepad;
}
