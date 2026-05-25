using UnityEngine;
using System;

/// <summary>
/// 런타임 차량 상태 싱글톤. 현재 선택된 차를 보관하고 변경 시 OnCarChanged 이벤트 발화.
/// </summary>
public class CarDataManager : MonoBehaviour
{
    [SerializeField] private CarDataBase carDatabase;

    private CarData currentCar;
    public CarData CurrentCar => currentCar;

    public event Action<CarData> OnCarChanged;

    private static CarDataManager instance;

    public static CarDataManager Instance
    {
        get
        {
#if UNITY_EDITOR
            // 에디트 모드에서는 Awake가 안 돈 상태라 씬에서 직접 찾는다
            if (instance == null && !Application.isPlaying)
                instance = FindAnyObjectByType<CarDataManager>();
#endif
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (carDatabase == null)
        {
            carDatabase = Resources.Load<CarDataBase>("CarDatabase");
            if (carDatabase == null)
            {
                Debug.LogError("CarDatabase not found! Please assign it in Inspector or place in Resources folder.");
                return;
            }
        }

        SelectCar(0);
    }

    public void SelectCar(int index)
    {
        CarData newCar = carDatabase.GetCar(index);
        if (newCar != null)
        {
            currentCar = newCar;
            OnCarChanged?.Invoke(currentCar);
        }
    }
}
