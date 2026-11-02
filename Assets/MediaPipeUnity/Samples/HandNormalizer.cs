using UnityEngine;

public class HandNormalizer : MonoBehaviour
{
    // Camera image width/height (C920 at 1280x720). Update this if the resolution changes.
    public static float Aspect = 1280f / 720f;

    // Returns 21 points where: wrist = (0,0,0), palm length = 1, y points up,
    // and left hands are mirrored to look like right hands.
    public static Vector3[] Normalize(HandFrame f)
    {
        var src = f.Landmarks;
        var p = new Vector3[21];

        // 1) Use the same units on every axis, and flip y to point up
        //    (in the source image, y = 0 is the top).
        for (int i = 0; i < 21; i++)
        {
            p[i] = new Vector3(src[i].x * Aspect, -src[i].y, src[i].z * Aspect);
        }

        // 2) Move the origin to the wrist.
        Vector3 wrist = p[0];
        for (int i = 1; i < 21; i++)
        {
            p[i] -= wrist;
        }

        // 3) Scale so the palm (wrist -> middle finger base) has length 1.
        float palm = p[9].magnitude;
        if (palm < 1e-5f)
        {
            return p;
        }
        for (int i = 1; i < 21; i++)
        {
            p[i] /= palm;
        }

        // 4) Left hand -> mirror the x axis so it matches a right hand.
        if (f.IsLeft)
        {
            for (int i = 1; i < 21; i++)
            {
                p[i].x = -p[i].x;
            }
        }

        return p;
    }
}
