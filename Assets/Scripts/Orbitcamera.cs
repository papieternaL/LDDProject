using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Distance / Zoom")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 5f;
    public float zoomSmoothTime = 0.1f;

    [Header("Orbit Controls")]
    public float rotationSpeed = 200f;
    public float minPitch = -30f;
    public float maxPitch = 70f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.05f;
    public float rotationSmoothTime = 0.05f;   // ⭐ NEW (missing before)

    float yaw;
    float pitch;

    float targetYaw;
    float targetPitch;

    float desiredDistance;
    float currentDistance;

    Vector3 currentVelocity;
    float yawVelocity;
    float pitchVelocity;

    bool isRotating;
    bool justLocked; // ⭐ prevents choppy movement

    void Start()
    {
        if (!target)
        {
            Debug.LogWarning("[OrbitCamera] No target assigned.");
            enabled = false;
            return;
        }

        Vector3 euler = transform.rotation.eulerAngles;
        yaw = targetYaw = euler.y;
        pitch = targetPitch = euler.x;

        desiredDistance = distance;
        currentDistance = distance;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (!target) return;

        HandleInput();
        UpdateZoom();
        UpdateOrbit();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isRotating = true;
            justLocked = true;  // ⭐ skip first frame delta
        }

        if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isRotating = false;
        }

        if (!isRotating) return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        if (justLocked)
        {
            justLocked = false; // ignore first frame
            return;
        }

        targetYaw += mouseX * rotationSpeed * Time.deltaTime;
        targetPitch -= mouseY * rotationSpeed * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }

    void UpdateZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            desiredDistance -= scroll * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }

        currentDistance = Mathf.Lerp(
            currentDistance,
            desiredDistance,
            1f - Mathf.Exp(-zoomSmoothTime * Time.deltaTime * 60f)
        );
    }

    void UpdateOrbit()
    {
        // ⭐ Smooth yaw/pitch
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPos = target.position + targetOffset;
        Vector3 desiredPos = targetPos - rot * Vector3.forward * currentDistance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref currentVelocity,
            positionSmoothTime
        );

        transform.rotation = rot;
    }
}
