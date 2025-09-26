
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform defaultTarget; // Wheelchair
    public Transform playerTarget;  // PlayerCad

    [Header("Follow Settings")]
    public Vector3 wheelchairOffset = new Vector3(0f, 2f, -5f);
    public Vector3 playerOffset = new Vector3(0f, 1.5f, -3f);
    public float smoothSpeed = 5f;
    public float lookAtHeight = 1.5f;

    private Transform currentTarget;
    private Vector3 currentOffset;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        currentTarget = defaultTarget;
        currentOffset = wheelchairOffset;
        ResetCamera();
    }

    void LateUpdate()
    {
        if (!currentTarget) return;

        Vector3 targetPos = currentTarget.position - 
                            currentTarget.forward * currentOffset.z + 
                            Vector3.up * currentOffset.y;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(currentTarget.position + Vector3.up * lookAtHeight);
    }

    public void SwitchToPlayerFocus()
    {
        if (!playerTarget) return;
        currentTarget = playerTarget;
        currentOffset = playerOffset;
        smoothSpeed = 3f;
    }

    public void ReturnToWheelchair()
    {
        if (!defaultTarget) return;
        currentTarget = defaultTarget;
        currentOffset = wheelchairOffset;
        smoothSpeed = 5f;
    }

    public void ResetCamera()
    {
        if (!defaultTarget) return;

        Vector3 startPos = defaultTarget.position - 
                           defaultTarget.forward * wheelchairOffset.z + 
                           Vector3.up * wheelchairOffset.y;

        transform.position = startPos;
        transform.LookAt(defaultTarget.position + Vector3.up * lookAtHeight);
    }
}