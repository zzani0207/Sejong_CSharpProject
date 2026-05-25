using UnityEngine;

[CreateAssetMenu(fileName = "CarDatabase", menuName = "Scriptable Objects/Car Database")]
public class CarDataBase : ScriptableObject
{
    [SerializeField] private CarData[] cars;

    public CarData GetCar(int index)
    {
        if (cars != null && index >= 0 && index < cars.Length)
            return cars[index];
        Debug.LogWarning($"Invalid car index: {index}");
        return null;
    }
}
