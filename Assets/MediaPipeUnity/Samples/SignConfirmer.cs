using System;
using UnityEngine;
using UnityEngine.UI;

public class SignConfirmer : MonoBehaviour
{
    [Header("Timing (second)")]
    public float holdTime = 0.6f;           // How long a sign must be held to confirm it.
    public float graceTime = 0.15f;         // Brief misreads shorter than this don't reset the hold.
    public float handLostTimeout = 0.3f;    // No hand for this long counts as "hand removed".

    [Header("Optional UI")]
    public Image progressRing;              // Image with Image Type = Filled.

    public static event Action<string> OnSignConfirm;

    public string Candidate { get; private set; }
    public float Progress { get; private set; }  // 0..1

    float holdStart, lastSeen, lastHandTime;
    string lastConfirmed; // Blocked from repeating until Rest or hand removed.

    private void OnEnable() => HandInput.OnHand += OnHand;
    private void OnDisable() => HandInput.OnHand -= OnHand;

    public float confirmFlashTime = 0.3f;
    public Color holdColor = Color.white;
    public Color confirmColor = Color.green;

    float confirmedAt = -10f;

    void OnHand(HandFrame f)
    {
        float now = Time.time;
        lastHandTime = now;

        string sign = SignRules.Classify(SignRules.GetFingerState(HandNormalizer.Normalize(f)));

        if (sign == "Rest")
        {
            lastConfirmed = null;
            ResetHold();
            return;
        }

        if (sign != null && sign == Candidate)
        {
            lastSeen = now;
        }
        else if (sign != null)
        {
            Candidate = sign;
            holdStart = now;
            lastSeen = now;
        }
        else if (now - lastSeen > graceTime)
        {
            Candidate = null;                       // Only misreads (null) get the grace period.
        }

        if(Candidate == null || Candidate == lastConfirmed)
        {
            Progress = 0;
            return;
        }

        Progress = Mathf.Clamp01((now - holdStart) / holdTime);
        if(Progress >= 1f)
        {
            lastConfirmed = Candidate;
            confirmedAt = now;
            OnSignConfirm?.Invoke(Candidate);
            ResetHold();
        }
    }

    void Update()
    {
        // Hand left the camera -> re-arm.
        if (Time.time - lastHandTime > handLostTimeout)
        {
            lastConfirmed = null;
            ResetHold();
        }
        if (progressRing)
        {
            bool flashing = Time.time - confirmedAt < confirmFlashTime;
            progressRing.fillAmount = flashing ? 1f : Progress;
            progressRing.color = flashing ? confirmColor : holdColor;
        }
    }

    void ResetHold()
    {
        Candidate = null;
        Progress = 0f;
    }
}
