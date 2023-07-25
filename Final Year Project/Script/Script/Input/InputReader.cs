using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, IAPlayerControls.IPlayerControlsActions
{
    private IAPlayerControls _controls;
    public Vector3 MovementValue { get; private set; }
    public Vector2 CamRotation { get; private set; }
    [field: SerializeField] public GameObject Menu { get; private set; }

    private void Start()
    {
        _controls = new IAPlayerControls();
        _controls.PlayerControls.SetCallbacks(this);
        _controls.PlayerControls.Enable();
    }

    public void OnToggleMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Menu.activeSelf)
            {
                Menu.SetActive(false);
                ToggleCursor();
            }
            else
            {
                Menu.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector3>();
    }

    public void OnLooking(InputAction.CallbackContext context)
    {
        CamRotation = context.ReadValue<Vector2>();
    }

    public void OnNoClipToggle(InputAction.CallbackContext context)
    {
        if (context.performed && !Cursor.visible) ToggleNoClipEvent?.Invoke();
    }

    public void OnRespawn(InputAction.CallbackContext context)
    {
        if (context.performed && !Cursor.visible) RespawnEvent?.Invoke();
    }

    public void OnWorldSpawnRespawn(InputAction.CallbackContext context)
    {
        if (context.performed && !Cursor.visible) WorldSpawnRespawnEvent?.Invoke();
    }

    public void OnIncreaseSpeed(InputAction.CallbackContext context)
    {
        if (context.performed && !Cursor.visible) IncreaseMovementSpeed?.Invoke();
    }

    public void OnDecreaseSpeed(InputAction.CallbackContext context)
    {
        if (context.performed && !Cursor.visible) DecreaseMovementSpeed?.Invoke();
    }

    public void OnIncreaseSensitivity(InputAction.CallbackContext context)
    {
        if (context.performed && !Cursor.visible) IncreaseSensitivity?.Invoke();
    }

    public void OnDecreaseSensitivity(InputAction.CallbackContext context)
    {
        if (context.performed && !Cursor.visible) DecreaseSensitivity?.Invoke();
    }

    public void OnCursorToggle(InputAction.CallbackContext context)
    {
        if (context.performed) ToggleCursor();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started) RunStartEvent?.Invoke();

        if (context.canceled) RunEndEvent?.Invoke();
    }

    public event Action RespawnEvent;
    public event Action WorldSpawnRespawnEvent;
    public event Action ToggleNoClipEvent;
    public event Action IncreaseMovementSpeed;
    public event Action DecreaseMovementSpeed;
    public event Action IncreaseSensitivity;
    public event Action DecreaseSensitivity;
    public event Action RunStartEvent;
    public event Action RunEndEvent;

    private void ToggleCursor()
    { 
        Cursor.lockState = Cursor.visible ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !Cursor.visible;

    }
}