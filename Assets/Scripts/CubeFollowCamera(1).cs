using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CubeFollowCamera : MonoBehaviour
{
    public Transform target;
    [Tooltip("Position relative to the cube's facing direction.")]
    public Vector3 offset = new Vector3(0f, 7f, -9f);
    [Min(0.01f)] public float smoothTime = 0.15f;
    public float lookHeight = 0.5f;
    public bool rotateWithTarget = true;
    private Vector3 followVelocity;

    private Vector3 DesiredPosition()
    {
        Quaternion rotation = rotateWithTarget
            ? Quaternion.Euler(0f, target.eulerAngles.y, 0f)
            : Quaternion.identity;
        return target.position + rotation * offset;
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CubeFollowCamera: assign the cube to Target.", this);
            return;
        }
        transform.position = DesiredPosition();
        LookAtTarget();
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.SmoothDamp(transform.position,
            DesiredPosition(), ref followVelocity, Mathf.Max(0.01f, smoothTime));
        LookAtTarget();
    }

    private void LookAtTarget()
    {
        Vector3 direction = target.position + Vector3.up * lookHeight - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void OnDisable() => followVelocity = Vector3.zero;
}
