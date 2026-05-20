using UnityEngine;
using UnityEngine.InputSystem;

public class ManualCameraController : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private bool rightClickToggle = true;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float fastMultiplier = 2f;

    [Tooltip("true면 우클릭 시야 조작 모드일 때만 WASD 이동")]
    [SerializeField] private bool moveOnlyWhileLookMode = true;

    private bool allowLook = true;
    private bool allowMove = true;
    private bool lookMode;

    private void OnDisable()
    {
        SetLookMode(false);
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        if (mouse == null)
            return;

        HandleLookModeToggle(mouse);

        if (allowLook && lookMode)
            HandleLook(mouse);

        if (keyboard != null && allowMove)
            HandleMove(keyboard);
    }

    private void HandleLookModeToggle(Mouse mouse)
    {
        if (!allowLook)
        {
            SetLookMode(false);
            return;
        }

        if (rightClickToggle)
        {
            if (mouse.rightButton.wasPressedThisFrame)
                SetLookMode(!lookMode);
        }
        else
        {
            SetLookMode(mouse.rightButton.isPressed);
        }
    }

    private void SetLookMode(bool active)
    {
        lookMode = active;

        if (lookMode)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleLook(Mouse mouse)
    {
        Vector2 mouseDelta = mouse.delta.ReadValue();

        Vector3 euler = transform.eulerAngles;

        euler.x -= mouseDelta.y * mouseSensitivity;
        euler.y += mouseDelta.x * mouseSensitivity;
        euler.z = 0f;

        transform.eulerAngles = euler;
    }

    private void HandleMove(Keyboard keyboard)
    {
        if (moveOnlyWhileLookMode && !lookMode)
            return;

        Vector3 input = Vector3.zero;

        if (keyboard.wKey.isPressed)
            input += transform.forward;

        if (keyboard.sKey.isPressed)
            input -= transform.forward;

        if (keyboard.aKey.isPressed)
            input -= transform.right;

        if (keyboard.dKey.isPressed)
            input += transform.right;

        if (input.sqrMagnitude <= 0.001f)
            return;

        float speed = moveSpeed;

        if (keyboard.leftShiftKey.isPressed)
            speed *= fastMultiplier;

        transform.position += input.normalized * speed * Time.deltaTime;
    }

    public void SetExteriorMode()
    {
        enabled = true;

        allowLook = true;
        allowMove = true;

        SetLookMode(false);
    }

    public void SetInteriorFreeMode()
    {
        enabled = true;

        allowLook = true;
        allowMove = false;

        SetLookMode(false);
    }

    public void SetFixedViewMode()
    {
        allowLook = false;
        allowMove = false;

        SetLookMode(false);
        enabled = false;
    }
}