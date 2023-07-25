using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [field: SerializeField] public float movementSpeed;
    private float _xRotation;
    private Camera Cam;
    private float cameraSensitivity = 0.1f;
    private CharacterController Controller;
    private InputReader InputReader;
    private bool isNoClip = true;

    private bool isRunning;

    private float verticalVelocity;

    // Start is called before the first frame update
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cam = GetComponentInChildren<Camera>();
        Controller = GetComponent<CharacterController>();
        InputReader = GetComponent<InputReader>();
        Controller.detectCollisions = false;
        Cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        InputReader.ToggleNoClipEvent += () =>
        {
            isNoClip = !isNoClip;
            Controller.enabled = !isNoClip;
            verticalVelocity = isNoClip ? -2 : verticalVelocity;
        };
        InputReader.WorldSpawnRespawnEvent += () =>
        {
            Controller.enabled = false;
            transform.position = new Vector3(-1f, 5f, -1f);
            isNoClip = true;
            verticalVelocity = -2;
        };
        InputReader.RespawnEvent += () =>
        {
            Controller.enabled = false;
            transform.position = GeneratorGlobalVals.Instance._worldSpawnLocation;
            isNoClip = false;
            Controller.enabled = true;
        };
        InputReader.IncreaseMovementSpeed += () => movementSpeed += 5;
        InputReader.DecreaseMovementSpeed += () => movementSpeed = Math.Max(0, movementSpeed - 5);
        InputReader.IncreaseSensitivity += () => cameraSensitivity += 0.02f;
        InputReader.DecreaseSensitivity += () => cameraSensitivity = Math.Max(0, cameraSensitivity - 0.02f);
        InputReader.RunStartEvent += () => isRunning = true;
        InputReader.RunEndEvent += () => isRunning = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Cursor.visible) return;
        var movement = Vector3.zero;
        movement.x = InputReader.MovementValue.x * movementSpeed;
        movement.y = InputReader.MovementValue.y * movementSpeed;
        movement.z = InputReader.MovementValue.z * movementSpeed;
        movement *= isRunning ? 2 : 1;
        var mouseX = InputReader.CamRotation.x;
        var mouseY = InputReader.CamRotation.y;
        _xRotation -= mouseY * cameraSensitivity;
        if (!isNoClip)
        {
            verticalVelocity += -9 * Time.deltaTime;
            if (Controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
            movement.y = verticalVelocity;
            Controller.Move(transform.TransformDirection(movement * Time.deltaTime));
            _xRotation = Mathf.Clamp(_xRotation, -70f, 70f);
        }
        else
        {
            transform.position += transform.TransformDirection(movement * Time.deltaTime);
        }

        Cam.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        transform.Rotate(Vector3.up * (mouseX * cameraSensitivity));
    }
}