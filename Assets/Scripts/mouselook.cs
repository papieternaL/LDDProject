using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 200f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    float pitch;
    bool isLooking = false;

    void Start()
    {
        // Start the game WITHOUT locking the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Initialize pitch to match camera
        pitch = transform.localEulerAngles.x;
    }

    void Update()
    {
        // Press RMB: lock cursor + start looking around
        if (Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isLooking = true;
        }

        // Release RMB: unlock cursor + stop looking
        if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isLooking = false;
        }

        // Do NOT move the camera unless RMB is held
        if (!isLooking)
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotate player horizontally
        playerBody.Rotate(Vector3.up * mouseX * sensitivity * Time.deltaTime);

        // Rotate camera vertically
        pitch -= mouseY * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
