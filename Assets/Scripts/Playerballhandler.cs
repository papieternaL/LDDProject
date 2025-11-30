using UnityEngine;
using UnityEngine.UI;   // for Image / CanvasGroup

public class PlayerBallHandler : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;             // Auto-created if left empty
    public Camera cam;                      // Drag Main Camera or leave empty

    [Header("Pickup Settings")]
    public float pickupRange = 6f;

    [Header("Throw Settings")]
    public float minThrowForce = 8f;        // force on quick tap
    public float maxThrowForce = 22f;       // max force when fully charged
    public float maxChargeTime = 1.5f;      // seconds to reach max force
    public float upwardBoost = 1.5f;        // adds a bit of arc

    [Header("First-Person Comfort")]
    public bool shrinkWhileHeld = true;
    [Range(0.25f, 1.0f)] public float heldScaleFactor = 0.6f;  // 60% size while held

    [Header("Charge UI")]
    public CanvasGroup chargeUI;            // CanvasGroup on the bar root
    public Image chargeFillImage;           // Fill image (Image Type = Filled)
    public float uiFadeSpeed = 10f;

    private Ball heldBall;
    private Ball nearestBall;
    private Vector3 savedOriginalScale;

    // charge state
    private bool isCharging;
    private float currentThrowForce;

    void Awake()
    {
        if (!cam) cam = Camera.main;

        // Create a good FPS hold point if none assigned
        if (!holdPoint)
        {
            var hp = new GameObject("HoldPoint");
            hp.transform.SetParent(cam ? cam.transform : transform, false);
            // FPS-friendly offset: right, slightly down, and forward
            hp.transform.localPosition = new Vector3(0.35f, -0.35f, 1.9f);
            holdPoint = hp.transform;
        }

        if (chargeUI)
            chargeUI.alpha = 0f;
        if (chargeFillImage)
            chargeFillImage.fillAmount = 0f;
    }

    void Update()
    {
        HandlePickupDrop();
        HandleChargeAndThrow();
        UpdateHeldBallPosition();
        nearestBall = FindNearestBall();
        UpdateChargeUI();
    }

    // ---------- Pickup / Drop ----------

    void HandlePickupDrop()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldBall == null) TryPickup();
            else Drop();
        }
    }

    void TryPickup()
    {
        Ball best = FindNearestBall();
        if (!best)
            return;

        float dist = Vector3.Distance(transform.position + Vector3.up, best.transform.position);
        if (dist > pickupRange)
            return;

        Pickup(best);
    }

    void Pickup(Ball b)
    {
        if (!b) return;

        var rb = b.GetComponent<Rigidbody>();
        if (!rb) return;

        // Stop physics and parent to hold point
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Save original scale and optionally shrink for FP readability
        savedOriginalScale = b.transform.localScale;
        if (shrinkWhileHeld)
            b.transform.localScale = savedOriginalScale * heldScaleFactor;

        b.transform.SetParent(holdPoint);
        b.transform.localPosition = Vector3.zero;
        b.transform.localRotation = Quaternion.identity;

        heldBall = b;
    }

    void Drop()
    {
        if (!heldBall) return;

        // Restore scale
        heldBall.transform.localScale = savedOriginalScale;

        var rb = heldBall.GetComponent<Rigidbody>();
        heldBall.transform.SetParent(null);
        if (rb) rb.isKinematic = false;

        heldBall = null;
        isCharging = false;
    }

    // ---------- Charge + Throw ----------

    void HandleChargeAndThrow()
    {
        if (heldBall == null)
        {
            isCharging = false;
            return;
        }

        // start charging
        if (Input.GetButtonDown("Fire1"))
        {
            isCharging = true;
            currentThrowForce = minThrowForce;
        }

        // while holding
        if (isCharging && Input.GetButton("Fire1"))
        {
            float chargeRate = (maxThrowForce - minThrowForce) / Mathf.Max(0.01f, maxChargeTime);
            currentThrowForce += chargeRate * Time.deltaTime;
            currentThrowForce = Mathf.Clamp(currentThrowForce, minThrowForce, maxThrowForce);
        }

        // release to throw
        if (isCharging && Input.GetButtonUp("Fire1"))
        {
            Vector3 dir = cam ? cam.transform.forward : transform.forward;
            Throw(dir, currentThrowForce);
            isCharging = false;
        }
    }

    void Throw(Vector3 direction, float force)
    {
        if (!heldBall) return;

        // Restore scale before throw
        heldBall.transform.localScale = savedOriginalScale;

        var rb = heldBall.GetComponent<Rigidbody>();
        heldBall.transform.SetParent(null);

        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 throwDir = direction.normalized + Vector3.up * upwardBoost * 0.1f;
            rb.AddForce(throwDir.normalized * force, ForceMode.VelocityChange);
        }

        heldBall = null;
    }

    // ---------- Utility ----------

    void UpdateHeldBallPosition()
    {
        if (heldBall != null && holdPoint != null)
        {
            heldBall.transform.position = holdPoint.position;
            heldBall.transform.rotation = holdPoint.rotation;
        }
    }

    Ball FindNearestBall()
    {
        Ball[] balls = FindObjectsOfType<Ball>();
        if (balls == null || balls.Length == 0) return null;

        Transform me = transform;
        float bestDist = float.MaxValue;
        Ball best = null;

        foreach (var b in balls)
        {
            if (!b) continue;
            var rb = b.GetComponent<Rigidbody>();
            if (!rb) continue;

            float d = Vector3.Distance(me.position + Vector3.up * 1.0f, b.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = b;
            }
        }
        return best;
    }

    void UpdateChargeUI()
    {
        if (!chargeUI || !chargeFillImage)
            return;

        float targetAlpha = 0f;
        float fill = 0f;

        if (heldBall != null && isCharging)
        {
            float denom = Mathf.Max(0.01f, maxThrowForce - minThrowForce);
            fill = Mathf.Clamp01((currentThrowForce - minThrowForce) / denom);
            targetAlpha = 1f;
        }

        chargeFillImage.fillAmount = fill;
        chargeUI.alpha = Mathf.MoveTowards(chargeUI.alpha, targetAlpha, uiFadeSpeed * Time.deltaTime);
        chargeUI.interactable = false;
        chargeUI.blocksRaycasts = false;
    }

    // ---------- UI Hint (optional) ----------

    void OnGUI()
    {
        if (heldBall != null || nearestBall == null || !cam) return;

        float dist = Vector3.Distance(transform.position + Vector3.up, nearestBall.transform.position);
        if (dist > pickupRange) return;

        Vector3 screen = cam.WorldToScreenPoint(nearestBall.transform.position + Vector3.up * 0.35f);
        if (screen.z <= 0) return;

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(screen.x - 75, Screen.height - screen.y - 20, 150, 40), "Press E to pick up", style);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, pickupRange);
    }
}
