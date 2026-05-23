using UnityEngine;
using System.Collections;

public class RotatingDoor : MonoBehaviour, IDoor
{
    [Header("회전 설정")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closeAngle = 0f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isOpen = false;
    private float openProgress = 0f;
    private Coroutine animationCoroutine;
    private Quaternion closedRotation;
    private Quaternion openedRotation;

    public bool IsOpen => isOpen;
    public float OpenProgress => openProgress;

    private void Start()
    {
        closedRotation = Quaternion.Euler(closeAngle * rotationAxis);
        openedRotation = Quaternion.Euler(openAngle * rotationAxis);
        transform.localRotation = closedRotation;
    }

    public void Open()
    {
        if (isOpen) return;
        StopAnimation();
        animationCoroutine = StartCoroutine(AnimateDoor(1f));
        isOpen = true;
        if (audiomanager.Instance != null)
            audiomanager.Instance.PlayDoorOpenSound();
    }

    public void Close()
    {
        if (!isOpen) return;
        StopAnimation();
        animationCoroutine = StartCoroutine(AnimateDoor(0f));
        isOpen = false;
        if (audiomanager.Instance != null)
            audiomanager.Instance.PlayDoorOpenSound();
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    public void SetDoorState(float progress)
    {
        progress = Mathf.Clamp01(progress);
        openProgress = progress;
        
        Quaternion targetRotation = Quaternion.Lerp(closedRotation, openedRotation, progress);
        transform.localRotation = targetRotation;
    }

    private IEnumerator AnimateDoor(float targetProgress)
    {
        float elapsedTime = 0f;
        float startProgress = openProgress;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / animationDuration;
            float easedTime = animationCurve.Evaluate(normalizedTime);
            
            openProgress = Mathf.Lerp(startProgress, targetProgress, easedTime);
            SetDoorState(openProgress);

            yield return null;
        }

        openProgress = targetProgress;
        SetDoorState(targetProgress);
    }

    private void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
}
