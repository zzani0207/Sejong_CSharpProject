using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VehicleInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private ManualCameraController manualCameraController;
    [SerializeField] private PanelManager panelManager;

    [Header("Raycast")]
    [SerializeField] private LayerMask exteriorPartMask;
    [SerializeField] private LayerMask interiorPartMask;
    [SerializeField] private float rayDistance = 100f;

    [Header("Mode")]
    [SerializeField] private VehicleArea currentArea = VehicleArea.Exterior;

    [Header("Exterior Position Filter")]
    [SerializeField] private bool useExteriorPositionFilter = true;

    [Tooltip("차량 좌/우 기준 X 중앙값")]
    [SerializeField] private float vehicleCenterX = -6.455f;

    [Tooltip("차량 전/후 기준 Z 중앙값")]
    [SerializeField] private float vehicleCenterZ = 4.5f;

    [Header("Debug Keys")]
    [SerializeField] private bool enableDebugModeKeys = true;

    private bool isRaycastBlocked;
    private bool isCameraTransitioning;

    private bool isExteriorPartFocused;
    private bool isInteriorPartFocused;

    public VehicleArea CurrentArea => currentArea;
    public bool IsInteriorMode => currentArea == VehicleArea.Interior;

    public event Action<VehicleArea> OnAreaChanged;

    private void Awake()
    {
        if (panelManager != null)
            panelManager.OnCloseRequested += CloseCurrentInteraction;
    }

    private void Start()
    {
        NotifyAreaChanged();
    }

    private void OnDestroy()
    {
        if (panelManager != null)
            panelManager.OnCloseRequested -= CloseCurrentInteraction;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CloseCurrentInteraction();
                return;
            }

            if (enableDebugModeKeys)
            {
                if (keyboard.iKey.wasPressedThisFrame)
                {
                    EnterInteriorMode();
                    return;
                }

                if (keyboard.oKey.wasPressedThisFrame)
                {
                    ExitInteriorMode();
                    return;
                }
            }
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition = mouse.position.ReadValue();
            TryClickVehiclePart(screenPosition);
        }
    }

    private void TryClickVehiclePart(Vector2 screenPosition)
    {
        if (targetCamera == null)
            return;

        // UI 버튼 위 클릭이면 차량 오브젝트 Raycast 실행 안 함
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 패널 열림 / 카메라 이동 중에는 추가 클릭 차단
        if (IsVehicleRaycastBlocked())
            return;

        Ray ray = targetCamera.ScreenPointToRay(screenPosition);
        LayerMask mask = GetCurrentMask();

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, mask))
            return;

        VehiclePartTarget target = hit.transform.GetComponentInParent<VehiclePartTarget>();

        if (target == null)
            return;

        if (target.Area != currentArea)
            return;

        Parts clickedPart = target.Part;

        if (currentArea == VehicleArea.Exterior)
        {
            if (!IsExteriorPartAllowedByCameraPosition(clickedPart))
            {
                Debug.Log($"Blocked by exterior position filter: {clickedPart}");
                return;
            }

            HandleExteriorPartClick(clickedPart);
        }
        else
        {
            HandleInteriorPartClick(clickedPart);
        }
    }

    private bool IsVehicleRaycastBlocked()
    {
        if (isRaycastBlocked)
            return true;

        if (isCameraTransitioning)
            return true;

        if (panelManager != null && panelManager.HasOpenPanel)
            return true;

        return false;
    }

    private LayerMask GetCurrentMask()
    {
        return currentArea == VehicleArea.Exterior
            ? exteriorPartMask
            : interiorPartMask;
    }

    private void HandleExteriorPartClick(Parts clickedPart)
    {
        isRaycastBlocked = true;
        isCameraTransitioning = true;

        isExteriorPartFocused = true;
        isInteriorPartFocused = false;

        cameraMovement.SaveCurrentCameraPose();

        if (panelManager != null)
            panelManager.OpenPanel(clickedPart);

        cameraMovement.GoView(clickedPart, () =>
        {
            isCameraTransitioning = false;

            // UI 패널이 열린 동안 카메라 이동/회전 고정
            if (manualCameraController != null)
                manualCameraController.SetFixedViewMode();
        });
    }

    private void HandleInteriorPartClick(Parts clickedPart)
    {
        isRaycastBlocked = true;
        isCameraTransitioning = true;

        isInteriorPartFocused = true;
        isExteriorPartFocused = false;

        cameraMovement.SaveCurrentCameraPose();

        if (panelManager != null)
            panelManager.OpenPanel(clickedPart);

        cameraMovement.GoView(clickedPart, () =>
        {
            isCameraTransitioning = false;

            // 내부 상세 View에서는 시야 고정
            if (manualCameraController != null)
                manualCameraController.SetFixedViewMode();
        });
    }

    private bool IsExteriorPartAllowedByCameraPosition(Parts part)
    {
        if (!useExteriorPositionFilter)
            return true;

        Transform reference = GetCameraPositionReference();

        if (reference == null)
            return true;

        Vector3 cameraPosition = reference.position;

        // 라이트는 X 좌우가 아니라 Z 전후 기준으로 제한
        if (IsLightPart(part))
        {
            return IsLightAllowedByCameraZ(part, cameraPosition.z);
        }

        // 라이트가 아닌 외부 파츠는 X 좌우 기준으로 제한
        if (cameraPosition.x < vehicleCenterX)
        {
            return IsLeftExteriorPart(part);
        }

        if (cameraPosition.x > vehicleCenterX)
        {
            return IsRightExteriorPart(part);
        }

        return true;
    }

    private Transform GetCameraPositionReference()
    {
        // Cinemachine 구조에서는 ManualCameraController가 붙은 CM_PlayerCamera 기준
        if (manualCameraController != null)
            return manualCameraController.transform;

        // fallback: 실제 렌더링 카메라
        if (targetCamera != null)
            return targetCamera.transform;

        return null;
    }

    private bool IsLightPart(Parts part)
    {
        return IsFrontLight(part) || IsRearLight(part);
    }

    private bool IsFrontLight(Parts part)
    {
        return part == Parts.FrontLightL ||
               part == Parts.FrontLightR;
    }

    private bool IsRearLight(Parts part)
    {
        return part == Parts.RearLightL ||
               part == Parts.RearLightR;
    }

    private bool IsLightAllowedByCameraZ(Parts part, float cameraZ)
    {
        // 현재 기준:
        // cameraZ > 4.5  -> 전면, 헤드라이트 클릭 허용
        // cameraZ < 4.5  -> 후면, 후미등 클릭 허용

        if (cameraZ > vehicleCenterZ)
        {
            return IsFrontLight(part);
        }

        if (cameraZ < vehicleCenterZ)
        {
            return IsRearLight(part);
        }

        return true;
    }

    private bool IsLeftExteriorPart(Parts part)
    {
        return part == Parts.LFTire ||
               part == Parts.LBTire ||
               part == Parts.LeftDoor;
    }

    private bool IsRightExteriorPart(Parts part)
    {
        return part == Parts.RFTire ||
               part == Parts.RBTire ||
               part == Parts.RightDoor;
    }

    private void CloseCurrentInteraction()
    {
        if (panelManager == null)
            return;

        // 닫는 중에도 추가 클릭 방지
        isRaycastBlocked = true;

        if (currentArea == VehicleArea.Exterior && isExteriorPartFocused)
        {
            panelManager.CloseCurrentPanel(() =>
            {
                isCameraTransitioning = true;

                cameraMovement.ReturnToSavedHome(() =>
                {
                    isCameraTransitioning = false;
                    isRaycastBlocked = false;

                    isExteriorPartFocused = false;
                    isInteriorPartFocused = false;

                    if (manualCameraController != null)
                        manualCameraController.SetExteriorMode();
                });
            });

            return;
        }

        if (currentArea == VehicleArea.Interior && isInteriorPartFocused)
        {
            panelManager.CloseCurrentPanel(() =>
            {
                isCameraTransitioning = true;

                cameraMovement.ReturnToSavedHome(() =>
                {
                    isCameraTransitioning = false;
                    isRaycastBlocked = false;

                    isInteriorPartFocused = false;
                    isExteriorPartFocused = false;

                    if (manualCameraController != null)
                        manualCameraController.SetInteriorFreeMode();
                });
            });

            return;
        }

        panelManager.CloseCurrentPanel(() =>
        {
            isRaycastBlocked = false;
            isCameraTransitioning = false;

            isExteriorPartFocused = false;
            isInteriorPartFocused = false;
        });
    }

    public void EnterInteriorMode()
    {
        if (IsVehicleRaycastBlocked())
            return;

        if (currentArea == VehicleArea.Interior)
            return;

        currentArea = VehicleArea.Interior;
        NotifyAreaChanged();

        isRaycastBlocked = true;
        isCameraTransitioning = true;

        isExteriorPartFocused = false;
        isInteriorPartFocused = false;

        if (panelManager != null)
            panelManager.CloseCurrentPanel();

        cameraMovement.SaveCurrentCameraPose();

        cameraMovement.GoView(Parts.DriverSeat, () =>
        {
            isCameraTransitioning = false;
            isRaycastBlocked = false;

            if (manualCameraController != null)
                manualCameraController.SetInteriorFreeMode();
        });
    }

    public void ExitInteriorMode()
    {
        if (isCameraTransitioning)
            return;

        if (currentArea == VehicleArea.Exterior)
            return;

        isRaycastBlocked = true;
        isCameraTransitioning = true;

        if (panelManager != null)
            panelManager.CloseCurrentPanel();

        cameraMovement.ReturnToSavedHome(() =>
        {
            currentArea = VehicleArea.Exterior;
            NotifyAreaChanged();

            isCameraTransitioning = false;
            isRaycastBlocked = false;

            isExteriorPartFocused = false;
            isInteriorPartFocused = false;

            if (manualCameraController != null)
                manualCameraController.SetExteriorMode();
        });
    }

    private void NotifyAreaChanged()
    {
        OnAreaChanged?.Invoke(currentArea);
    }
}