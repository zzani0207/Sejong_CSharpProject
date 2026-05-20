using UnityEngine;

public class DoorInputController : MonoBehaviour
{
    [SerializeField] private CarDoorManager doorManager;

    [Header("키 설정")]
    [SerializeField] private KeyCode openAllKey = KeyCode.O;
    [SerializeField] private KeyCode closeAllKey = KeyCode.C;
    [SerializeField] private KeyCode leftDoorKey = KeyCode.Q;
    [SerializeField] private KeyCode rightDoorKey = KeyCode.E;
    [SerializeField] private KeyCode bonnetKey = KeyCode.B;
    [SerializeField] private KeyCode trunkKey = KeyCode.T;
    [SerializeField] private KeyCode headlightsKey = KeyCode.H;
    [SerializeField] private KeyCode rearlightsKey = KeyCode.X;
    [SerializeField] private KeyCode statusKey = KeyCode.P;
    private void Start()
    {
        if (doorManager == null) doorManager = GetComponent<CarDoorManager>();
    }

    private void Update()
    {
        if (doorManager == null) return;

        if (Input.GetKeyDown(openAllKey)) doorManager.OpenAllDoors();
        if (Input.GetKeyDown(closeAllKey)) doorManager.CloseAllDoors();
        if (Input.GetKeyDown(leftDoorKey)) doorManager.ToggleLeftDoor();
        if (Input.GetKeyDown(rightDoorKey)) doorManager.ToggleRightDoor();
        if (Input.GetKeyDown(bonnetKey)) doorManager.ToggleBonnet();
        if (Input.GetKeyDown(trunkKey)) doorManager.ToggleTrunk();
        if (Input.GetKeyDown(headlightsKey)) doorManager.ToggleHeadlights();
        if (Input.GetKeyDown(rearlightsKey)) doorManager.ToggleRearlights();
        if (Input.GetKeyDown(statusKey)) doorManager.PrintDoorStatus();
    }
}