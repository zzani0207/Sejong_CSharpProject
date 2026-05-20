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

    [Header("Debug Keys")]
    [SerializeField] private bool enableDebugModeKeys = true;

    private bool isExteriorPartFocused;
    private bool isInteriorPartFocused;

    // 핵심: 이 값이 true면 차량 오브젝트 Raycast 클릭 차단
    private bool isRaycastBlocked;

    // 카메라 이동 중에도 추가 클릭 방지
    private bool isCameraTransitioning;

    private void Awake()
    {
        if (panelManager != null)
            panelManager.OnCloseRequested += CloseCurrentInteraction;
    }

    private void OnDestroy()
    {
        if (panelManager != null)
            panelManager.OnCloseRequested -= CloseCurrentInteraction;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

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

        // UI 위를 클릭한 경우 차량 Raycast 막기
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 패널이 열려 있거나 카메라 이동 중이면 차량 오브젝트 클릭 차단
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

        if (panelManager != null && !panelManager.HasPanel(clickedPart))
        {
            Debug.LogWarning($"Panel is not assigned for {clickedPart}");
            return;
        }

        if (currentArea == VehicleArea.Exterior)
        {
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

    private void HandleExteriorPartClick(Parts clickedPart)
    {
        isRaycastBlocked = true;
        isCameraTransitioning = true;

        isExteriorPartFocused = true;
        isInteriorPartFocused = false;

        cameraMovement.SaveCurrentCameraPose();

        // UI 패널 Fade In
        if (panelManager != null)
            panelManager.OpenPanel(clickedPart);

        // 카메라 이동
        cameraMovement.GoView(clickedPart, () =>
        {
            isCameraTransitioning = false;

            if (manualCameraController != null)
                manualCameraController.SetExteriorMode();
        });
    }

    private void HandleInteriorPartClick(Parts clickedPart)
    {
        isRaycastBlocked = true;
        isCameraTransitioning = true;

        isInteriorPartFocused = true;
        isExteriorPartFocused = false;

        cameraMovement.SaveCurrentCameraPose();

        // UI 패널 Fade In
        if (panelManager != null)
            panelManager.OpenPanel(clickedPart);

        // 내부 오브젝트 클릭 시 해당 view로 이동 후 시야 고정
        cameraMovement.GoView(clickedPart, () =>
        {
            isCameraTransitioning = false;

            if (manualCameraController != null)
                manualCameraController.SetFixedViewMode();
        });
    }

    private LayerMask GetCurrentMask()
    {
        return currentArea == VehicleArea.Exterior
            ? exteriorPartMask
            : interiorPartMask;
    }

    private void CloseCurrentInteraction()
    {
        if (panelManager == null)
            return;

        // 닫는 도중에도 추가 Raycast 막기
        isRaycastBlocked = true;

        if (currentArea == VehicleArea.Exterior && isExteriorPartFocused)
        {
            panelManager.CloseCurrentPanel(() =>
            {
                isCameraTransitioning = true;

                cameraMovement.ReturnToSavedHome(() =>
                {
                    isCameraTransitioning = false;
                    isExteriorPartFocused = false;
                    isInteriorPartFocused = false;
                    isRaycastBlocked = false;

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
                    isInteriorPartFocused = false;
                    isExteriorPartFocused = false;
                    isRaycastBlocked = false;

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
        });
    }

    public void EnterInteriorMode()
    {
        if (IsVehicleRaycastBlocked())
            return;

        currentArea = VehicleArea.Interior;

        isRaycastBlocked = true;
        isCameraTransitioning = true;

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

        isExteriorPartFocused = false;
        isInteriorPartFocused = false;
    }

    public void ExitInteriorMode()
    {
        if (isCameraTransitioning)
            return;

        isRaycastBlocked = true;
        isCameraTransitioning = true;

        if (panelManager != null)
            panelManager.CloseCurrentPanel();

        cameraMovement.ReturnToSavedHome(() =>
        {
            currentArea = VehicleArea.Exterior;

            isCameraTransitioning = false;
            isRaycastBlocked = false;

            isExteriorPartFocused = false;
            isInteriorPartFocused = false;

            if (manualCameraController != null)
                manualCameraController.SetExteriorMode();
        });
    }
}