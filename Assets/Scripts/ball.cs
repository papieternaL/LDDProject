using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void ResetBall()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
    }
}