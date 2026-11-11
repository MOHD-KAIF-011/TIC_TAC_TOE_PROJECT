using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple orbit camera: right-drag rotates around the board and the scroll
/// wheel zooms. The camera position is calculated with spherical coordinates.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    public Vector3 pivot = new Vector3(0f, -0.05f, 0f);
    public float distance = 8f;
    public float yaw = 15f;
    public float pitch = 44f;

    public float minPitch = 20f;
    public float maxPitch = 82f;
    public float minDistance = 5f;
    public float maxDistance = 14f;

    public float rotateSpeed = 0.25f;
    public float zoomSpeed = 0.01f;

    private float startDistance;
    private float startYaw;
    private float startPitch;

    private void Start()
    {
        startDistance = distance;
        startYaw = yaw;
        startPitch = pitch;
        ApplyTransform();
    }

    private void LateUpdate()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * rotateSpeed;
            pitch = Mathf.Clamp(pitch - delta.y * rotateSpeed, minPitch, maxPitch);
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            distance = startDistance;
            yaw = startYaw;
            pitch = startPitch;
        }

        ApplyTransform();
    }

    private void ApplyTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = pivot + rotation * Vector3.back * distance;
        transform.rotation = Quaternion.LookRotation(pivot - transform.position);
    }
}
