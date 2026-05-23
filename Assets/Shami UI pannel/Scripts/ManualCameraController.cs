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
    [SerializeField] private bool moveOnlyWhileLookMode = true;

    [Header("Position Limit")]
    [SerializeField] private float minY = 0f;

    [Header("Interior Rotation Limit")]
    [SerializeField] private Vector2 interiorPitchLimit = new Vector2(-25f, 25f);
    [SerializeField] private Vector2 interiorYawLimit = new Vector2(-60f, 60f);

    private bool allowLook = true;
    private bool allowMove = true;
    private bool lookMode;

    private bool useInteriorRotationLimit;

    private float pitch;
    private float yaw;
    private float baseInteriorPitch;
    private float baseInteriorYaw;

    private void OnEnable()
    {
        SyncRotationState();
        ClampPosition();
    }

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

        ClampPosition();
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

        pitch -= mouseDelta.y * mouseSensitivity;
        yaw += mouseDelta.x * mouseSensitivity;

        if (useInteriorRotationLimit)
        {
            pitch = Mathf.Clamp(
                pitch,
                baseInteriorPitch + interiorPitchLimit.x,
                baseInteriorPitch + interiorPitchLimit.y
            );

            yaw = Mathf.Clamp(
                yaw,
                baseInteriorYaw + interiorYawLimit.x,
                baseInteriorYaw + interiorYawLimit.y
            );
        }

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
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

        ClampPosition();
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;

        if (pos.y < minY)
        {
            pos.y = minY;
            transform.position = pos;
        }
    }

    private void SyncRotationState()
    {
        Vector3 euler = transform.eulerAngles;

        pitch = NormalizeAngle(euler.x);
        yaw = NormalizeAngle(euler.y);
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }

    public void SetExteriorMode()
    {
        enabled = true;

        allowLook = true;
        allowMove = true;
        useInteriorRotationLimit = false;

        SyncRotationState();
        ClampPosition();
        SetLookMode(false);
    }

    public void SetInteriorFreeMode()
    {
        enabled = true;

        allowLook = true;
        allowMove = false;
        useInteriorRotationLimit = true;

        SyncRotationState();

        baseInteriorPitch = pitch;
        baseInteriorYaw = yaw;

        ClampPosition();
        SetLookMode(false);
    }

    public void SetFixedViewMode()
    {
        allowLook = false;
        allowMove = false;

        SetLookMode(false);
        ClampPosition();

        enabled = false;
    }
}