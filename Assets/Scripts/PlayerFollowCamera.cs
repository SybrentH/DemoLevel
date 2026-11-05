using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CubeFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 7f, -9f);
    public float smoothTime = 0.15f;
    public float lookHeight = 0.5f;

    private Vector3 followVelocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CubeFollowCamera: sleep de speler in Target.", this);
            return;
        }

        transform.position = target.position + offset;
        LookAtTarget();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            Mathf.Max(0.01f, smoothTime));

        LookAtTarget();
    }

    void LookAtTarget()
    {
        Vector3 direction = target.position + Vector3.up * lookHeight - transform.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }

    void OnDisable()
    {
        followVelocity = Vector3.zero;
    }
}