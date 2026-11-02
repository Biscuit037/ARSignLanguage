using UnityEngine;
using UnityEngine.Rendering;

public struct FingerState
{
    public bool Thumb, Index, Middle, Ring, Pinky;
    public override string ToString() => $"{(Thumb ? 1 : 0)}{(Index ? 1 : 0)}{(Middle ? 1 : 0)}{(Ring ? 1 : 0)}{(Pinky ? 1 : 0)}";
}

public class SignRules 
{
    public static float FingerRatio = 1.15f;
    public static float ThumbDistance = 1.0f;
    public static FingerState GetFingerState(Vector3[] p)
    {
        return new FingerState
        {
            Thumb = Vector3.Distance(p[4], p[17]) > ThumbDistance,
            Index = IsExtended(p, 8, 6),
            Middle = IsExtended(p, 12, 10),
            Ring = IsExtended(p, 16, 14),
            Pinky = IsExtended(p, 20, 18),
        };
    }

    static bool IsExtended(Vector3[] p, int tip, int pip) => p[tip].magnitude > p[pip].magnitude * FingerRatio;

    public static string Classify(FingerState f)
    {
        switch (f.ToString())
        {
            case "01000": return "Forward";
            case "01100": return "Left";
            case "01110": return "Right";
            case "11111": return "Start";
            case "00000": return "Rest";
                   
            default: return null;
        }
    }
}
