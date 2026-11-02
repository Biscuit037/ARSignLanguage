using UnityEngine;

public class HandDebug : MonoBehaviour
{
    private void OnEnable() => HandInput.OnHand += Log;
    private void OnDisable() => HandInput.OnHand -= Log;

    void Log(HandFrame f) => Debug.Log($"{(f.IsLeft ? "L" : "R")} wrist={f.Landmarks[0]} indexTip={f.Landmarks[8]}");
}
