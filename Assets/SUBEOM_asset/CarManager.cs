using UnityEngine;
using System.Collections.Generic;

public class CarDoorManager : MonoBehaviour
{
    [SerializeField] private GameObject frontLeftDoorObject;
    [SerializeField] private GameObject frontRightDoorObject;

    [SerializeField] private GameObject bonnetObject;
    [SerializeField] private GameObject trunkObject;
    [SerializeField] private GameObject[] headlightObjects; // 전방등
    [SerializeField] private GameObject[] rearlightObjects;  // 후미등

    private IDoor frontLeftDoor;
    private IDoor frontRightDoor;
    private IDoor bonnet;
    private IDoor trunk;

    private List<IDoor> allDoors = new List<IDoor>();
    private bool isHeadlightsOn = false; //
    private bool isRearlightsOn = false; //

    private void Start()
    {
        if (frontLeftDoorObject != null) frontLeftDoor = frontLeftDoorObject.GetComponent<IDoor>();
        if (frontRightDoorObject != null) frontRightDoor = frontRightDoorObject.GetComponent<IDoor>();

        if (bonnetObject != null) bonnet = bonnetObject.GetComponent<IDoor>();
        if (trunkObject != null) trunk = trunkObject.GetComponent<IDoor>();

        if (frontLeftDoor != null) allDoors.Add(frontLeftDoor);
        if (frontRightDoor != null) allDoors.Add(frontRightDoor);

       
        SetHeadlights(false);
        SetRearlights(false); 
    }

    public void ToggleLeftDoor() => frontLeftDoor?.Toggle();
    public void ToggleRightDoor() => frontRightDoor?.Toggle();
    public void ToggleBonnet() => bonnet?.Toggle();
    public void ToggleTrunk() => trunk?.Toggle();

    public void ToggleHeadlights()
    {
        isHeadlightsOn = !isHeadlightsOn;
        SetHeadlights(isHeadlightsOn);
        Debug.Log($"해드라이트: {(isHeadlightsOn ? "켜짐" : "꺼짐")}");
    }

    private void SetHeadlights(bool state)
    {
        if (headlightObjects == null) return;

        foreach (var lightObj in headlightObjects)
        {
            if (lightObj == null) continue;
            Light lightComponent = lightObj.GetComponentInChildren<Light>();

            if (lightComponent != null)
            {
                lightComponent.enabled = state;
                if (state)
                {
                    lightComponent.intensity = 15000f;
                    lightComponent.range = 40f;
                }
            }
            else
            {
                lightObj.SetActive(state);
            }
        }
    }

    public void ToggleRearlights()
    {
        
        isRearlightsOn = !isRearlightsOn;
        SetRearlights(isRearlightsOn);
        Debug.Log($"후미등: {(isRearlightsOn ? "켜짐" : "꺼짐")}");
    }

    private void SetRearlights(bool state)
    {
        if (rearlightObjects == null) return;

        foreach (var lightObj in rearlightObjects)
        {
            if (lightObj == null) continue;
            Light lightComponent = lightObj.GetComponentInChildren<Light>();

            if (lightComponent != null)
            {
                lightComponent.enabled = state;
                if (state)
                {
                  
                    lightComponent.intensity = 15000f;
                    lightComponent.range = 40f;
                }
            }
            else
            {
                lightObj.SetActive(state);
            }
        }
    }

    public void OpenAllDoors() { foreach (var door in allDoors) door?.Open(); }
    public void CloseAllDoors() { foreach (var door in allDoors) door?.Close(); }

    public void PrintDoorStatus()
    {
        string status = "====== 차량 기능 실시간 상태 ======\n";
        status += $"앞 왼쪽 문: {(frontLeftDoor?.IsOpen == true ? "열림" : "닫힘")}\n";
        status += $"앞 오른쪽 문: {(frontRightDoor?.IsOpen == true ? "열림" : "닫힘")}\n";
        status += $"보넷 상태: {(bonnet?.IsOpen == true ? "열림" : "닫힘")}\n";
        status += $"트렁크 상태: {(trunk?.IsOpen == true ? "열림" : "닫힘")}\n";
        status += $"해드라이트: {(isHeadlightsOn ? "★ ON ★" : "☆ OFF ☆")}\n"; // 개행 문자(\n) 누락 수정
        status += $"후미등: {(isRearlightsOn ? "★ ON ★" : "☆ OFF ☆")}";
        Debug.Log(status);
    }
}