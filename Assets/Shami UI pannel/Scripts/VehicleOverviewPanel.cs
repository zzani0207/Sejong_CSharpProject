using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum VehicleConnectionStatus
{
    Connected,
    Disconnected,
    Syncing,
    Error
}

public enum VehicleSystemStatus
{
    Normal,
    Warning,
    Critical,
    Offline
}

public enum VehicleWarningType
{
    None,
    TirePressureLow,
    BatteryLow,
    DoorOpen,
    LightLeftOn,
    SensorOffline
}

[Serializable]
public class VehicleOverviewData
{
    public string vehicleName;
    public string vehicleLocation;
    public VehicleConnectionStatus connectionStatus;
    public VehicleSystemStatus systemStatus;
    public VehicleWarningType warningType;
    public string lastUpdateTime;
}

public class VehicleOverviewPanel : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text vehicleNameText;
    [SerializeField] private TMP_Text vehicleLocationText;
    [SerializeField] private TMP_Text connectionStatusText;
    [SerializeField] private TMP_Text systemStatusText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private TMP_Text lastUpdateTimeText;

    [Header("Buttons")]
    [SerializeField] private Button interiorViewButton;
    [SerializeField] private TMP_Text interiorViewButtonText;
    [SerializeField] private Button viewLogsButton;

    [Header("Button Labels")]
    [SerializeField] private string enterInteriorLabel = "Interior View";
    [SerializeField] private string exitInteriorLabel = "Exterior View";

    [Header("External References")]
    [SerializeField] private VehicleInteractionController vehicleInteractionController;
    [SerializeField] private GameObject maintenanceLogPanel;

    [Header("Debug / Dummy Data")]
    [SerializeField] private bool useDummyDataOnStart = true;

    private void Awake()
    {
        if (interiorViewButtonText == null && interiorViewButton != null)
            interiorViewButtonText = interiorViewButton.GetComponentInChildren<TMP_Text>(true);

        if (interiorViewButton != null)
        {
            interiorViewButton.onClick.RemoveAllListeners();
            interiorViewButton.onClick.AddListener(OnClickInteriorToggle);
        }

        if (viewLogsButton != null)
        {
            viewLogsButton.onClick.RemoveAllListeners();
            viewLogsButton.onClick.AddListener(OnClickViewLogs);
        }

        if (maintenanceLogPanel != null)
            maintenanceLogPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (vehicleInteractionController != null)
            vehicleInteractionController.OnAreaChanged += UpdateInteriorButtonLabel;
    }

    private void OnDisable()
    {
        if (vehicleInteractionController != null)
            vehicleInteractionController.OnAreaChanged -= UpdateInteriorButtonLabel;
    }

    private void Start()
    {
        if (useDummyDataOnStart)
        {
            VehicleOverviewData dummyData = new VehicleOverviewData
            {
                vehicleName = "GBX Coupe",
                vehicleLocation = "Garage A",
                connectionStatus = VehicleConnectionStatus.Connected,
                systemStatus = VehicleSystemStatus.Normal,
                warningType = VehicleWarningType.None,
                lastUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            SetOverviewData(dummyData);
        }

        if (vehicleInteractionController != null)
            UpdateInteriorButtonLabel(vehicleInteractionController.CurrentArea);
        else
            SetInteriorButtonText(enterInteriorLabel);
    }

    public void SetOverviewData(VehicleOverviewData data)
    {
        if (data == null)
            return;

        SetText(vehicleNameText, data.vehicleName);
        SetText(vehicleLocationText, data.vehicleLocation);
        SetText(connectionStatusText, FormatConnectionStatus(data.connectionStatus));
        SetText(systemStatusText, FormatSystemStatus(data.systemStatus));
        SetText(warningText, FormatWarningType(data.warningType));
        SetText(lastUpdateTimeText, data.lastUpdateTime);
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText != null)
            targetText.text = value;
    }

    private void OnClickInteriorToggle()
    {
        if (vehicleInteractionController == null)
        {
            Debug.LogWarning("VehicleInteractionController is not assigned.");
            return;
        }

        if (vehicleInteractionController.IsInteriorMode)
        {
            vehicleInteractionController.ExitInteriorMode();
        }
        else
        {
            vehicleInteractionController.EnterInteriorMode();
        }
    }

    private void UpdateInteriorButtonLabel(VehicleArea area)
    {
        if (area == VehicleArea.Interior)
            SetInteriorButtonText(exitInteriorLabel);
        else
            SetInteriorButtonText(enterInteriorLabel);
    }

    private void SetInteriorButtonText(string text)
    {
        if (interiorViewButtonText != null)
            interiorViewButtonText.text = text;
    }

    private void OnClickViewLogs()
    {
        if (maintenanceLogPanel != null)
        {
            maintenanceLogPanel.SetActive(true);
        }
        else
        {
            Debug.Log("View Logs clicked. Maintenance Log UI is not implemented yet.");
        }
    }

    private string FormatConnectionStatus(VehicleConnectionStatus status)
    {
        switch (status)
        {
            case VehicleConnectionStatus.Connected:
                return "Connected";
            case VehicleConnectionStatus.Disconnected:
                return "Disconnected";
            case VehicleConnectionStatus.Syncing:
                return "Syncing";
            case VehicleConnectionStatus.Error:
                return "Error";
            default:
                return "Unknown";
        }
    }

    private string FormatSystemStatus(VehicleSystemStatus status)
    {
        switch (status)
        {
            case VehicleSystemStatus.Normal:
                return "Normal";
            case VehicleSystemStatus.Warning:
                return "Warning";
            case VehicleSystemStatus.Critical:
                return "Critical";
            case VehicleSystemStatus.Offline:
                return "Offline";
            default:
                return "Unknown";
        }
    }

    private string FormatWarningType(VehicleWarningType warning)
    {
        switch (warning)
        {
            case VehicleWarningType.None:
                return "None";
            case VehicleWarningType.TirePressureLow:
                return "Tire Pressure Low";
            case VehicleWarningType.BatteryLow:
                return "Battery Low";
            case VehicleWarningType.DoorOpen:
                return "Door Open";
            case VehicleWarningType.LightLeftOn:
                return "Light Left On";
            case VehicleWarningType.SensorOffline:
                return "Sensor Offline";
            default:
                return "Unknown";
        }
    }
}