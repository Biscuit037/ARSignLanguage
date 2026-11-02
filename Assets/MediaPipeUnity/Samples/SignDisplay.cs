using UnityEngine;
using TMPro;

public class SignDisplay : MonoBehaviour
{
    public TMP_Text label;

    private void OnEnable()
    {
        HandInput.OnHand += OnHand;
        HandInput.OnHandLost += OnHandLost;
    }

    private void OnDisable()
    {
        HandInput.OnHand += OnHand;
        HandInput.OnHandLost += OnHandLost;
    }

    private void OnHand(HandFrame f)
    {
        var points = HandNormalizer.Normalize(f);
        var fingers = SignRules.GetFingerState(points);
        string sign = SignRules.Classify(fingers) ?? "?";
        label.text = $"{sign}\n<size=40%>{fingers}</size>";
    }

    void OnHandLost() => label.text = "No hand";
}
