using UnityEngine;

public class HandDebug : MonoBehaviour
{
    private void OnEnable() => HandInput.OnHand += Log;
    private void OnDisable() => HandInput.OnHand -= Log;

    void Log(HandFrame f)
    {
        var n = HandNormalizer.Normalize(f);
        var fingers = SignRules.GetFingerState(n);
        float idxRatio = n[9].magnitude / n[6].magnitude;
        float thumbDist = Vector3.Distance(n[4], n[17]);
        Debug.Log($"{fingers} {SignRules.Classify(fingers) ?? "-"} idxRatio={idxRatio:F2}  thumb={thumbDist:F2}");
    }
}
