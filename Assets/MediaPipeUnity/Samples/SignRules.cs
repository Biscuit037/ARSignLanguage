using UnityEngine;
using UnityEngine.Rendering;

public struct FingerState
{
    public bool Thumb, Index, Middle, Ring, Pinky;
    public override string ToString() => $"{(Thumb ? 1 : 0)}{(Index ? 1 : 0)}{(Middle ? 1 : 0)}{(Ring ? 1 : 0)}{(Pinky ? 1 : 0)}";
}

public class SignRules
{
    // A finger is extended when its tip is this much farther from the wrist than its PIP joint.
    public static float FingerRatio = 1.15f;
    // Thumb is extended when its tip is farther than this from the pinky base (normalized units).
    public static float ThumbDistance = 1.25f;

    // Expects points from HandNormalizer.Normalize (wrist at origin, palm length = 1).
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

    // Returns the command name, "Rest" for a fist, or null if no rule matches.
    public static string Classify(FingerState f)
    {
        bool fourCurled = !f.Index && !f.Middle && !f.Ring && !f.Pinky;

        if (fourCurled)
            return f.Thumb ? "Forward" : "Rest";                      

        if (!f.Index && f.Middle && f.Ring && f.Pinky) return "Left";  
        if (f.Index && f.Middle && !f.Ring && !f.Pinky) return "Right"; 
        if (f.Index && f.Middle && f.Ring && f.Pinky) return "Start"; 

        return null;
    }
}
