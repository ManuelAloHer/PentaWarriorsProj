using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraController : MonoBehaviour
{
    [SerializeField] float inputSensitivity = 1f;
    [SerializeField] float mouseSensitivity = 1f;

    [SerializeField] bool continuosScrolling = true;

    [SerializeField] Transform bottomLeftBorder, upperRightBorder;
    Vector3 processedCameraInput;
    Vector3 pointOfOrigin;

    private PlayerInput inputActions;
    private InputAction cursorPosition;
    private InputAction cameraMovement;
    private InputAction confirmAction;
    private InputAction viewPosAction;

    private bool isConfirmPressed = false;

    private void Awake()
    {
        inputActions = new PlayerInput();
        cursorPosition = inputActions.MainCombatMap.CursorMovement;
        cameraMovement = inputActions.MainCombatMap.CameraMovement;
        confirmAction = inputActions.MainCombatMap.ConfirmAction;
        viewPosAction = inputActions.MainCombatMap.CameraDisplaceButton;
    }

    private void OnEnable()
    {
        EnableControls();
    }

    private void OnDisable()
    {
        DisableControls();
    }

    private void EnableControls()
    {
        inputActions.Enable();
        cursorPosition.Enable();
        cameraMovement.Enable();
        confirmAction.Enable();
        viewPosAction.Enable();
    }

    private void DisableControls()
    {
        inputActions.Disable();
        cursorPosition.Disable();
        cameraMovement.Disable();
        confirmAction.Disable();
        viewPosAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        NullInput();
        MoveCameraInput();
        MoveCamera();
    }

    private void NullInput()
    {
        processedCameraInput = Vector3.zero;
    }

    private void MoveCameraInput()
    {
        AxisInput();
        CursorInput();
    }

    private void CursorInput()
    {
        if (viewPosAction.WasPressedThisFrame()) 
        { 
            pointOfOrigin = cursorPosition.ReadValue<Vector2>();
            isConfirmPressed = true;
        }
        if (viewPosAction.IsPressed() && isConfirmPressed)
        {
            Vector3 mouseInput = cursorPosition.ReadValue<Vector2>();
            processedCameraInput.x += (mouseInput.x - pointOfOrigin.x) * mouseSensitivity;
            processedCameraInput.z += (mouseInput.y - pointOfOrigin.y) * mouseSensitivity;
            if (continuosScrolling == false) 
            { 
                pointOfOrigin = mouseInput;
            }
        }
        if (viewPosAction.WasReleasedThisFrame())
        {
            isConfirmPressed = false;
        }
    }

    private void AxisInput()
    {
        processedCameraInput.x += cameraMovement.ReadValue<Vector2>().x * inputSensitivity;
        processedCameraInput.z += cameraMovement.ReadValue<Vector2>().y * inputSensitivity;
        
    }
    private void MoveCamera()
    {
        Vector3 position = transform.position;  
        position += (processedCameraInput * Time.deltaTime);
        position.x = Mathf.Clamp(position.x, bottomLeftBorder.position.x, upperRightBorder.position.x);
        position.z = Mathf.Clamp(position.z, bottomLeftBorder.position.z, upperRightBorder.position.z);

        transform.position = position;
    }

}
