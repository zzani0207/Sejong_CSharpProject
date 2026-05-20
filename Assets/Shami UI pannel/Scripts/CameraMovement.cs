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
    [SerializeField] private Camera targetCamera;

    [Header("Camera Points")]
    [SerializeField] private Transform homeView;
    [SerializeField] private Transform[] views;

    [Header("Tween Settings")]
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private Ease ease = Ease.InOutSine;

    [Header("Manual Camera Control")]
    [SerializeField] private ManualCameraController manualCameraController;

    private Pose savedHomePose;

    private Transform CamTransform
    {
        get
        {
            if (targetCamera != null)
                return targetCamera.transform;

            return Camera.main.transform;
        }
    }

    public void SaveCurrentCameraPose()
    {
        Transform cam = CamTransform;
        savedHomePose = new Pose(cam.position, cam.rotation);
    }

    public void GoView(Parts part, Action onComplete = null)
    {
        Debug.Log($"Go to view {part}");
        MoveCameraTo(views[(int)part], onComplete);
    }

    public void GoHome(Action onComplete = null)
    {
        if (homeView == null)
        {
            Debug.LogWarning("Home View is not assigned.");
            return;
        }

        MoveCameraTo(homeView, onComplete);
    }

    public void ReturnToSavedHome(Action onComplete = null)
    {
        homeView.position = savedHomePose.position;
        homeView.rotation = savedHomePose.rotation;

        GoHome(onComplete);
    }

    private void MoveCameraTo(Transform targetView, Action onComplete = null)
    {
        if (targetView == null) return;

        Transform cam = CamTransform;

        if (manualCameraController != null)
            manualCameraController.SetFixedViewMode();

        Tween.StopAll(onTarget: cam);

        Sequence.Create()
            .Group(Tween.Position(cam, endValue: targetView.position, duration: duration, ease: ease))
            .Group(Tween.Rotation(cam, endValue: targetView.rotation, duration: duration, ease: ease))
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }
}