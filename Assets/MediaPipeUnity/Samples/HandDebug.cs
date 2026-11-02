using UnityEngine;

public class HandDebug : MonoBehaviour
{
    private void OnEnable() => HandInput.OnHand += Log;
    private void OnDisable() => HandInput.OnHand -= Log;

    void Log(HandFrame f)
    {
        var n = HandNormalizer.Normalize(f);
        Debug.Log($"{(f.IsLeft ? "L" : "R")}  raw={f.Landmarks[8]:F2} norm={n[8]:F2} len={n[8].magnitude:F2}");
    }
}
