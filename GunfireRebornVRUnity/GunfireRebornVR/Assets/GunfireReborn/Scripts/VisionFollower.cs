using UnityEngine;

public class VisionFollower : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distance = 3.0f;

    private bool isCentered = false;

    // Built-in Unity *Magic*
    private void OnBecameInvisible()
    {
        isCentered = false;
    }

    private void Update()
    {
        if (!isCentered)
        {
            // Find the position we need to be at
            Vector3 targetPosition = FindTargetPosition();

            // Move just a little bit at a time
            MoveTowards(targetPosition);

            // If we've reached the position, don't do anymore work
            if (ReachedPosition(targetPosition))
                isCentered = true;
        }
    }

    private Vector3 FindTargetPosition()
    {
        // Let's get a position infront of the player's camera
        return cameraTransform.position + (cameraTransform.forward * distance);
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        // Instead of a tween, that would need to be constantly restarted
        transform.position += (targetPosition - transform.position) * 0.025f;
    }

    private bool ReachedPosition(Vector3 targetPosition)
    {
        // Simple distance check, can be replaced if you wish
        return Vector3.Distance(targetPosition, transform.position) < 0.1f;
    }
}