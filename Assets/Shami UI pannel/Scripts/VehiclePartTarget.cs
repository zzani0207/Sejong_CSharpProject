using UnityEngine;

public enum VehicleArea
{
    Exterior,
    Interior
}

public class VehiclePartTarget : MonoBehaviour
{
    [SerializeField] private VehicleArea area;
    [SerializeField] private Parts part;

    public VehicleArea Area => area;
    public Parts Part => part;
}