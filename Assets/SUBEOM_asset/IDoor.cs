using UnityEngine;

public interface IDoor
{
    bool IsOpen { get; }
    float OpenProgress { get; }

    void Open();
    void Close();
    void Toggle();
    void SetDoorState(float progress);
}
