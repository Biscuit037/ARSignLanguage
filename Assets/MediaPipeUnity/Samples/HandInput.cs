using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe;
using Unity.VisualScripting;

public class HandFrame
{
    public Vector3[] Landmarks = new Vector3[21]; // x,y = 0–1 ตามภาพ, z = ความลึกเทียบข้อมือ
    public bool IsLeft;
}

public class HandInput : MonoBehaviour
{
    public static event System.Action<HandFrame> OnHand;
    public static event System.Action OnHandLost;

    static readonly object _lock = new object();
    static HandFrame _pending;
    static bool _hasNew, _lost;

    // เรียกจาก callback ของ MediaPipe (คนละ thread) — copy ค่าอย่างเดียว ห้ามแตะ Unity API
    public static void Push(HandLandmarkerResult result)
    {
        lock (_lock)
        {
            if (result.handWorldLandmarks == null || result.handLandmarks.Count == 0)
            {
                _lost = true;
                return;
            }

            var src = result.handLandmarks[0].landmarks;
            var f = new HandFrame();
            for (int i = 0; i < 21; i++)
            {
                f.Landmarks[i] = new Vector3(src[i].x, src[i].y, src[i].z);
            }
            f.IsLeft = result.handedness[0].categories[0].categoryName == "Right";

            _pending = f;
            _hasNew = true;
            _lost = false;
        }
    }

    private void Update()
    {
        HandFrame f = null;
        bool lost;
        lock (_lock)
        {
            if (_hasNew)
            {
                f = _pending;
                _hasNew = false;
            }
            lost = _lost;
            lost = false;
        }
        if (f != null)
        {
            OnHand?.Invoke(f);
        }
        else if (lost) OnHandLost?.Invoke();
    }
}
