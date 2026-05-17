using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float smoothSpeed = 5f;

    Transform _target;

    void LateUpdate()
    {
        if (_target == null)
        {
            var pw = FindFirstObjectByType<PlayerWarrior>();
            if (pw != null) _target = pw.transform;
            else return;
        }

        Vector3 desired = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
