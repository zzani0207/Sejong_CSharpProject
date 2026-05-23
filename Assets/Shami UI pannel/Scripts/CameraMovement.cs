using System;
using UnityEngine;
using PrimeTween;

public enum Parts
{
    LFTire,
    LBTire,
    RFTire,
    RBTire,

    LeftDoor,
    RightDoor,

    FrontLightL,
    FrontLightR,
    RearLightL,
    RearLightR,

    DriverSeat,
    Dashboard,
    Navigation
}

public class CameraMovement : MonoBehaviour
{
    [Header("Camera Target")]
    [SerializeField] private Transform cameraRoot;

    [Header("Camera Points")]
    [SerializeField] private Transform homeView;
    [SerializeField] private Transform[] views;

    [Header("Tween Settings")]
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private Ease ease = Ease.InOutSine;

    [Header("Position Limit")]
    [SerializeField] private float minCameraY = 0f;

    [Header("Manual Camera Control")]
    [SerializeField] private ManualCameraController manualCameraController;

    private Pose savedHomePose;

    private Transform CamTransform
    {
        get
        {
            if (cameraRoot != null)
                return cameraRoot;

            Debug.LogWarning("Camera Root is not assigned. Assign CM_PlayerCamera.");
            return null;
        }
    }

    public void SaveCurrentCameraPose()
    {
        Transform cam = CamTransform;

        if (cam == null)
            return;

        Vector3 clampedPosition = ClampCameraPosition(cam.position);
        savedHomePose = new Pose(clampedPosition, cam.rotation);
    }

    public void GoView(Parts part, Action onComplete = null)
    {
        Debug.Log($"Go to view {part}");

        int index = (int)part;

        if (views == null || index < 0 || index >= views.Length || views[index] == null)
        {
            Debug.LogWarning($"Camera view is not assigned for {part}");
            onComplete?.Invoke();
            return;
        }

        MoveCameraTo(views[index], onComplete);
    }

    public void GoHome(Action onComplete = null)
    {
        if (homeView == null)
        {
            Debug.LogWarning("Home View is not assigned.");
            onComplete?.Invoke();
            return;
        }

        MoveCameraTo(homeView, onComplete);
    }

    public void ReturnToSavedHome(Action onComplete = null)
    {
        if (homeView == null)
        {
            Debug.LogWarning("Home View is not assigned.");
            onComplete?.Invoke();
            return;
        }

        homeView.position = ClampCameraPosition(savedHomePose.position);
        homeView.rotation = savedHomePose.rotation;

        GoHome(onComplete);
    }

    private void MoveCameraTo(Transform targetView, Action onComplete = null)
    {
        if (targetView == null)
        {
            onComplete?.Invoke();
            return;
        }

        Transform cam = CamTransform;

        if (cam == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (manualCameraController != null)
            manualCameraController.SetFixedViewMode();

        Vector3 targetPosition = ClampCameraPosition(targetView.position);
        Quaternion targetRotation = targetView.rotation;

        Tween.StopAll(onTarget: cam);

        Sequence.Create()
            .Group(Tween.Position(
                cam,
                endValue: targetPosition,
                duration: duration,
                ease: ease
            ))
            .Group(Tween.Rotation(
                cam,
                endValue: targetRotation,
                duration: duration,
                ease: ease
            ))
            .OnComplete(() =>
            {
                ClampCameraRootInstant();
                onComplete?.Invoke();
            });
    }

    private Vector3 ClampCameraPosition(Vector3 position)
    {
        if (position.y < minCameraY)
            position.y = minCameraY;

        return position;
    }

    private void ClampCameraRootInstant()
    {
        Transform cam = CamTransform;

        if (cam == null)
            return;

        cam.position = ClampCameraPosition(cam.position);
    }
}