using UnityEngine;

/// <summary>
/// CarManager 사용 예제 및 테스트 스크립트
/// 씬에 추가하면 각종 기능을 테스트할 수 있습니다
/// </summary>
public class CarManagerExample : MonoBehaviour
{
    private void Update()
    {
        // 키보드 입력으로 차 전환
        if (Input.GetKeyDown(KeyCode.E))
        {
            CarManager.Instance.SelectNextCar();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CarManager.Instance.SelectPreviousCar();
        }

        // 배터리 충전/방전 테스트 (전기/하이브리드/PHEV만)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CarManager.Instance.ChargeBattery(10);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            CarManager.Instance.DischargeBattery(5);
        }

        // 연료 충전/사용 테스트 (가솔린/디젤/하이브리드/PHEV)
        if (Input.GetKeyDown(KeyCode.F))
        {
            CarManager.Instance.RefuelCar(10);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            CarManager.Instance.UseFuel(2);
        }

        // 주행거리 추가 테스트
        if (Input.GetKeyDown(KeyCode.D))
        {
            CarManager.Instance.AddMileage(10);
        }

        // 현재 차 정보 출력
        if (Input.GetKeyDown(KeyCode.P))
        {
            CarManager.Instance.PrintCurrentCarInfo();
        }

        // 모든 차 정보 출력
        if (Input.GetKeyDown(KeyCode.A))
        {
            CarManager.Instance.PrintAllCarsInfo();
        }
    }

    /// <summary>
    /// 특정 차 선택 테스트
    /// </summary>
    public void TestSelectCarByIndex(int index)
    {
        CarManager.Instance.SelectCar(index);
    }

    /// <summary>
    /// 배터리 충전 테스트
    /// </summary>
    public void TestChargeFullBattery()
    {
        CarManager.Instance.ChargeBattery(100);
    }

    /// <summary>
    /// 배터리 완전 방전 테스트
    /// </summary>
    public void TestDischargeBattery()
    {
        CarManager.Instance.DischargeBattery(100);
    }

    /// <summary>
    /// 주행 시뮬레이션 테스트
    /// </summary>
    public void TestDriveSimulation(float distance)
    {
        CarManager.Instance.AddMileage(distance);
        CarManager.Instance.DischargeBattery(distance * 0.15f); // 주행거리에 따라 배터리 소모
    }

    /// <summary>
    /// 온스크린 정보 표시 (Optional - UI 텍스트가 없을 때)
    /// </summary>
    private void OnGUI()
    {
        if (CarManager.Instance == null || CarManager.Instance.CurrentCar == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 500, 700));
        GUILayout.Box("연결된 차 정보");

        var car = CarManager.Instance.CurrentCar;
        
        GUILayout.Label($"<b>차량: {car.CarName}</b>");
        GUILayout.Label($"제조사: {car.Manufacturer}");
        GUILayout.Label($"연료타입: {car.FuelTypeString}");
        GUILayout.Label($"최대속도: {car.MaxSpeed}km/h");
        
        // 연료/배터리 정보 표시
        if (car.FuelType == FuelType.Electric)
        {
            GUILayout.Label($"배터리: {car.CurrentChargeLevel}% (남은거리: {car.RemainingBatteryRange:F0}km)");
        }
        else if (car.FuelType == FuelType.Gasoline || car.FuelType == FuelType.Diesel)
        {
            GUILayout.Label($"연료: {car.CurrentFuelLevel:F1}/{car.FuelTankCapacity}L (남은거리: {car.RemainingFuelRange:F0}km)");
        }
        else if (car.FuelType == FuelType.Hybrid || car.FuelType == FuelType.PHEV)
        {
            GUILayout.Label($"연료: {car.CurrentFuelLevel:F1}L / 배터리: {car.CurrentChargeLevel}%");
            GUILayout.Label($"총 남은거리: {car.RemainingRange:F0}km");
        }

        GUILayout.Label($"주행거리: {car.Mileage:F0}km");

        GUILayout.Space(10);
        GUILayout.Label("<b>테스트 버튼</b>");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("이전 차 (Q)")) CarManager.Instance.SelectPreviousCar();
        if (GUILayout.Button("다음 차 (E)")) CarManager.Instance.SelectNextCar();
        GUILayout.EndHorizontal();

        // 배터리 컨트롤 (전기/하이브리드/PHEV)
        if (car.FuelType == FuelType.Electric || car.FuelType == FuelType.Hybrid || car.FuelType == FuelType.PHEV)
        {
            GUILayout.Label("<b>배터리 컨트롤</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("충전 +10% (Space)")) CarManager.Instance.ChargeBattery(10);
            if (GUILayout.Button("방전 -5% (R)")) CarManager.Instance.DischargeBattery(5);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("풀 충전")) CarManager.Instance.ChargeBattery(100);
        }

        // 연료 컨트롤 (가솔린/디젤/하이브리드/PHEV)
        if (car.FuelType == FuelType.Gasoline || car.FuelType == FuelType.Diesel || 
            car.FuelType == FuelType.Hybrid || car.FuelType == FuelType.PHEV)
        {
            GUILayout.Label("<b>연료 컨트롤</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("충전 +10L (F)")) CarManager.Instance.RefuelCar(10);
            if (GUILayout.Button("사용 -2L (U)")) CarManager.Instance.UseFuel(2);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("풀 충전")) CarManager.Instance.RefuelCar(100);
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>주행 시뮬레이션</b>");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("10km 주행 (D)")) CarManager.Instance.AddMileage(10);
        if (GUILayout.Button("100km 주행")) CarManager.Instance.AddMileage(100);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("정보 출력 (P)")) CarManager.Instance.PrintCurrentCarInfo();

        GUILayout.EndArea();
    }
}
