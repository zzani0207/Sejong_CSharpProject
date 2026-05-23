using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PartPanelBinding
{
    public Parts part;
    public GameObject panelRoot;
    public UIAnimationMaster animator;
    public Button closeButton;
}

public class PanelManager : MonoBehaviour
{
    [SerializeField] private PartPanelBinding[] panels;

    private PartPanelBinding currentBinding;

    public event Action OnCloseRequested;

    public bool HasOpenPanel => currentBinding != null;

    public bool HasPanel(Parts part)
    {
        return FindBinding(part) != null;
    }

    private void Awake()
    {
        InitializePanels();
        InitializeCloseButtons();
    }

    private void InitializePanels()
    {
        foreach (PartPanelBinding binding in panels)
        {
            if (binding == null || binding.panelRoot == null)
                continue;

            if (binding.animator == null)
                binding.animator = binding.panelRoot.GetComponent<UIAnimationMaster>();

            if (binding.closeButton == null)
                binding.closeButton = binding.panelRoot.GetComponentInChildren<Button>(true);

            CanvasGroup canvasGroup = binding.panelRoot.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    private void InitializeCloseButtons()
    {
        foreach (PartPanelBinding binding in panels)
        {
            if (binding == null || binding.closeButton == null)
                continue;

            binding.closeButton.onClick.RemoveAllListeners();
            binding.closeButton.onClick.AddListener(RequestClose);
        }
    }

    public void OpenPanel(Parts part)
    {
        PartPanelBinding target = FindBinding(part);

        if (target == null)
        {
            Debug.LogWarning($"Panel binding is not assigned for {part}");
            return;
        }

        if (currentBinding != null && currentBinding != target)
            PlayOut(currentBinding);

        currentBinding = target;
        PlayIn(currentBinding);
    }

    public void RequestClose()
    {
        if (OnCloseRequested != null)
            OnCloseRequested.Invoke();
        else
            CloseCurrentPanel();
    }

    public void CloseCurrentPanel(Action onComplete = null)
    {
        if (currentBinding == null)
        {
            onComplete?.Invoke();
            return;
        }

        PartPanelBinding target = currentBinding;
        currentBinding = null;

        PlayOut(target, onComplete);
    }

    private PartPanelBinding FindBinding(Parts part)
    {
        foreach (PartPanelBinding binding in panels)
        {
            if (binding != null && binding.part == part)
                return binding;
        }

        return null;
    }

    private void PlayIn(PartPanelBinding binding)
    {
        if (binding == null || binding.panelRoot == null)
            return;

        binding.panelRoot.SetActive(true);

        CanvasGroup canvasGroup = binding.panelRoot.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (binding.animator != null)
        {
            binding.animator.PlayIn();
        }
        else
        {
            // UIAnimationMaster가 없을 때 fallback
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }
    }

    private void PlayOut(PartPanelBinding binding, Action onComplete = null)
    {
        if (binding == null || binding.panelRoot == null)
        {
            onComplete?.Invoke();
            return;
        }

        CanvasGroup canvasGroup = binding.panelRoot.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (binding.animator != null)
        {
            binding.animator.PlayOut(onComplete);
        }
        else
        {
            // UIAnimationMaster가 없을 때 fallback
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            onComplete?.Invoke();
        }
    }
}