using UnityEngine;

public class BallMovement : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // This method is called from the RollerBallAgent script
    public void ApplyForce(Vector3 force)
    {
        rb.AddForce(force);
    }
}
