using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("CarData 연동")]
    [SerializeField] private CarData carData;

    [Header("Debug / Dummy Data")]
    [SerializeField] private bool useDummyDataOnStart = false;

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
        // CarData가 할당되면 그것을 사용, 아니면 더미 데이터 사용
        if (carData != null)
        {
            LoadDataFromCarData();
        }
        else if (useDummyDataOnStart)
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

    /// <summary>
    /// CarData에서 데이터를 로드해서 UI에 표시합니다.
    /// </summary>
    public void LoadDataFromCarData()
    {
        if (carData == null)
        {
            Debug.LogWarning("CarData가 할당되지 않았습니다.");
            return;
        }

        VehicleOverviewData data = new VehicleOverviewData
        {
            vehicleName = carData.CarName,
            vehicleLocation = carData.VehicleLocation,
            connectionStatus = carData.ConnectionStatus,
            systemStatus = carData.SystemStatus,
            warningType = carData.WarningType,
            lastUpdateTime = carData.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
        };

        SetOverviewData(data);
    }

    /// <summary>
    /// CarData 참조를 설정하고 데이터를 로드합니다.
    /// </summary>
    public void SetCarData(CarData newCarData)
    {
        carData = newCarData;
        LoadDataFromCarData();
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