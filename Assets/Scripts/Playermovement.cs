using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Slide Settings")]
    public float slideAcceleration = 15f;      // How fast you speed up down slopes
    public float maxSlideSpeed = 12f;         // Cap slide speed
    public float slopeLimit = 45f;            // Match controller.slopeLimit or override it
    public LayerMask groundMask = ~0;         // Layers considered "ground"

    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 slideVelocity;            // stored slide motion across frames

    void Update()
    {
        // --- Ground check ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // --- Movement (WASD) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        Vector3 move = (camForward * v + camRight * h);
        if (move.sqrMagnitude > 1f) move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime);

        // --- Jump ---
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- Gravity ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- SHIFT TO SLIDE ---
        HandleSlide();
    }

    void HandleSlide()
    {
        // Only slide while Shift is held
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            slideVelocity = Vector3.zero;
            return;
        }

        // Sphere cast under the player to read slope
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        if (Physics.SphereCast(origin, 0.3f, Vector3.down, out RaycastHit hit, 1f, groundMask))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            // Only slide on slopes beyond the slopeLimit
            if (angle > slopeLimit)
            {
                // Compute direction of steepest descent
                Vector3 downslope = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

                // Accelerate slide
                slideVelocity += downslope * slideAcceleration * Time.deltaTime;

                // Clamp to max speed
                if (slideVelocity.magnitude > maxSlideSpeed)
                    slideVelocity = slideVelocity.normalized * maxSlideSpeed;

                // Apply slide motion
                controller.Move(slideVelocity * Time.deltaTime);
            }
            else
            {
                // Flatten slide when no longer on slope
                slideVelocity = Vector3.zero;
            }
        }
        else
        {
            slideVelocity = Vector3.zero;
        }
    }
}
