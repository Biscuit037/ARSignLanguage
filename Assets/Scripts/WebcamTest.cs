using UnityEngine;
using UnityEngine.UI;

public class WebcamTest : MonoBehaviour
{
    public RawImage target;
    WebCamTexture cam;

    private void Start()
    {
        foreach(var d in WebCamTexture.devices)
        {
            Debug.Log($"Camera: {d.name} (front = {d.isFrontFacing})");
        }

        if(WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("Not found camera");
        }

        cam = new WebCamTexture(WebCamTexture.devices[0].name, 1280, 720, 30);
        target.texture = cam;
        cam.Play();
    }

    private void Update()
    {
        if (cam != null && cam.didUpdateThisFrame && Time.frameCount % 60 == 0)
        {
            Debug.Log($"{cam.deviceName}: {cam.width}x{cam.height}");
        }
    }

    private void OnDestroy() => cam?.Stop();
}
